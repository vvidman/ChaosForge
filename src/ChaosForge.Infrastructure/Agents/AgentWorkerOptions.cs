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

namespace ChaosForge.Infrastructure.Agents;

/// <summary>
/// Configuration options shared across all agent worker background services.
/// Bound from the <c>Agents</c> section of appsettings.json.
/// </summary>
public sealed class AgentWorkerOptions
{
    /// <summary>Gets the polling interval in milliseconds between work cycles.</summary>
    public int PollingIntervalMs { get; init; } = 3000;
}
