using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrectMe.Services.Models
{
    public struct DetectedLanguage
    {
        public string LanguageInEnglish { get; set; }
        public string LanguageInNative { get; set; }

        public DetectedLanguage(string languageInEnglish, string languageInNative)
        {
            LanguageInEnglish = languageInEnglish;
            LanguageInNative = languageInNative;
        }
    }
}
