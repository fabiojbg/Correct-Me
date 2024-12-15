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
    public class LanguageCorrector
    {
        readonly string _openAIKey;
        public LanguageCorrector(string openAIKey)
        {
            _openAIKey = openAIKey;
        }

        public CollectionResult<StreamingChatCompletionUpdate> CorrectTextMistakes(string language,
                                                                                   string UILanguage,
                                                                                   string userText)
        {
                ChatClient client = new ChatClient(model: "gpt-4o", apiKey: _openAIKey);

                var systemMessage = ChatMessage.CreateSystemMessage($@"You are an {language} teacher, and you help users correct the errors in their writing.
Respond to every user message with the corrected form. Correct all errors in syntax, verb tense, agreement, or spelling. The language to be used is {language}.
Do not process HTML, XML tags or line breaks; repeat them in your response as is.
At the end, add detailed explanations, in {UILanguage}, for the modifications you've made entitled with the equivalent word for 'Explanations' in the '{UILanguage}' language.
");

                var userMessage = ChatMessage.CreateUserMessage(userText);

                var completionUpdates = client.CompleteChatStreaming(systemMessage, userMessage);

            return completionUpdates;
        }

    }
}
