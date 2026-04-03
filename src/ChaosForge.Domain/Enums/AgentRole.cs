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
/// Represents the functional role of an AI agent within the team.
/// </summary>
public enum AgentRole
{
    /// <summary>Business Analyst — singleton role.</summary>
    BusinessAnalyst,

    /// <summary>Architect — singleton role.</summary>
    Architect,

    /// <summary>Scrum Master — singleton role.</summary>
    ScrumMaster,

    /// <summary>Developer agent.</summary>
    Developer,

    /// <summary>Tester agent.</summary>
    Tester,

    /// <summary>Reviewer agent.</summary>
    Reviewer,

    /// <summary>Technical Writer agent.</summary>
    TechnicalWriter,
}
