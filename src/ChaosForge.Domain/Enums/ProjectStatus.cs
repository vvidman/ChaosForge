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
/// Represents the lifecycle phase of a <see cref="Entities.Project"/>.
/// Transitions are forward-only: Setup → RequirementsPhase → ArchitecturePhase → SprintPlanning → Development → Completed.
/// </summary>
public enum ProjectStatus
{
    /// <summary>Initial setup phase.</summary>
    Setup,

    /// <summary>Requirements are being gathered and defined.</summary>
    RequirementsPhase,

    /// <summary>Architecture is being designed.</summary>
    ArchitecturePhase,

    /// <summary>Sprint backlog is being planned.</summary>
    SprintPlanning,

    /// <summary>Active development is underway.</summary>
    Development,

    /// <summary>Project is complete.</summary>
    Completed,
}
