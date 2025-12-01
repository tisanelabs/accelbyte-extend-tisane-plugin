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
        private readonly ITisaneService _tisaneService;

        public PersonalChatSentService(ILogger<PersonalChatSentService> logger, ITisaneService tisaneService)
        {
            _Logger = logger;
            _tisaneService = tisaneService;
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
                    var result = await _tisaneService.ParseAsync(payload.Payload);
                    if (result.IsProfane)
                    {
                         _Logger.LogWarning("⛔ Message flagged as profane by Tisane: {Message}", result.Message);
                    }
                    else
                    {
                        _Logger.LogInformation("✅ Tisane API call successful! Message is clean.");
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
