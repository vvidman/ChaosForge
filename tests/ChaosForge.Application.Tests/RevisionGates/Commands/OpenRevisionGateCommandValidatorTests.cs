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

using ChaosForge.Application.RevisionGates.Commands;
using ChaosForge.Domain.Enums;
using FluentValidation.TestHelper;

namespace ChaosForge.Application.Tests.RevisionGates.Commands;

public sealed class OpenRevisionGateCommandValidatorTests
{
    private readonly OpenRevisionGateCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_Passes()
    {
        // Arrange
        var command = new OpenRevisionGateCommand(Guid.NewGuid(), RevisionGateType.Requirements, "Agent output here.");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyProjectId_Fails()
    {
        // Arrange
        var command = new OpenRevisionGateCommand(Guid.Empty, RevisionGateType.Requirements, "Agent output here.");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ProjectId)
            .WithErrorMessage("ProjectId must not be empty.");
    }

    [Fact]
    public void Validate_WithEmptyAgentOutput_Fails()
    {
        // Arrange
        var command = new OpenRevisionGateCommand(Guid.NewGuid(), RevisionGateType.Requirements, "");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.AgentOutput)
            .WithErrorMessage("AgentOutput must not be empty.");
    }
}
