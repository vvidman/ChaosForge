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

using ChaosForge.Domain.Events;
using ChaosForge.Infrastructure.Events;
using FluentAssertions;
using MediatR;
using NSubstitute;

namespace ChaosForge.Infrastructure.Tests.Events;

public sealed class DomainEventDispatcherTests
{
    private readonly IPublisher _publisher = Substitute.For<IPublisher>();

    private DomainEventDispatcher CreateDispatcher() => new(_publisher);

    [Fact]
    public async Task DispatchAsync_WithMultipleEvents_PublishesEachEventInOrder()
    {
        // Arrange
        var event1 = Substitute.For<IDomainEvent>();
        var event2 = Substitute.For<IDomainEvent>();
        var events = new List<IDomainEvent> { event1, event2 };
        var dispatcher = CreateDispatcher();

        // Act
        await dispatcher.DispatchAsync(events, CancellationToken.None);

        // Assert
        Received.InOrder(() =>
        {
            _publisher.Publish(event1, CancellationToken.None);
            _publisher.Publish(event2, CancellationToken.None);
        });
    }

    [Fact]
    public async Task DispatchAsync_WithSingleEvent_PublishesExactlyOnce()
    {
        // Arrange
        var domainEvent = Substitute.For<IDomainEvent>();
        var events = new List<IDomainEvent> { domainEvent };
        var dispatcher = CreateDispatcher();

        // Act
        await dispatcher.DispatchAsync(events, CancellationToken.None);

        // Assert
        await _publisher.Received(1).Publish(domainEvent, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DispatchAsync_WithEmptyList_DoesNotPublishAnyEvent()
    {
        // Arrange
        var events = new List<IDomainEvent>();
        var dispatcher = CreateDispatcher();

        // Act
        await dispatcher.DispatchAsync(events, CancellationToken.None);

        // Assert
        await _publisher.DidNotReceiveWithAnyArgs().Publish(default(object)!, default);
    }
}
