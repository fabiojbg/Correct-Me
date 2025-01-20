using CorrectMe.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace CorrectMe.Services.ValueTypes
{
    public struct GPTDefinitions
    {
        public GPTServiceType Type { get; set; }

        public string Model { get; set; }

        public string URL { get; set; }

        public string APIKey { get; set; }

        private  GPTDefinitions(GPTServiceType gptType, string gptModel, string gptURL, string gptAPIKey)
        {
            Type = gptType;
            Model = gptModel;
            URL = gptURL;
            APIKey = gptAPIKey;
        }

        public static GPTDefinitions FromSettings()
        {
            return new GPTDefinitions((GPTServiceType)Enum.Parse(typeof(GPTServiceType), AppSettings.GPT_Type, true), 
                                        AppSettings.GPT_Model, 
                                        AppSettings.GPT_URL, 
                                        AppSettings.GPT_Key);
        }

        public bool IsValid
        {
            get
            {
                if (Type == GPTServiceType.Custom)
                {
                    return !string.IsNullOrWhiteSpace(Model) && !string.IsNullOrWhiteSpace(URL) && !string.IsNullOrWhiteSpace(APIKey);
                }
                else
                {
                    return !string.IsNullOrWhiteSpace(Model) && !string.IsNullOrWhiteSpace(APIKey);
                }
            }

        }
    }
}
