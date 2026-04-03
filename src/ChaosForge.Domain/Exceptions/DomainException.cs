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

namespace ChaosForge.Domain.Exceptions;

/// <summary>
/// Represents an error that occurs when a domain invariant is violated.
/// </summary>
public sealed class DomainException : Exception
{
    /// <summary>
    /// Initializes a new instance with the specified error message.
    /// </summary>
    /// <param name="message">The message that describes the invariant violation.</param>
    public DomainException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance with the specified error message and inner exception.
    /// </summary>
    /// <param name="message">The message that describes the invariant violation.</param>
    /// <param name="inner">The exception that caused this exception.</param>
    public DomainException(string message, Exception inner) : base(message, inner)
    {
    }
}
