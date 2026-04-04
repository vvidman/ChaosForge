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
/// Raised when a task attempt is approved or rejected by a reviewer or tester.
/// </summary>
/// <param name="TaskAttemptId">The identifier of the resolved attempt.</param>
/// <param name="WorkTaskId">The identifier of the work task being attempted.</param>
/// <param name="Result">The outcome of the resolution.</param>
public sealed record TaskAttemptResolvedEvent(
    Guid TaskAttemptId,
    Guid WorkTaskId,
    AttemptResult Result) : IDomainEvent;
