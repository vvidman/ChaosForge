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

namespace ChaosForge.Domain.Enums;

/// <summary>
/// Represents the current state of a <see cref="Entities.WorkTask"/> in the workflow.
/// </summary>
public enum WorkTaskStatus
{
    /// <summary>Task is in the backlog, not yet started.</summary>
    Backlog,

    /// <summary>Task is actively being worked on.</summary>
    InProgress,

    /// <summary>Task has been submitted for code review.</summary>
    InReview,

    /// <summary>Task is undergoing testing.</summary>
    InTesting,

    /// <summary>Task is being documented.</summary>
    InDocumentation,

    /// <summary>Task is fully complete.</summary>
    Done,
}
