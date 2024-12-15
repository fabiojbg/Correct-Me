using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace CorrectMe.Helpers
{
    public static class Cryptography
    {
        #region Settings

        private static int _iterations = 2;
        private static int _defaultKeySize = 128;

        private static string _hash = "SHA1";
        private static string _salt =   "!F@b10|B0t3!h0#*";   // Random
        private static string _vector = "-F4b10|G@g14n0-|"; // Random

        #endregion

        #region ************ AES Specific ****************

        //---------------------------------------------------------------------------------------------------------------------------------
        public static string EncryptAESToString(string value, string password, int? keySize = null)
        {
            return EncryptToString<AesManaged>(value, password, keySize);
        }
        //---------------------------------------------------------------------------------------------------------------------------------
        public static string EncryptAESToString(byte[] value, string password, int? keySize = null)
        {
            return EncryptToString<AesManaged>(value, password, keySize);
        }
        //---------------------------------------------------------------------------------------------------------------------------------
        public static byte[] EncryptAESToByteArray(string value, string password, int? keySize = null)
        {
            return EncryptToByteArray<AesManaged>(value, password, keySize);
        }
        //---------------------------------------------------------------------------------------------------------------------------------
        public static byte[] EncryptAESToByteArray(byte[] value, string password, int? keySize = null)
        {
            return EncryptToByteArray<AesManaged>(value, password, keySize);
        }

        //---------------------------------------------------------------------------------------------------------------------------------
        public static string DecryptAESToString(byte[] value, string password, int? keySize = null)
        {
            return DecryptToString<AesManaged>(value, password, keySize);
        }
        //---------------------------------------------------------------------------------------------------------------------------------
        public static string DecryptAESToString(string value, string password, int? keySize = null)
        {
            return DecryptToString<AesManaged>(value, password, keySize);
        }
        //---------------------------------------------------------------------------------------------------------------------------------
        public static byte[] DecryptAESToByteArray(string value, string password, int? keySize = null)
        {
            return DecryptToByteArray<AesManaged>(value, password, keySize);
        }
        //---------------------------------------------------------------------------------------------------------------------------------
        public static byte[] DecryptAESToByteArray(byte[] value, string password, int? keySize = null)
        {
            return DecryptToByteArray<AesManaged>(value, password, keySize);
        }
        #endregion

        #region ***************** Generic Cryptography Functions ******************
        //---------------------------------------------------------------------------------------------------------------------------------
        public static string EncryptToString<T>(string value, string password, int? keySize = null)
                                    where T : SymmetricAlgorithm, new()
        {
            return Convert.ToBase64String(EncryptToByteArray<T>(UTF8Encoding.UTF8.GetBytes(value), password, keySize));
        }


        //---------------------------------------------------------------------------------------------------------------------------------
        public static string EncryptToString<T>(byte[] value, string password, int? keySize = null)
                                    where T : SymmetricAlgorithm, new()
        {
            return Convert.ToBase64String(EncryptToByteArray<T>(value, password, keySize));
        }

        //---------------------------------------------------------------------------------------------------------------------------------
        public static byte[] EncryptToByteArray<T>(string value, string password, int? keySize = null)
                                    where T : SymmetricAlgorithm, new()
        {
            return EncryptToByteArray<T>(UTF8Encoding.UTF8.GetBytes(value), password, keySize);
        }

        //---------------------------------------------------------------------------------------------------------------------------------
        public static byte[] EncryptToByteArray<T>(byte[] value, string password, int? keySize = null)
                                    where T : SymmetricAlgorithm, new()
        {
            byte[] vectorBytes = UTF8Encoding.UTF8.GetBytes(_vector);
            byte[] saltBytes = UTF8Encoding.UTF8.GetBytes(_salt);
            byte[] valueBytes = value;

            byte[] encrypted;

            if (!keySize.HasValue) keySize = _defaultKeySize;

            using (T cipher = new T())
            {
                PasswordDeriveBytes _passwordBytes = new PasswordDeriveBytes(password, saltBytes, _hash, _iterations);
                byte[] keyBytes = _passwordBytes.GetBytes(keySize.Value / 8);

                cipher.Mode = CipherMode.CBC;

                using (ICryptoTransform encryptor = cipher.CreateEncryptor(keyBytes, vectorBytes))
                {
                    using (MemoryStream to = new MemoryStream())
                    {
                        using (CryptoStream writer = new CryptoStream(to, encryptor, CryptoStreamMode.Write))
                        {
                            writer.Write(valueBytes, 0, valueBytes.Length);
                            writer.FlushFinalBlock();
                            encrypted = to.ToArray();
                        }
                    }
                }
                cipher.Clear();
            }
            return encrypted;
        }

        //---------------------------------------------------------------------------------------------------------------------------------
        public static string DecryptToString<T>(string value, string password, int? keySize = null) where T : SymmetricAlgorithm, new()
        {
            if (value == null) return null;
            return Encoding.UTF8.GetString(DecryptToByteArray<T>(Convert.FromBase64String(value), password, keySize));
        }

        //---------------------------------------------------------------------------------------------------------------------------------
        public static string DecryptToString<T>(byte[] value, string password, int? keySize = null) where T : SymmetricAlgorithm, new()
        {
            if (value == null) return null;
            return Encoding.UTF8.GetString(DecryptToByteArray<T>(value, password, keySize));
        }
        //---------------------------------------------------------------------------------------------------------------------------------
        public static byte[] DecryptToByteArray<T>(string value, string password, int? keySize = null) where T : SymmetricAlgorithm, new()
        {
            return DecryptToByteArray<T>(Convert.FromBase64String(value), password, keySize);
        }
        //---------------------------------------------------------------------------------------------------------------------------------
        public static byte[] DecryptToByteArray<T>(byte[] value, string password, int? keySize = null) where T : SymmetricAlgorithm, new()
        {
            byte[] vectorBytes = UTF8Encoding.UTF8.GetBytes(_vector);
            byte[] saltBytes = UTF8Encoding.UTF8.GetBytes(_salt);
            byte[] valueBytes = value;

            byte[] decrypted;
            int decryptedByteCount = 0;

            if (!keySize.HasValue) keySize = _defaultKeySize;

            using (T cipher = new T())
            {
                PasswordDeriveBytes _passwordBytes = new PasswordDeriveBytes(password, saltBytes, _hash, _iterations);
                byte[] keyBytes = _passwordBytes.GetBytes(keySize.Value / 8);

                cipher.Mode = CipherMode.CBC;

                try
                {
                    using (ICryptoTransform decryptor = cipher.CreateDecryptor(keyBytes, vectorBytes))
                    {
                        using (MemoryStream from = new MemoryStream(valueBytes))
                        {
                            using (CryptoStream reader = new CryptoStream(from, decryptor, CryptoStreamMode.Read))
                            {
                                decrypted = new byte[valueBytes.Length];
                                int decodedBytes = 0, bytesToRead = 0;
                                do
                                {
                                    bytesToRead = valueBytes.Length - decryptedByteCount;
                                    decodedBytes = reader.Read(decrypted, decryptedByteCount, bytesToRead);
                                    decryptedByteCount += decodedBytes;
                                } while (decodedBytes != 0);
                            }
                        }
                    }
                }
                catch
                {
                    return new byte[0];
                }

                cipher.Clear();
            }
            return decrypted.Take(decryptedByteCount).ToArray();
        }
        #endregion
    }
}
