﻿// Copyright 2020 Energinet DataHub A/S
//
// Licensed under the Apache License, Version 2.0 (the "License2");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Energinet.DataHub.Core.FunctionApp.Common.Abstractions.Actor;
using Energinet.DataHub.Core.FunctionApp.Common.Abstractions.ServiceBus;
using Energinet.DataHub.Core.FunctionApp.Common.Extensions;
using Energinet.DataHub.Core.FunctionApp.Common.Middleware.Helpers;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.Core.FunctionApp.Common.Middleware
{
    public sealed class ServiceBusActorContextMiddleware : IFunctionsWorkerMiddleware
    {
        private readonly ILogger _logger;
        private readonly IActorContext _actorContext;

        public ServiceBusActorContextMiddleware(
            ILogger logger,
            IActorContext actorContext)
        {
            _logger = logger;
            _actorContext = actorContext;
        }

        public async Task Invoke(FunctionContext context, [NotNull] FunctionExecutionDelegate next)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var httpRequestData = context.GetHttpRequestData();
            if (httpRequestData != null)
            {
                await next(context).ConfigureAwait(false);
                return;
            }

            if (context.BindingContext.BindingData.TryGetValue("UserProperties", out var userPropertiesObject) && userPropertiesObject != null)
            {
                _actorContext.CurrentActor = ServiceBusActorParser.FromDictionaryString(userPropertiesObject as string ?? string.Empty, Constants.ServiceBusIdentityKey);
            }
            else
            {
                _logger.LogWarning("UserIdentity not found for invocation: {invocationId}", context.InvocationId);
                throw new InvalidOperationException();
            }

            await next(context).ConfigureAwait(false);
        }
    }
}
