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

using ChaosForge.Application.WorkTasks.Commands;
using FluentValidation.TestHelper;

namespace ChaosForge.Application.Tests.WorkTasks.Commands;

public sealed class CreateWorkTaskCommandValidatorTests
{
    private readonly CreateWorkTaskCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_Passes()
    {
        // Arrange
        var command = new CreateWorkTaskCommand(Guid.NewGuid(), "Title", "Description", 1);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptySRSId_Fails()
    {
        // Arrange
        var command = new CreateWorkTaskCommand(Guid.Empty, "Title", "Description", 1);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.SRSId)
            .WithErrorMessage("SRSId must not be empty.");
    }

    [Fact]
    public void Validate_WithEmptyTitle_Fails()
    {
        // Arrange
        var command = new CreateWorkTaskCommand(Guid.NewGuid(), "", "Description", 1);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Title must not be empty.");
    }

    [Fact]
    public void Validate_WithEmptyDescription_Fails()
    {
        // Arrange
        var command = new CreateWorkTaskCommand(Guid.NewGuid(), "Title", "", 1);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage("Description must not be empty.");
    }

    [Fact]
    public void Validate_WithStoryPointsLessThanOne_Fails()
    {
        // Arrange
        var command = new CreateWorkTaskCommand(Guid.NewGuid(), "Title", "Description", 0);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.StoryPoints)
            .WithErrorMessage("StoryPoints must be at least 1.");
    }
}
