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
/// Classifies the nature of a <see cref="Entities.TaskAttempt"/>.
/// </summary>
public enum AttemptType
{
    /// <summary>An implementation attempt by a developer agent.</summary>
    Implementation,

    /// <summary>A review attempt by a reviewer agent.</summary>
    Review,

    /// <summary>A testing attempt by a tester agent.</summary>
    Testing,

    /// <summary>A documentation attempt by a technical writer agent.</summary>
    Documentation,
}
