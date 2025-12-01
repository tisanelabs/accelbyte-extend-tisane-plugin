// Copyright (c) 2024 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;

using System.Threading.Tasks;
using System.Net.Http;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

using Grpc.Core;
using AccelByte.ProfanityFilter.Registered.V1;
using ProfanityFilterLib = ProfanityFilter.ProfanityFilter;

namespace AccelByte.Extend.Tisane.Plugin.Services
{
    public class ProfanityFilterServiceImpl : ProfanityFilterService.ProfanityFilterServiceBase
    {
        private readonly ILogger<ProfanityFilterServiceImpl> _Logger;
        private readonly ITisaneService _tisaneService;

        private ProfanityFilterLib _Filter;

        public ProfanityFilterServiceImpl(ILogger<ProfanityFilterServiceImpl> logger, ITisaneService tisaneService)
        {
            _Logger = logger;
            _tisaneService = tisaneService;
            _Filter = new ProfanityFilterLib(new []{ "bad", "ibad", "yourbad" });
        }

        public override async Task<ExtendProfanityValidationResponse> Validate(ExtendProfanityValidationRequest request, ServerCallContext context)
        {
            _Logger.LogInformation("REQUEST /accelbyte.profanityfilter.registered.v1.ProfanityFilterService/Validate");
            _Logger.LogInformation("Request Value: {Value}", request.Value);

            var result = await _tisaneService.ParseAsync(request.Value);

            return new ExtendProfanityValidationResponse()
            {
                IsProfane = result.IsProfane,
                Message = result.Message
            };
        }
    }
}