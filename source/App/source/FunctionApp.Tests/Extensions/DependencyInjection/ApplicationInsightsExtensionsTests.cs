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

using Energinet.DataHub.Core.App.Common.Extensibility.ApplicationInsights;
using Energinet.DataHub.Core.App.FunctionApp.Extensions.DependencyInjection;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Energinet.DataHub.Core.App.FunctionApp.Tests.Extensions.DependencyInjection;

public class ApplicationInsightsExtensionsTests
{
    private const string AnonymousSubsystem = "dummy";

    public ApplicationInsightsExtensionsTests()
    {
        Services = new ServiceCollection();
    }

    private ServiceCollection Services { get; }

    [Fact]
    public void AddApplicationInsightsForIsolatedWorker_WhenCalled_RegistrationsArePerformed()
    {
        // Act
        Services.AddApplicationInsightsForIsolatedWorker(AnonymousSubsystem);

        using var assertionScope = new AssertionScope();
        Services
            .Count(service => service.ServiceType == typeof(TelemetryClient))
            .Should().Be(1);
        Services
            .Count(service =>
                service.ServiceType == typeof(ITelemetryInitializer)
                && service.ImplementationInstance != null
                && service.ImplementationInstance.GetType() == typeof(SubsystemInitializer))
            .Should().Be(1);
    }
}
