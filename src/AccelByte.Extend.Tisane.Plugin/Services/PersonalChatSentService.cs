using System;
using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Grpc.Core;
using Google.Protobuf.WellKnownTypes;

using AccelByte.Chat.Chat;

namespace AccelByte.Extend.Tisane.Plugin.Services
{
    public class PersonalChatSentService : PersonalChatPersonalChatSentService.PersonalChatPersonalChatSentServiceBase
    {
        private readonly ILogger<PersonalChatSentService> _Logger;

        public PersonalChatSentService(ILogger<PersonalChatSentService> logger)
        {
            _Logger = logger;
        }

        public override async Task<Empty> OnMessage(PersonalChatSent request, ServerCallContext context)
        {
            _Logger.LogInformation("=== Chat Event Received ===");
            _Logger.LogInformation("Namespace: {Namespace}", request.Namespace);
            _Logger.LogInformation("UserId: {UserId}", request.UserId);

            var payload = request.Payload?.PersonalChatSent;
            if (payload != null)
            {
                _Logger.LogInformation("MessageId: {MessageId}", payload.MessageId);
                _Logger.LogInformation("SenderId: {SenderId}", payload.SenderId);
                _Logger.LogInformation("TargetId: {TargetId}", payload.TargetId);
                _Logger.LogInformation("TargetType: {TargetType}", payload.TargetType);
                _Logger.LogInformation("Message Content: {Payload}", payload.Payload);

                // Call Tisane to analyze the message content
                if (!string.IsNullOrWhiteSpace(payload.Payload))
                {
                    try
                    {
                        _Logger.LogInformation("--- Calling Tisane API directly ---");
                        
                        using var httpClient = new HttpClient();
                        
                        string language = Environment.GetEnvironmentVariable("TISANE_LANGUAGE_CODES") ?? "en";
                        string format = Environment.GetEnvironmentVariable("TISANE_SETTING_FORMAT") ?? "dialogue";
                        string parseURL = Environment.GetEnvironmentVariable("TISANE_PARSE_URL") ?? "https://api.tisane.ai/parse";
                        // Create the request body
                        var requestBody = new
                        {
                            language = language,
                            content = payload.Payload,
                            settings = new { format = format }
                        };
                        
                        var jsonBody = System.Text.Json.JsonSerializer.Serialize(requestBody);
                        _Logger.LogInformation("Request Body: {Body}", jsonBody);
                        
                        // Create the HTTP request
                        var httpRequest = new HttpRequestMessage(HttpMethod.Post, parseURL);
                        string apiKey = Environment.GetEnvironmentVariable("TISANE_API_KEY") ?? string.Empty;
                        httpRequest.Headers.Add("Ocp-Apim-Subscription-Key", apiKey);
                        httpRequest.Content = new StringContent(jsonBody, System.Text.Encoding.UTF8, "application/json");
                        
                        _Logger.LogInformation("Sending request to Tisane API...");
                        var response = await httpClient.SendAsync(httpRequest);
                        var responseContent = await response.Content.ReadAsStringAsync();
                        
                        _Logger.LogInformation("Tisane API Response Status: {StatusCode}", response.StatusCode);
                        _Logger.LogInformation("Tisane API Response Content: {Content}", responseContent);
                        
                        if (response.IsSuccessStatusCode)
                        {
                            _Logger.LogInformation("✅ Tisane API call successful!");
                        }
                        else
                        {
                            _Logger.LogWarning("⚠️  Tisane API call failed with status: {StatusCode}", response.StatusCode);
                        }
                    }
                    catch (Exception ex)
                    {
                        _Logger.LogError(ex, "Error calling Tisane API: {Message}", ex.Message);
                    }
                }
                else
                {
                    _Logger.LogInformation("Message content is empty, skipping Tisane API call");
                }
            }

            return new Empty();
        }
    }
}

