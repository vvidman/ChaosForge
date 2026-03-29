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
/// Represents a Software Requirement Specification derived from a URS.
/// </summary>
public sealed class SRS : EntityBase<Guid>
{
    /// <summary>Parameterless constructor required by EF Core. Do not use directly.</summary>
    private SRS()
    {
        Title = string.Empty;
        TechnicalDescription = string.Empty;
    }

    /// <summary>
    /// Initializes a new SRS for the given URS.
    /// </summary>
    /// <param name="ursId">The identifier of the owning URS.</param>
    /// <param name="title">The short title of this specification.</param>
    /// <param name="technicalDescription">The technical description of the requirement.</param>
    public SRS(Guid ursId, string title, string technicalDescription) : base(Guid.NewGuid())
    {
        URSId = ursId;
        Title = title;
        TechnicalDescription = technicalDescription;
    }

    /// <summary>Gets the identifier of the owning URS.</summary>
    public Guid URSId { get; private set; }

    /// <summary>Gets the short title of this specification.</summary>
    public string Title { get; private set; }

    /// <summary>Gets the technical description of the requirement.</summary>
    public string TechnicalDescription { get; private set; }

    /// <summary>Gets the human edit note from the last manual revision, if any.</summary>
    public string? HumanEditNote { get; private set; }

    /// <summary>
    /// Applies a human-authored edit to the technical description.
    /// </summary>
    /// <param name="editedDescription">The revised technical description. Must not be null or whitespace.</param>
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

        TechnicalDescription = editedDescription;
        HumanEditNote = note;
    }
}
