using CorrectMe.Helpers;
using Microsoft.Win32;


namespace CorrectMe.Services
{
    public static class SecretKeyKeeper
    {
        public static void SaveKey(string key)
        {
            const string userRoot = "HKEY_CURRENT_USER";
            const string subkey = "SOFTWARE\\CorrectMe";
            const string keyName = userRoot + "\\" + subkey;

            var encrytedKey = Cryptography.EncryptAESToString(key, Base64.ToBase64(keyName));

            Registry.SetValue(keyName, "SecretKey", encrytedKey);
        }

        public static string GetKey()
        {
            const string userRoot = "HKEY_CURRENT_USER";
            const string subkey = "SOFTWARE\\CorrectMe";
            const string keyName = userRoot + "\\" + subkey;

            var encryptedKey = (string)Registry.GetValue(keyName, "SecretKey", null);

            if (string.IsNullOrEmpty(encryptedKey))
                return null;

            var key = Cryptography.DecryptAESToString(encryptedKey, Base64.ToBase64(keyName));
            return key;
        }
    }
}