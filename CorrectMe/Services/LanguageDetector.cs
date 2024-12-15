﻿using CorrectMe.Extensions;
using CorrectMe.Services.Models;
using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrectMe.Services
{    
    public class LanguageDetector
    {
        public const string UNKOWN_LANGUAGE = "Unknown";
        const string _detectingPrompt = @"You are an assistant specialized in current human languages, and you help us identify the language used in texts.
For each text you receive from the user, you will try your best to identify the language being used and respond only with the name of this language in English followed by a semicolon and the name of the language in the detected language.
Example:
User: Eu gostaria de saber onde estamos.
You: Portuguese;Português

If you are certain that the language variant belongs to some specific country, you can mention it in your response.
Example: ¿Adónde vamos esta noche?
You: Spanish;Español

If you cannot be certain, guess your best. The user text is the following:
";
        
        string _openAIKey;
        public LanguageDetector(string openAIKey)
        {
            _openAIKey = openAIKey;
        }

        public DetectedLanguage DetectLanguage(string openAIModel, 
                                               string textToDetect)
        {
            ChatClient client = new ChatClient(model: openAIModel, apiKey: _openAIKey);

            var systemMessage = ChatMessage.CreateSystemMessage(_detectingPrompt);

            var userMessage = ChatMessage.CreateUserMessage(textToDetect.Trim());

            var response = client.CompleteChat(systemMessage, userMessage);

            var responseText = response.Value.Content[0].Text;

            if( responseText.ContainsCI("Unknown"))
            {
                return new DetectedLanguage("Unknown", "Unknown");
            }

            var languageInEnglish = "";
            var languageInNative = "";
            if ( responseText.ContainsCI(";"))
            {
                languageInEnglish = responseText.Split(';')[0].Trim();
                languageInNative = responseText.Split(';')[1].Trim();
            }

            // compare the response with the language selected in case insensitive manner   
            if (languageInEnglish.ContainsCI("Portuguese"))
            {
                if (!responseText.ContainsCI("Portugal") && !responseText.ContainsCI("European"))
                    return new DetectedLanguage("Brazilian Portuguese", "Português Brasileiro");
            }
            else if (responseText.ContainsCI("English"))
            {
                if (!responseText.ContainsCI("British"))
                    return new DetectedLanguage("American English", "American English");
            }

            return new DetectedLanguage(languageInEnglish, languageInNative);
        }

    }
}