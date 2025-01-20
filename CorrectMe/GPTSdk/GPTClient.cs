using CorrectMe.Extensions;
using CorrectMe.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace GPTSdk
{
    public abstract class GPTClient : IChatGPTApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        string IChatGPTApiClient.DefaultModel => throw new NotImplementedException();

        /// <summary>
        /// Initializes a new instance of the <see cref="GPTClient"/> class.
        /// </summary>
        /// <param name="baseUrl">The base URL for the API endpoint.</param>
        /// <param name="apiKey">The API key used for authentication.</param>
        public GPTClient(string baseUrl, string apiKey)
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(baseUrl)
            };
            _apiKey = apiKey;
            // Set up the default headers
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        /// <summary>
        /// Retrieves a list of available models from the ChatGPT API.
        /// </summary>
        /// <returns>A list of model names as strings.</returns>
        public async Task<IEnumerable<string>> ListModelsAsync()
        {
            var response = await _httpClient.GetAsync("v1/models");

            await treatResponseError(response);
            // Parse the JSON response
            var jsonResponse = await response.Content.ReadAsStringAsync();
            using (var jsonDoc = JsonDocument.Parse(jsonResponse))
            {
                // Extract the model names
                var models = new List<string>();
                if (jsonDoc.RootElement.TryGetProperty("data", out var dataArray))
                {
                    foreach (var model in dataArray.EnumerateArray())
                    {
                        if (model.TryGetProperty("id", out var idProperty))
                        {
                            models.Add(idProperty.GetString());
                        }
                    }
                }
                return models;
            }
        }

        /// <summary>
        /// Sends a chat message to the specified model and returns the complete response.
        /// </summary>
        /// <param name="model">The model to use for the chat.</param>
        /// <param name="messages">The list of messages to send to the model.</param>
        /// <returns>The complete response from the model as a string.</returns>
        public async Task<string> ChatAsync(string model, params GPTMessage[] messages)
        {
            var response = "";

            await ChatStreamAsync(model,
                            null,
                            (completeResponse) =>  //onFinished
                            {
                                response = completeResponse;
                            },
                            (ex) => //onError
                            {
                                response = "There was an error while correcting text: " + ex.Message;
                            },
                            messages
                            );

            return response;
        }

        /// <summary>
        /// Sends a chat message to the specified model using streaming.
        /// </summary>
        /// <param name="model">The model to use for the chat.</param>
        /// <param name="messages">The list of messages to send to the model.</param>
        /// <param name="onChunkReceived">A callback function to handle each chunk of data received. Can be null</param>
        /// <param name="onFinished">A callback function to receive the complete message received. Can be null</param>
        /// <param name="onError">A callback function to handle exceptions. Can be null</param>        
        public async Task ChatStreamAsync(string model,
                                          Action<string> onChunkReceived,
                                          Action<string> onFinished,
                                          Action<Exception> onError,
                                          params GPTMessage[] messages
                                          )
        {
            try
            {
                var requestBody = new ChatRequest
                {
                    Model = model,
                    Messages = messages,
                    Stream = true,
                    MaxTokens = 4096
                };

                var jsonOptions = new JsonSerializerOptions
                {
                    Converters = { new JsonStringEnumConverter() } // Automatically convert enums to strings
                };

                var content = new StringContent(JsonSerializer.Serialize(requestBody, jsonOptions),
                                                Encoding.UTF8,
                                                "application/json");

                var request = new HttpRequestMessage(HttpMethod.Post, "v1/chat/completions")
                {
                    Content = content
                };

                using (var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
                {
                    await treatResponseError(response);

                    using (var stream = await response.Content.ReadAsStreamAsync())
                    using (var reader = new System.IO.StreamReader(stream))
                    {
                        var completeResponse = new StringBuilder();
                        while (!reader.EndOfStream)
                        {
                            var serializedChunk = await reader.ReadLineAsync();
                            if (!serializedChunk.Contains("data:") ||
                                 serializedChunk.ContainsCI("[DONE]"))
                            {
                                continue;
                            }
                            serializedChunk = serializedChunk.Substring("data:".Length);
                            var completionPart = JsonSerializer.Deserialize<GPTStreamCompletion>(serializedChunk,
                                                                                                 new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                            var textChunk = completionPart.Choices?[0]?.Delta?.Content;
                            if (!string.IsNullOrEmpty(textChunk))
                            {
                                completeResponse.AppendLine(textChunk);
                                onChunkReceived?.Invoke(textChunk);
                            }
                        }

                        onFinished?.Invoke(completeResponse.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                onError?.Invoke(ex);
            }
        }

        /// <summary>
        /// Handles errors in the HTTP response by ensuring the status code is successful.
        /// Generates an exception with the error message if the response is not successful.
        /// </summary>
        /// <param name="response">The HTTP response message to check for errors.</param>
        private async Task treatResponseError(HttpResponseMessage response)
        {
            var error = "";
            try
            {
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                try
                {
                    var errorInResponse = await response.Content.ReadAsStringAsync();
                    error = $"Error: {ex.Message}. Details={errorInResponse}";
                }
                catch { }

                if (!String.IsNullOrWhiteSpace(error))
                    throw new Exception(error, ex);

                throw;
            }
        }
    }
}