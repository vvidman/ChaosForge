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

using ChaosForge.Application.SRS.Commands;
using FluentValidation.TestHelper;

namespace ChaosForge.Application.Tests.SRS.Commands;

public sealed class ApplyHumanEditToSRSCommandValidatorTests
{
    private readonly ApplyHumanEditToSRSCommandValidator _validator = new();

    [Fact]
    public void Validate_WithEmptySRSId_Fails()
    {
        // Arrange
        var command = new ApplyHumanEditToSRSCommand(Guid.Empty, "Description", "Note");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.SRSId);
    }

    [Fact]
    public void Validate_WithEmptyEditedDescription_Fails()
    {
        // Arrange
        var command = new ApplyHumanEditToSRSCommand(Guid.NewGuid(), "", "Note");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.EditedDescription);
    }

    [Fact]
    public void Validate_WithEmptyNote_Fails()
    {
        // Arrange
        var command = new ApplyHumanEditToSRSCommand(Guid.NewGuid(), "Description", "");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Note);
    }

    [Fact]
    public void Validate_WithValidCommand_Passes()
    {
        // Arrange
        var command = new ApplyHumanEditToSRSCommand(Guid.NewGuid(), "Description", "Note");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
