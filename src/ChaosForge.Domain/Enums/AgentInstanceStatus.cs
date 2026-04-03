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
/// Represents the operational state of an <see cref="Entities.AgentInstance"/>.
/// </summary>
public enum AgentInstanceStatus
{
    /// <summary>The agent is available and waiting for work.</summary>
    Idle,

    /// <summary>The agent is actively executing a task.</summary>
    Working,

    /// <summary>The agent is blocked and cannot proceed.</summary>
    Blocked,

    /// <summary>The agent has completed its lifecycle and will not receive new tasks.</summary>
    Finished,
}
