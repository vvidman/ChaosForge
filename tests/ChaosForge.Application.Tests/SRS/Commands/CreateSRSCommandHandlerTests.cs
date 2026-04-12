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
using ChaosForge.Domain.Repositories;
using FluentAssertions;
using NSubstitute;

namespace ChaosForge.Application.Tests.SRS.Commands;

public sealed class CreateSRSCommandHandlerTests
{
    private readonly ISRSRepository _srsRepository = Substitute.For<ISRSRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private CreateSRSCommandHandler CreateHandler() =>
        new(_srsRepository, _unitOfWork);

    [Fact]
    public async Task Handle_WithValidCommand_AddsSRSAndSavesChanges()
    {
        // Arrange
        var ursId = Guid.NewGuid();
        var command = new CreateSRSCommand(ursId, "Auth module spec", "The module shall implement JWT");
        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _srsRepository.Received(1).AddAsync(
            Arg.Is<Domain.Entities.SRS>(s => s.URSId == ursId && s.Title == "Auth module spec"),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
