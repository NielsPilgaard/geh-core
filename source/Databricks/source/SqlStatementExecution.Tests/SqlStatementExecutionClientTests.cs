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

using System.Net;
using System.Text;
using AutoFixture.Xunit2;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.AppSettings;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Serialization;
using Energinet.DataHub.Core.Databricks.SqlStatementExecutionTests.Assets;
using Energinet.DataHub.Core.Databricks.SqlStatementExecutionTests.Helpers;
using Moq;

namespace Energinet.DataHub.Core.Databricks.SqlStatementExecutionTests
{
    public class SqlStatementExecutionClientTests
    {
        [Theory]
        [AutoData]
        public async Task GetAsync_WhenCalled_ReturnsTimeSeries(
            TestFiles testFiles,
            JsonSerializer jsonSerializer,
            DatabricksSqlResponseParser databricksSqlResponseParser,
            Mock<IHttpClientFactory> httpClientFactory)
        {
            // Arrange
            var databricksOptions = new DatabricksOptions
            {
                Instance = "abc-1234567890123456.78",
                Endpoint = "azuredatabricks.net/sql/1.0/warehouses/",
                WarehouseId = "ab123456a7bc89d0",
            };
            var fakeHandler = new FakeHttpMessageHandler((request, _) =>
            {
                var response = request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(testFiles.TimeSeriesResponse, Encoding.UTF8, "application/json");
                return Task.FromResult(response);
            });
            httpClientFactory
                .Setup(hcf => hcf.CreateClient(It.IsAny<string>()))
                .Returns(() => new HttpClient(fakeHandler) { BaseAddress = new Uri("https://12345.azuredatabricks.net") });
            var sut = new SqlStatementExecutionClient(httpClientFactory.Object, jsonSerializer, databricksOptions, databricksSqlResponseParser);

            // Act
            var result = await sut.SendSqlStatementAsync("sql query");

            // Assert
            result.Table.RowCount.Should().Be(13);
        }
    }
}
