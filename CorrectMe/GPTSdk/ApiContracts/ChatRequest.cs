using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace GPTSdk
{
    /// <summary>
    /// Represents a chat request to the OpenAI API.
    /// </summary>
    public class ChatRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; }

        [JsonPropertyName("messages")]
        public IEnumerable<GPTMessage> Messages { get; set; }

        [JsonPropertyName("stream")]
        public bool Stream { get; set; } = false;

        [JsonPropertyName("max_tokens")]
        public int MaxTokens { get; set; }
    }

}
