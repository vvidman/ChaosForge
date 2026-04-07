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

namespace ChaosForge.Domain.Events;

/// <summary>
/// Dispatches collected domain events to their handlers. Implemented in Infrastructure.
/// </summary>
public interface IDomainEventDispatcher
{
    /// <summary>
    /// Dispatches all provided domain events to their registered handlers.
    /// </summary>
    /// <param name="events">The events to dispatch.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task DispatchAsync(IReadOnlyList<IDomainEvent> events, CancellationToken cancellationToken = default);
}
