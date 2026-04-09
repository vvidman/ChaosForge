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

using ChaosForge.Application.UseCases.Commands;
using FluentValidation.TestHelper;

namespace ChaosForge.Application.Tests.UseCases.Commands;

public sealed class CreateUseCaseCommandValidatorTests
{
    private readonly CreateUseCaseCommandValidator _validator = new();

    [Fact]
    public void Validate_WithEmptyProjectId_Fails()
    {
        // Arrange
        var command = new CreateUseCaseCommand(Guid.Empty, "Title", "Description", 0);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ProjectId);
    }

    [Fact]
    public void Validate_WithEmptyTitle_Fails()
    {
        // Arrange
        var command = new CreateUseCaseCommand(Guid.NewGuid(), "", "Description", 0);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void Validate_WithEmptyDescription_Fails()
    {
        // Arrange
        var command = new CreateUseCaseCommand(Guid.NewGuid(), "Title", "", 0);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Validate_WithNegativePriority_Fails()
    {
        // Arrange
        var command = new CreateUseCaseCommand(Guid.NewGuid(), "Title", "Description", -1);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Priority);
    }

    [Fact]
    public void Validate_WithValidCommand_Passes()
    {
        // Arrange
        var command = new CreateUseCaseCommand(Guid.NewGuid(), "Title", "Description", 0);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
