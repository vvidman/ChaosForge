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

using System.Text;
using ChaosForge.Application.Abstractions;
using LLama;
using LLama.Common;
using Microsoft.Extensions.Options;

namespace ChaosForge.Infrastructure.LLM;

internal sealed class LlamaSharpLlmProvider : ILlmProvider, IDisposable
{
    private readonly LLamaWeights _weights;
    private readonly ModelParams _modelParams;
    private readonly int _maxTokens;

    public LlamaSharpLlmProvider(IOptions<LlamaSharpOptions> options)
    {
        var opts = options.Value;

        if (string.IsNullOrWhiteSpace(opts.ModelPath))
        {
            throw new InvalidOperationException(
                "LlamaSharp:ModelPath is not configured. Provide a valid path to a GGUF model file.");
        }

        if (!File.Exists(opts.ModelPath))
        {
            throw new InvalidOperationException(
                $"LlamaSharp model file not found at '{opts.ModelPath}'.");
        }

        _maxTokens = opts.MaxTokens;
        _modelParams = new ModelParams(opts.ModelPath);
        _weights = LLamaWeights.LoadFromFile(_modelParams);
    }

    public async Task<string> CompleteAsync(
        string systemPrompt,
        string userPrompt,
        CancellationToken cancellationToken = default)
    {
        using var context = _weights.CreateContext(_modelParams);
        var executor = new InteractiveExecutor(context);

        var prompt = systemPrompt + "\n" + userPrompt;
        var inferenceParams = new InferenceParams { MaxTokens = _maxTokens };

        var sb = new StringBuilder();
        await foreach (var token in executor.InferAsync(prompt, inferenceParams, cancellationToken))
        {
            sb.Append(token);
        }

        return sb.ToString();
    }

    public void Dispose()
    {
        _weights.Dispose();
    }
}
