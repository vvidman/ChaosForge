/*
   Copyright 2026 Viktor Vidman (vvidman)

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using System.Text.Json;
using ChaosForge.Domain.Enums;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using HttpJsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;

namespace ChaosForge.Infrastructure.Tests;

public sealed class DependencyInjectionTests
{
    [Fact]
    public void AddInfrastructure_HttpJsonOptions_SerializesEnumAsName()
    {
        // Arrange
        var options = BuildHttpSerializerOptions();

        // Act
        var json = JsonSerializer.Serialize(ProjectStatus.RequirementsPhase, options);

        // Assert
        json.Should().Be("\"RequirementsPhase\"");
    }

    [Theory]
    [InlineData("\"BusinessAnalyst\"")]
    [InlineData("\"businessAnalyst\"")]
    [InlineData("0")]
    public void AddInfrastructure_HttpJsonOptions_DeserializesEnumFromNameOrNumber(string json)
    {
        // Arrange
        var options = BuildHttpSerializerOptions();

        // Act
        var role = JsonSerializer.Deserialize<AgentRole>(json, options);

        // Assert
        role.Should().Be(AgentRole.BusinessAnalyst);
    }

    private static JsonSerializerOptions BuildHttpSerializerOptions()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Data Source=:memory:",
                ["InferRouter:BaseUrl"] = "http://localhost:5100",
            })
            .Build();

        var services = new ServiceCollection();
        services.AddInfrastructure(configuration);

        using var provider = services.BuildServiceProvider();

        return provider.GetRequiredService<IOptions<HttpJsonOptions>>().Value.SerializerOptions;
    }
}
