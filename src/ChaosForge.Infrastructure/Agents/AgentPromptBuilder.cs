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

using ChaosForge.Domain.Entities;

namespace ChaosForge.Infrastructure.Agents;

/// <summary>
/// Pure helper for constructing LLM prompts. Concrete agents call these methods
/// directly — no DI required.
/// </summary>
public static class AgentPromptBuilder
{
    /// <summary>
    /// Returns <paramref name="basePrompt"/> unchanged when <paramref name="priorAttempt"/> is null.
    /// When a prior rejected attempt is provided, appends its output and rejection note as context.
    /// </summary>
    public static string BuildWithPriorAttempt(string basePrompt, TaskAttempt? priorAttempt)
    {
        if (priorAttempt is null)
        {
            return basePrompt;
        }

        return basePrompt + "\n\n" + FormatRejectionContext(priorAttempt);
    }

    /// <summary>
    /// Formats a prior rejected attempt's output and rejection note into a readable prompt section.
    /// </summary>
    public static string FormatRejectionContext(TaskAttempt attempt)
    {
        var note = attempt.ReviewNote ?? attempt.TestNote ?? "(no rejection note provided)";

        return $"""
            --- Previous Attempt (Rejected) ---
            Output:
            {attempt.Output}

            Rejection Note:
            {note}
            --- End of Previous Attempt ---
            """;
    }
}
