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

namespace Energinet.DataHub.Core.FeatureManagement.SampleApp.Common
{
    /// <summary>
    /// Feature flags container, to avoid using magic strings for flags.
    /// </summary>
    public static class FeatureFlags
    {
        /// <summary>
        /// On Windows it is also possible to use ":" as separator, but "__" is supported on multiple platforms.
        /// </summary>
        public const string ConfigurationPrefix = "FeatureManagement__";

        /// <summary>
        /// Current feature flags.
        /// </summary>
        public enum Names
        {
            UseGuidMessage,
        }
    }
}
