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

using ChaosForge.Application.Common.Behaviors;
using FluentAssertions;
using FluentValidation;
using MediatR;

namespace ChaosForge.Application.Tests.Common.Behaviors;

public sealed class ValidationBehaviorTests
{
    public sealed record TestRequest(string? Value) : IRequest<string>;

    private sealed class RequiredValueValidator : AbstractValidator<TestRequest>
    {
        public RequiredValueValidator()
        {
            RuleFor(x => x.Value).NotEmpty().WithMessage("Value is required");
        }
    }

    private sealed class MinLengthValidator : AbstractValidator<TestRequest>
    {
        public MinLengthValidator()
        {
            RuleFor(x => x.Value).MinimumLength(5).WithMessage("Value is too short");
        }
    }

    private static RequestHandlerDelegate<string> NextReturning(string value) =>
        _ => Task.FromResult(value);

    [Fact]
    public async Task Handle_WhenNoValidators_PassesThrough()
    {
        // Arrange
        var behavior = new ValidationBehavior<TestRequest, string>([]);

        // Act
        var result = await behavior.Handle(new TestRequest("hi"), NextReturning("ok"), CancellationToken.None);

        // Assert
        result.Should().Be("ok");
    }

    [Fact]
    public async Task Handle_WhenValidationPasses_PassesThrough()
    {
        // Arrange
        var behavior = new ValidationBehavior<TestRequest, string>([new RequiredValueValidator()]);

        // Act
        var result = await behavior.Handle(new TestRequest("hello"), NextReturning("ok"), CancellationToken.None);

        // Assert
        result.Should().Be("ok");
    }

    [Fact]
    public async Task Handle_WhenSingleValidationFailure_ThrowsValidationException()
    {
        // Arrange
        var behavior = new ValidationBehavior<TestRequest, string>([new RequiredValueValidator()]);

        // Act
        var act = async () => await behavior.Handle(new TestRequest(null), NextReturning("ok"), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_WhenMultipleValidationFailuresFromOneValidator_CollectsAllIntoSingleException()
    {
        // Arrange
        // A request with null value fails both NotEmpty and MinimumLength
        var behavior = new ValidationBehavior<TestRequest, string>(
        [
            new RequiredValueValidator(),
            new MinLengthValidator(),
        ]);

        // Act
        var act = async () => await behavior.Handle(new TestRequest(null), NextReturning("ok"), CancellationToken.None);

        // Assert
        var exception = await act.Should().ThrowAsync<ValidationException>();
        exception.Which.Errors.Should().HaveCountGreaterThan(1);
    }

    [Fact]
    public async Task Handle_WhenMultipleValidators_CollectsFailuresFromAll()
    {
        // Arrange
        var behavior = new ValidationBehavior<TestRequest, string>(
        [
            new RequiredValueValidator(),
            new MinLengthValidator(),
        ]);

        // Act
        var act = async () => await behavior.Handle(new TestRequest(null), NextReturning("ok"), CancellationToken.None);

        // Assert
        var exception = await act.Should().ThrowAsync<ValidationException>();
        exception.Which.Errors
            .Select(e => e.PropertyName)
            .Should().Contain("Value");
    }
}
