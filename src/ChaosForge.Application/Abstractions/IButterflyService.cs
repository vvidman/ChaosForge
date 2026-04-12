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

namespace ChaosForge.Application.Abstractions;

/// <summary>
/// Propagates human edits from a resolved revision gate downstream through the requirements pipeline.
/// Implemented in spec 28.
/// </summary>
public interface IButterflyService
{
    /// <summary>
    /// Propagates the human-edited output from the specified gate to all downstream artefacts.
    /// </summary>
    /// <param name="revisionGateId">The identifier of the resolved gate whose edited output should propagate.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task PropagateAsync(Guid revisionGateId, CancellationToken cancellationToken = default);
}
