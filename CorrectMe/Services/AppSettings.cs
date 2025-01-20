using CorrectMe.Enums;
using CorrectMe.Helpers;
using CorrectMe.Services.ValueTypes;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace CorrectMe.Services
{
    public static class AppSettings
    {
        const string userRoot = "HKEY_CURRENT_USER";
        const string subkey = "SOFTWARE\\CorrectMe";
        static string keyPath = userRoot + "\\" + subkey;

        private static void Save(string configName, string value)
        {
            Registry.SetValue(keyPath, configName, value);
        }

        private static void Save(string configName, int value)
        {
            Registry.SetValue(keyPath, configName, value, RegistryValueKind.DWord);
        }

        private static string GetString(string configName)
        {
            return (string)Registry.GetValue(keyPath, configName, null);
        }
        private static int? GetInt(string configName)
        {
            return Registry.GetValue(keyPath, configName, null) as int?;
        }

        public static int TranslateTarget
        {
            get => GetInt(nameof(TranslateTarget)) ?? -1;
            set => Save(nameof(TranslateTarget), value);
        }

        public static string UILanguage
        {
            get => GetString(nameof(UILanguage)) ?? Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName.ToUpper();
            set => Save(nameof(UILanguage), value);
        }

        public static string GPT_Type
        {
            get => GetString(nameof(GPT_Type)) ?? GPTServiceType.OpenAI.ToString();
            set => Save(nameof(GPT_Type), value);
        }

        public static string GPT_Model
        {
            get => GetString($"{GPT_Type}_{nameof(GPT_Model)}") ?? "gpt-4o";
            set => Save($"{GPT_Type}_{nameof(GPT_Model)}", value);
        }

        public static string GPT_URL
        {
            get => GetString($"{GPT_Type}_{nameof(GPT_URL)}") ?? "https://api.openai.com/";
            set => Save($"{GPT_Type}_{nameof(GPT_URL)}", value);
        }

        public static string GPT_Key
        {
            get
            {
                var encryptedKey = GetString($"{GPT_Type}_{nameof(GPT_Key)}");

                if (string.IsNullOrEmpty(encryptedKey))
                    return String.Empty;

                var keyValue = Cryptography.DecryptAESToString(encryptedKey, Base64.ToBase64(keyPath));
                return keyValue;
            }
            set
            {
                var encrytedKey = Cryptography.EncryptAESToString(value, Base64.ToBase64(keyPath));

                Save($"{GPT_Type}_{nameof(GPT_Key)}", encrytedKey);
            }

        }
    }
}
