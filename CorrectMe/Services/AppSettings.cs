using CorrectMe.Helpers;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

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
            return (string)Registry.GetValue(keyPath, configName, string.Empty);
        }
        private static int? GetInt(string configName)
        {
            return Registry.GetValue(keyPath, configName, string.Empty) as int?;
        }

        public static int TranslateTarget
        {
            get => GetInt(nameof(TranslateTarget))??-1;
            set => Save(nameof(TranslateTarget), value);
        }

        public static string UILanguage
        {
            get => GetString(nameof(UILanguage)) ?? Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName.ToUpper();
            set => Save(nameof(UILanguage), value);
        }
    }
}
