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
/// Raised when a human resolves a revision gate by accepting, editing and accepting, or rejecting.
/// </summary>
/// <param name="RevisionGateId">The identifier of the resolved gate.</param>
/// <param name="ProjectId">The identifier of the project this gate belongs to.</param>
/// <param name="Action">The action taken by the human to resolve the gate.</param>
public sealed record RevisionGateResolvedEvent(
    Guid RevisionGateId,
    Guid ProjectId,
    RevisionGateAction Action) : IDomainEvent;
