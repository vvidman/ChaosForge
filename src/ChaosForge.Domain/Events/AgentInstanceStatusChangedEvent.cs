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

using ChaosForge.Domain.Enums;

namespace ChaosForge.Domain.Events;

/// <summary>
/// Raised when an agent instance transitions from one operational status to another.
/// </summary>
/// <param name="AgentInstanceId">The identifier of the agent instance that changed status.</param>
/// <param name="OldStatus">The status before the transition.</param>
/// <param name="NewStatus">The status after the transition.</param>
public sealed record AgentInstanceStatusChangedEvent(
    Guid AgentInstanceId,
    AgentInstanceStatus OldStatus,
    AgentInstanceStatus NewStatus) : IDomainEvent;
