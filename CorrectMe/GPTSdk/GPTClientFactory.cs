using CorrectMe.Enums;
using OpenAI;
using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPTSdk
{
    public static class GPTClientFactory
    {
        public static IChatGPTApiClient CreateClient(GPTServiceType service, string apiKey, string serviceURL = null)
        {
            switch (service)
            {
                case GPTServiceType.OpenAI:
                    return new OpenAIClient(apiKey);
                case GPTServiceType.DeepSeek:
                    return new DeepSeekClient(apiKey);
                case GPTServiceType.OpenRouter:
                    return new OpenRouterAIClient(apiKey);
                case GPTServiceType.Custom:
                    return new CustomGPTClient(serviceURL, apiKey);
                default:
                    throw new ArgumentException("Invalid GPT service");
            }

        }
    }

    public class OpenAIClient : GPTClient, IChatGPTApiClient
    {
        public string DefaultModel { get; set; } = "gpt-4o";
        public OpenAIClient(string apiKey) : base("https://api.openai.com", apiKey)
        {

        }
    }

    public class DeepSeekClient : GPTClient, IChatGPTApiClient
    {
        public string DefaultModel { get; set; } = "deepseek-chat"; 
        public DeepSeekClient(string apiKey) : base("https://api.deepseek.com", apiKey)
        {

        }
    }

    public class OpenRouterAIClient : GPTClient, IChatGPTApiClient
    {
        public string DefaultModel { get; set; } = "deepseek/deepseek-chat-v3-0324:free";
        public OpenRouterAIClient(string apiKey) : base("https://openrouter.ai/api", apiKey)
        {

        }
    }

    public class CustomGPTClient : GPTClient, IChatGPTApiClient
    {
        public string DefaultModel { get; set; } = "";

        public CustomGPTClient(string serviceURL, string apiKey) : base(serviceURL, apiKey)
        {

        }
    }

}