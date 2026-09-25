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

using Microsoft.Extensions.Options;

namespace ChaosForge.Infrastructure.LLM;

/// <summary>
/// Fails application startup when <see cref="InferRouterOptions.BaseUrl"/> is missing or is not an
/// absolute http(s) URL, instead of failing later on the first agent LLM call.
/// </summary>
internal sealed class InferRouterOptionsValidator : IValidateOptions<InferRouterOptions>
{
    private const string BaseUrlKey = $"{InferRouterOptions.SectionName}:{nameof(InferRouterOptions.BaseUrl)}";

    public ValidateOptionsResult Validate(string? name, InferRouterOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.BaseUrl))
        {
            return ValidateOptionsResult.Fail(
                $"{BaseUrlKey} is required. Set it in appsettings.Development.json " +
                "or via the InferRouter__BaseUrl environment variable.");
        }

        var isHttpUrl = Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);

        return isHttpUrl
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(
                $"{BaseUrlKey} must be an absolute http or https URL. Current value: '{options.BaseUrl}'.");
    }
}
