using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GPTSdk
{
    public interface IChatGPTApiClient
    {
        Task<string> ChatAsync(string model, params GPTMessage[] messages);
        Task ChatStreamAsync(string model,
                             Action<string> onChunkReceived,
                             Action<string> onFinished,
                             Action<Exception> onError,
                             params GPTMessage[] messages);
        Task<IEnumerable<string>> ListModelsAsync();

        string DefaultModel { get; }
    }
}