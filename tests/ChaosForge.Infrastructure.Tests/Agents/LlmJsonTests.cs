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
using ChaosForge.Infrastructure.Agents;
using FluentAssertions;

namespace ChaosForge.Infrastructure.Tests.Agents;

public sealed class LlmJsonTests
{
    private static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true };

    [Theory]
    [InlineData("""{"a":1}""")]
    [InlineData("```json\n{\"a\":1}\n```")]
    [InlineData("```\n{\"a\":1}\n```")]
    [InlineData("Here is the plan:\n{\"a\":1}\nLet me know if you need changes.")]
    public void Extract_WithFenceOrSurroundingProse_ReturnsJsonPayload(string raw)
    {
        // Act
        var json = LlmJson.Extract(raw);

        // Assert
        json.Should().Be("""{"a":1}""");
    }

    [Fact]
    public void Extract_WithoutJson_ReturnsTrimmedInput()
    {
        // Act
        var json = LlmJson.Extract("  no json here  ");

        // Assert
        json.Should().Be("no json here");
    }

    [Theory]
    [InlineData("""[{"title":"A"},{"title":"B"}]""")]
    [InlineData("```json\n[{\"title\":\"A\"},{\"title\":\"B\"}]\n```")]
    [InlineData("""{"tasks":[{"title":"A"},{"title":"B"}]}""")]
    public void DeserializeList_WithArrayFencedArrayOrWrappedArray_ReturnsItems(string raw)
    {
        // Act
        var items = LlmJson.DeserializeList<Item>(raw, Options);

        // Assert
        items.Select(i => i.Title).Should().Equal("A", "B");
    }

    [Theory]
    [InlineData("this is not valid json {{ broken")]
    [InlineData("""{"title":"A"}""")]
    public void DeserializeList_WithInvalidJsonOrNoArray_ThrowsJsonException(string raw)
    {
        // Act
        var act = () => LlmJson.DeserializeList<Item>(raw, Options);

        // Assert
        act.Should().Throw<JsonException>();
    }

    private sealed record Item(string Title);
}
