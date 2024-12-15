using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrectMe.Services.Models
{
    public class LanguageToTranslate
    {
        public int Id { get; private set; }
        public string LocalizedName { get; private set; }
        public bool IsDefault => LocalizedName.Contains("*");

        public LanguageToTranslate(int index, string localizedName)
        {
            Id = index;
            LocalizedName = localizedName;
        }

        public static LanguageToTranslate Decode(string encoded)
        {
            var parts = encoded.Split(':');
            return new LanguageToTranslate(int.Parse(parts[0]), parts[1].Trim());
        }

        public override string ToString()
        {
            return LocalizedName.Trim('*');
        }
    }
}
