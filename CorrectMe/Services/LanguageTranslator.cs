using CorrectMe.Services.Models;
using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ClientModel;

namespace CorrectMe.Services
{
    public class LanguageTranslator
    {
        readonly string _openAIKey;
        public LanguageTranslator(string openAIKey)
        {
            _openAIKey = openAIKey;
        }


        public CollectionResult<StreamingChatCompletionUpdate> Translate(string fromLanguage,
                                                                         string toLanguage,
                                                                         string userText)
        {
                ChatClient client = new ChatClient(model: "gpt-4o", apiKey: _openAIKey);

            var systemMessage = ChatMessage.CreateSystemMessage($@"You are a translator specialized in translating texts.
Try your best to detect the language users are entering and respond to their message with the same message translated into '{toLanguage}'.
Do not process HTML, XML tags or line breaks; repeat them in your response as is.
Remember, just respond to the user with the text they entered with the translated version, for example:
User: Eu gostaria de saber onde estamos.
System: I would like to know where we are.
            ");


            var userMessage = ChatMessage.CreateUserMessage(userText);

                var completionUpdates = client.CompleteChatStreaming(systemMessage, userMessage);

            return completionUpdates;
        }

    }
}
