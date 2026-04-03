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

using ChaosForge.Domain.Common;
using ChaosForge.Domain.Exceptions;

namespace ChaosForge.Domain.Entities;

/// <summary>
/// Represents a User Requirement Specification derived from a use case.
/// </summary>
public sealed class URS : EntityBase<Guid>
{
    /// <summary>Parameterless constructor required by EF Core. Do not use directly.</summary>
    private URS()
    {
        Title = string.Empty;
        Description = string.Empty;
    }

    /// <summary>
    /// Initializes a new URS for the given use case.
    /// </summary>
    /// <param name="useCaseId">The identifier of the owning use case.</param>
    /// <param name="title">The short title of this requirement.</param>
    /// <param name="description">The full description of the requirement.</param>
    public URS(Guid useCaseId, string title, string description) : base(Guid.NewGuid())
    {
        UseCaseId = useCaseId;
        Title = title;
        Description = description;
    }

    /// <summary>Gets the identifier of the owning use case.</summary>
    public Guid UseCaseId { get; private set; }

    /// <summary>Gets the short title of this requirement.</summary>
    public string Title { get; private set; }

    /// <summary>Gets the full description of the requirement.</summary>
    public string Description { get; private set; }

    /// <summary>Gets the human edit note from the last manual revision, if any.</summary>
    public string? HumanEditNote { get; private set; }

    /// <summary>
    /// Applies a human-authored edit to the requirement description.
    /// </summary>
    /// <param name="editedDescription">The revised description. Must not be null or whitespace.</param>
    /// <param name="note">A note explaining the edit. Must not be null or whitespace.</param>
    /// <exception cref="DomainException">Thrown when either parameter is null or whitespace.</exception>
    public void ApplyHumanEdit(string editedDescription, string note)
    {
        if (string.IsNullOrWhiteSpace(editedDescription))
        {
            throw new DomainException($"{nameof(editedDescription)} must not be null or whitespace.");
        }

        if (string.IsNullOrWhiteSpace(note))
        {
            throw new DomainException($"{nameof(note)} must not be null or whitespace.");
        }

        Description = editedDescription;
        HumanEditNote = note;
    }
}
