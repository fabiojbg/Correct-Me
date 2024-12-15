using System;
using System.Collections.Generic;
using System.Text;

namespace CorrectMe.Helpers
{
    public static class Base64
    {
        public static string ToBase64(this string text)
        {
            if (text == null)
                return null;

            var bytes = Encoding.UTF8.GetBytes(text);
            return Convert.ToBase64String(bytes);
        }

        public static string FromBase64(this string text)
        {
            if (text == null)
                return null;

            var bytes = Convert.FromBase64String(text);

            var result = Encoding.UTF8.GetString(bytes);

            return result;
        }
    }
}
