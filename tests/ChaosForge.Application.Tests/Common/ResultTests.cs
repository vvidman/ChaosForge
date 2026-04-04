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

using ChaosForge.Application.Common;
using FluentAssertions;

namespace ChaosForge.Application.Tests.Common;

public sealed class ResultTests
{
    [Fact]
    public void Success_SetsIsSuccessTrue_AndNoError()
    {
        // Act
        var result = Result.Success();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeNull();
    }

    [Fact]
    public void Failure_SetsIsSuccessFalse_AndError()
    {
        // Arrange
        const string error = "something went wrong";

        // Act
        var result = Result.Failure(error);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void GenericSuccess_SetsIsSuccessTrue_AndValue()
    {
        // Arrange
        const int value = 42;

        // Act
        var result = Result<int>.Success(value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(value);
        result.Error.Should().BeNull();
    }

    [Fact]
    public void GenericFailure_SetsIsSuccessFalse_AndError()
    {
        // Arrange
        const string error = "not found";

        // Act
        var result = Result<int>.Failure(error);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(error);
    }
}
