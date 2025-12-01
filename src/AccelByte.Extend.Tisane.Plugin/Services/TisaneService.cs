using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace AccelByte.Extend.Tisane.Plugin.Services
{
    public class TisaneService : ITisaneService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<TisaneService> _logger;

        public TisaneService(IHttpClientFactory httpClientFactory, ILogger<TisaneService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<(bool IsProfane, string Message)> ParseAsync(string content)
        {
            bool isProfane = false;
            string message = "";

            if (string.IsNullOrWhiteSpace(content))
            {
                return (isProfane, message);
            }

            try
            {
                _logger.LogInformation("--- Calling Tisane API directly ---");

                using var httpClient = _httpClientFactory.CreateClient();

                string language = Environment.GetEnvironmentVariable("TISANE_LANGUAGE_CODES") ?? "en";
                string format = Environment.GetEnvironmentVariable("TISANE_SETTING_FORMAT") ?? "dialogue";
                string parseURL = Environment.GetEnvironmentVariable("TISANE_PARSE_URL") ?? "https://api.tisane.ai/parse";

                bool banCompetitiveLanguage = false;
                string? banCompLangEnv = Environment.GetEnvironmentVariable("TISANE_BAN_COMPETITIVE_LANGUAGE");
                if (!string.IsNullOrEmpty(banCompLangEnv))
                {
                    if (bool.TryParse(banCompLangEnv, out bool result))
                    {
                        banCompetitiveLanguage = result;
                    }
                }

                object settings;
                if (banCompetitiveLanguage)
                {
                    settings = new { format = format };
                }
                else
                {
                    settings = new { format = format, memory = new { flags = new[] { "game_violence_ok" } } };
                }

                var settingsJson = JsonSerializer.Serialize(settings);
                _logger.LogInformation("Tisane Settings: {Settings}", settingsJson);

                // Create the request body
                var requestBody = new
                {
                    language = language,
                    content = content,
                    settings = settings
                };

                var jsonBody = JsonSerializer.Serialize(requestBody);
                _logger.LogInformation("Request Body: {Body}", jsonBody);

                // Create the HTTP request
                var httpRequest = new HttpRequestMessage(HttpMethod.Post, parseURL);
                string apiKey = Environment.GetEnvironmentVariable("TISANE_API_KEY") ?? string.Empty;
                httpRequest.Headers.Add("Ocp-Apim-Subscription-Key", apiKey);
                httpRequest.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                _logger.LogInformation("Sending request to Tisane API...");
                var response = await httpClient.SendAsync(httpRequest);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("Tisane API Response Status: {StatusCode}", response.StatusCode);
                _logger.LogInformation("Tisane API Response Content: {Content}", responseContent);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("✅ Tisane API call successful!");

                    // Parse response to check for profanity
                    using var doc = JsonDocument.Parse(responseContent);
                    if (doc.RootElement.TryGetProperty("abuse", out var abuseElement) && abuseElement.GetArrayLength() > 0)
                    {
                        var sbMessage = new StringBuilder();

                        foreach (var abuseItem in abuseElement.EnumerateArray())
                        {
                            string type = abuseItem.GetProperty("type").GetString() ?? "";
                            bool isMatch = false;
                            string? typeMessage = null;

                            switch (type)
                            {
                                case "personal_attack":
                                    isMatch = true;
                                    typeMessage = Environment.GetEnvironmentVariable("PERSONAL_ATTACK_MESSAGE") ?? "Insult detected";
                                    break;
                                case "bigotry":
                                    typeMessage = Environment.GetEnvironmentVariable("BIGOTRY_MESSAGE");
                                    isMatch = !string.IsNullOrEmpty(typeMessage);
                                    break;
                                case "external_contact":
                                    typeMessage = Environment.GetEnvironmentVariable("EXTERNAL_CONTACT_MESSAGE");
                                    isMatch = !string.IsNullOrEmpty(typeMessage);
                                    break;
                                case "profanity":
                                    typeMessage = Environment.GetEnvironmentVariable("PROFANITY_MESSAGE");
                                    isMatch = !string.IsNullOrEmpty(typeMessage);
                                    break;
                                case "sexual_advances":
                                    typeMessage = Environment.GetEnvironmentVariable("SEXUAL_ADVANCES_MESSAGE");
                                    isMatch = !string.IsNullOrEmpty(typeMessage);
                                    break;
                                case "adult_only":
                                    typeMessage = Environment.GetEnvironmentVariable("ADULT_ONLY_MESSAGE");
                                    isMatch = !string.IsNullOrEmpty(typeMessage);
                                    break;
                                case "data_leak":
                                    typeMessage = Environment.GetEnvironmentVariable("DATA_LEAK_MESSAGE");
                                    isMatch = !string.IsNullOrEmpty(typeMessage);
                                    break;
                            }

                            // Check for threat in tags if not already matched
                            if (!isMatch && abuseItem.TryGetProperty("tags", out var tagsElement) && tagsElement.ValueKind == JsonValueKind.Array)
                            {
                                foreach (var tag in tagsElement.EnumerateArray())
                                {
                                    if (tag.GetString() == "threat")
                                    {
                                        string? threatMsg = Environment.GetEnvironmentVariable("THREAT_MESSAGE");
                                        if (!string.IsNullOrEmpty(threatMsg))
                                        {
                                            isMatch = true;
                                            typeMessage = threatMsg;
                                            break;
                                        }
                                    }
                                }
                            }

                            if (isMatch && !string.IsNullOrEmpty(typeMessage))
                            {
                                isProfane = true;
                                if (sbMessage.Length > 0) sbMessage.Append(" ");
                                sbMessage.Append(typeMessage);
                            }
                        }

                        if (isProfane)
                        {
                            message = sbMessage.ToString();
                            _logger.LogWarning("⛔ Message flagged as profane by Tisane: {Message}", message);
                        }
                    }
                }
                else
                {
                    _logger.LogWarning("⚠️  Tisane API call failed with status: {StatusCode}", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Tisane API: {Message}", ex.Message);
            }

            return (isProfane, message);
        }
    }
}
