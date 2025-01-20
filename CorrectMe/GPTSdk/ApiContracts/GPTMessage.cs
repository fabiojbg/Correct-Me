using System;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace GPTSdk
{
    /// <summary>
    /// Represents a message in the chat.
    /// </summary>
    public class GPTMessage
    {
        [JsonPropertyName("role")]
        [JsonConverter(typeof(JsonStringEnumConverter))] // Automatically convert enum to string
        public Role Role { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; }

        static public GPTMessage CreateSystemMessage(string content)
        {
            return new GPTSystemMessage(content);
        }
        static public GPTMessage CreateUserMessage(string content)
        {
            return new GPTUserMessage(content);
        }
    }

    internal class GPTSystemMessage : GPTMessage
    {
        public GPTSystemMessage(string content)
        {
            Role = Role.System;
            Content = content;
        }
    }
    internal class GPTDeveloperMessage : GPTMessage
    {
        public GPTDeveloperMessage(string content)
        {
            Role = Role.Developer;
            Content = content;
        }
    }

    public class GPTUserMessage : GPTMessage
    {
        public GPTUserMessage(string content)
        {
            Role = Role.User;
            Content = content;
        }
    }


    /// <summary>
    /// Represents the role of a message sender.
    /// </summary>
    public enum Role
    {
        [JsonStringEnumMemberName("system")]
        System,
        [JsonStringEnumMemberName("developer")]
        Developer,
        [JsonStringEnumMemberName("user")]
        User,
        [JsonStringEnumMemberName("assistant")]
        Assistant,
        [JsonStringEnumMemberName("tool")]
        Tool,
        [JsonStringEnumMemberName("function")]
        Function,
    }

}
