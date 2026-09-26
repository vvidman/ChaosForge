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

using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ChaosForge.Infrastructure.Persistence.Converters;

/// <summary>
/// Stores <see cref="DateTime"/> values as UTC and marks values read back as
/// <see cref="DateTimeKind.Utc"/>. SQLite has no zone-aware timestamp type, so without this
/// values come back as <see cref="DateTimeKind.Unspecified"/>, are serialized without a "Z"
/// suffix, and browsers interpret them as local time.
/// </summary>
internal sealed class UtcDateTimeConverter() : ValueConverter<DateTime, DateTime>(
    value => value.Kind == DateTimeKind.Local ? value.ToUniversalTime() : value,
    value => DateTime.SpecifyKind(value, DateTimeKind.Utc));
