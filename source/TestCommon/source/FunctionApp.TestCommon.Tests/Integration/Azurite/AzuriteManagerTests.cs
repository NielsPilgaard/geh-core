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
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Azurite;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Tests.Fixtures;
using Energinet.DataHub.Core.TestCommon.Diagnostics;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Energinet.DataHub.Core.FunctionApp.TestCommon.Tests.Integration.Azurite
{
    public class AzuriteManagerTests
    {
        [Collection(nameof(AzuriteCollectionFixture))]
        public class VerifyAzuriteCanBeStartedTwice
        {
            [Fact]
            public async Task When_AzuriteProcessIsDisposed_Then_ItCanStartAgain()
            {
                // Arrange
                var azuriteManagerToStartFirst = new AzuriteManager();
                try
                {
                    azuriteManagerToStartFirst.StartAzurite();
                }
                finally
                {
                    // Act
                    azuriteManagerToStartFirst.Dispose();
                }

                // Assert
                var azuriteManagerToStartSecond = new AzuriteManager();
                try
                {
                    azuriteManagerToStartSecond.StartAzurite();
                    var exception = await Record.ExceptionAsync(CreateStorageContainerAsync);
                    exception.Should().BeNull();
                }
                finally
                {
                    azuriteManagerToStartSecond.Dispose();
                }
            }

            [Fact]
            public void When_AzuriteProcessIsNotDisposed_Then_ItCanStillStartAgain()
            {
                // Arrange
                var azuriteManagerToStartFirst = new AzuriteManager();
                var azuriteManagerToStartSecond = new AzuriteManager();
                try
                {
                    azuriteManagerToStartFirst.StartAzurite();

                    // Act
                    var exception = Record.Exception(() => azuriteManagerToStartSecond.StartAzurite());

                    // Assert
                    exception.Should().BeNull();
                }
                finally
                {
                    azuriteManagerToStartFirst.Dispose();
                    azuriteManagerToStartSecond.Dispose();
                }
            }

            private static async Task CreateStorageContainerAsync()
            {
                var storageConnectionString = "UseDevelopmentStorage=true";
                var containerName = $"Test{Guid.NewGuid()}".ToLower();

                var blobServiceClient = new BlobServiceClient(storageConnectionString);
                var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);
                await blobContainerClient.CreateAsync();
            }
        }

        /// <summary>
        /// When using Azurite with OAuth we must use Https and 'localhost' (not '127.0.0.1').
        /// </summary>
        [Collection(nameof(AzuriteCollectionFixture))]
        public sealed class Given_OAuthIsTrue : IDisposable
        {
            public Given_OAuthIsTrue()
            {
                AzuriteManager = new AzuriteManager(useOAuth: true);
                AzuriteManager.StartAzurite();
            }

            private AzuriteManager AzuriteManager { get; }

            public void Dispose()
            {
                AzuriteManager.Dispose();
            }

            [Fact]
            public async Task When_BlobServiceClient_UseDevelopmentStorageShortcut_Then_CreateContainerShouldFail()
            {
                // Arrange
                var client = new BlobServiceClient(
                    connectionString: "UseDevelopmentStorage=true",
                    CreateBlobNoRetryOptions());

                // Act
                var exception = await Record.ExceptionAsync(() => CreateBlobContainerAsync(client));

                // Assert
                exception.Should().NotBeNull();
            }

            [Fact]
            public async Task When_BlobServiceClient_UsingConnectionString_Then_CanCreateContainer()
            {
                // Arrange
                var client = new BlobServiceClient(
                    connectionString: AzuriteManager.BlobStorageConnectionString,
                    CreateBlobNoRetryOptions());

                // Act
                var exception = await Record.ExceptionAsync(() => CreateBlobContainerAsync(client));

                // Assert
                exception.Should().BeNull();
            }

            [Fact]
            public async Task When_BlobServiceClient_UsingHttpsAndTokenCredential_Then_CanCreateContainer()
            {
                // Arrange
                var client = new BlobServiceClient(
                    serviceUri: AzuriteManager.BlobStorageServiceUri,
                    credential: new DefaultAzureCredential(),
                    CreateBlobNoRetryOptions());

                // Act
                var exception = await Record.ExceptionAsync(() => CreateBlobContainerAsync(client));

                // Assert
                exception.Should().BeNull();
            }

            [Fact]
            public async Task When_QueueServiceClient_UseDevelopmentStorageShortcut_Then_CreateContainerShouldFail()
            {
                // Arrange
                var client = new QueueServiceClient(
                    connectionString: "UseDevelopmentStorage=true",
                    CreateQueueNoRetryOptions());

                // Act
                var exception = await Record.ExceptionAsync(() => CreateQueueAsync(client));

                // Assert
                exception.Should().NotBeNull();
            }

            [Fact]
            public async Task When_QueueServiceClient_UsingConnectionString_Then_CanCreateContainer()
            {
                // Arrange
                var client = new QueueServiceClient(
                    connectionString: AzuriteManager.BlobStorageConnectionString,
                    CreateQueueNoRetryOptions());

                // Act
                var exception = await Record.ExceptionAsync(() => CreateQueueAsync(client));

                // Assert
                exception.Should().BeNull();
            }

            [Fact]
            public async Task When_QueueServiceClient_UsingHttpsAndTokenCredential_Then_CanCreateContainer()
            {
                // Arrange
                var client = new QueueServiceClient(
                    serviceUri: AzuriteManager.BlobStorageServiceUri,
                    credential: new DefaultAzureCredential(),
                    CreateQueueNoRetryOptions());

                // Act
                var exception = await Record.ExceptionAsync(() => CreateQueueAsync(client));

                // Assert
                exception.Should().BeNull();
            }
        }

        [Collection(nameof(AzuriteCollectionFixture))]
        public sealed class Given_OAuthIsFalse : IDisposable
        {
            public Given_OAuthIsFalse()
            {
                AzuriteManager = new AzuriteManager();
                AzuriteManager.StartAzurite();
            }

            private AzuriteManager AzuriteManager { get; }

            public void Dispose()
            {
                AzuriteManager.Dispose();
            }

            [Fact]
            public async Task When_BlobServiceClient_UseDevelopmentStorageShortcut_Then_CanCreateContainer()
            {
                // Arrange
                var client = new BlobServiceClient(
                    connectionString: "UseDevelopmentStorage=true",
                    CreateBlobNoRetryOptions());

                // Act
                var exception = await Record.ExceptionAsync(() => CreateBlobContainerAsync(client));

                // Assert
                exception.Should().BeNull();
            }

            [Fact]
            public async Task When_BlobServiceClient_UsingConnectionString_Then_CanCreateContainer()
            {
                // Arrange
                var client = new BlobServiceClient(
                    connectionString: AzuriteManager.BlobStorageConnectionString,
                    CreateBlobNoRetryOptions());

                // Act
                var exception = await Record.ExceptionAsync(() => CreateBlobContainerAsync(client));

                // Assert
                exception.Should().BeNull();
            }

            [Fact]
            public async Task When_QueueServiceClient_UseDevelopmentStorageShortcut_Then_CanCreateContainer()
            {
                // Arrange
                var client = new QueueServiceClient(
                    connectionString: "UseDevelopmentStorage=true",
                    CreateQueueNoRetryOptions());

                // Act
                var exception = await Record.ExceptionAsync(() => CreateQueueAsync(client));

                // Assert
                exception.Should().BeNull();
            }

            [Fact]
            public async Task When_QueueServiceClient_UsingConnectionString_Then_CanCreateContainer()
            {
                // Arrange
                var client = new QueueServiceClient(
                    connectionString: AzuriteManager.BlobStorageConnectionString,
                    CreateQueueNoRetryOptions());

                // Act
                var exception = await Record.ExceptionAsync(() => CreateQueueAsync(client));

                // Assert
                exception.Should().BeNull();
            }
        }

        private static BlobClientOptions CreateBlobNoRetryOptions()
        {
            var retryOptions = new BlobClientOptions();
            retryOptions.Retry.MaxRetries = 0;

            return retryOptions;
        }

        private static Task CreateBlobContainerAsync(BlobServiceClient blobServiceClient)
        {
            var containerName = $"Test{Guid.NewGuid()}".ToLower();

            var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);
            return blobContainerClient.CreateAsync();
        }

        private static QueueClientOptions CreateQueueNoRetryOptions()
        {
            var retryOptions = new QueueClientOptions();
            retryOptions.Retry.MaxRetries = 0;

            return retryOptions;
        }

        private static Task CreateQueueAsync(QueueServiceClient queueServiceClient)
        {
            var queueName = $"Test{Guid.NewGuid()}".ToLower();

            var queueClient = queueServiceClient.GetQueueClient(queueName);
            return queueClient.CreateAsync();
        }
    }
}
