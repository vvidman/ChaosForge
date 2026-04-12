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

using ChaosForge.Domain.Entities;
using ChaosForge.Domain.Enums;
using ChaosForge.Infrastructure.Agents;
using FluentAssertions;

namespace ChaosForge.Infrastructure.Tests.Agents;

public sealed class AgentPromptBuilderTests
{
    private const string BasePrompt = "Write a requirements document.";

    [Fact]
    public void BuildWithPriorAttempt_WhenPriorAttemptIsNull_ReturnsBasePromptUnchanged()
    {
        // Act
        var result = AgentPromptBuilder.BuildWithPriorAttempt(BasePrompt, priorAttempt: null);

        // Assert
        result.Should().Be(BasePrompt);
    }

    [Fact]
    public void BuildWithPriorAttempt_WhenPriorAttemptExists_AppendsRejectionContext()
    {
        // Arrange
        var attempt = CreateRejectedReviewAttempt(output: "Draft v1", reviewNote: "Too vague.");

        // Act
        var result = AgentPromptBuilder.BuildWithPriorAttempt(BasePrompt, attempt);

        // Assert
        result.Should().StartWith(BasePrompt);
        result.Should().Contain("Draft v1");
        result.Should().Contain("Too vague.");
        result.Should().Contain("Previous Attempt (Rejected)");
    }

    [Fact]
    public void BuildWithPriorAttempt_WhenPriorAttemptHasNullReviewNote_HandlesGracefully()
    {
        // Arrange — a completed but not-yet-rejected attempt (no ReviewNote set)
        var attempt = CreateCompletedAttempt(output: "Draft v1");

        // Act
        var result = AgentPromptBuilder.BuildWithPriorAttempt(BasePrompt, attempt);

        // Assert
        result.Should().Contain("(no rejection note provided)");
    }

    // Helpers — construct domain entities via their public API

    private static TaskAttempt CreateRejectedReviewAttempt(string output, string reviewNote)
    {
        var attempt = new TaskAttempt(Guid.NewGuid(), Guid.NewGuid(), AttemptType.Review);
        attempt.Complete(output);
        attempt.Reject(reviewNote);

        return attempt;
    }

    private static TaskAttempt CreateCompletedAttempt(string output)
    {
        var attempt = new TaskAttempt(Guid.NewGuid(), Guid.NewGuid(), AttemptType.Implementation);
        attempt.Complete(output);

        return attempt;
    }
}
