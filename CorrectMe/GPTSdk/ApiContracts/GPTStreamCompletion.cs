using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using System.Text.Json;
using System.Web.UI.WebControls;

namespace GPTSdk
{
    public class GPTStreamCompletion
    {
        public string Id { get; set; }
        public string Object { get; set; }
        public long Created { get; set; }
        public string Model { get; set; }
        public string ServiceTier { get; set; }
        public string SystemFingerprint { get; set; }
        public List<Choice> Choices { get; set; }
    }

    public class Choice
    {
        public int Index { get; set; }
        public Delta Delta { get; set; }
        public object Logprobs { get; set; } // Pode ser null, então usamos object
        public string FinishReason { get; set; } // Pode ser null, então usamos string
    }

    public class Delta
    {
        public string Content { get; set; }
    }

}