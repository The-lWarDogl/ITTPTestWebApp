using System.Collections.Concurrent;
using System.Security.Cryptography;

namespace ITTPTestWebApp.Common
{
    static partial class Func
    {
        public static string ConvertToTitleCase(string s) =>
            !string.IsNullOrEmpty(s) ? char.ToUpper(s[0]) + s.Substring(1).ToLower() : s;

        public static Dictionary<TKey, TValue> DeepCopy<TKey, TValue>(Dictionary<TKey, TValue> original)
            where TKey : notnull
            where TValue : ICloneable
        {
            var copy = new Dictionary<TKey, TValue>(original.Count, original.Comparer);
            foreach (var kvp in original)
            {
                TKey clonedKey = kvp.Key;
                TValue clonedValue = (TValue)kvp.Value.Clone();
                copy.Add(clonedKey, clonedValue);
            }
            return copy;
        }

        public static TEnum ConvertToEnumElement<TEnum>(string statusString)
            where TEnum : struct, Enum
        {
            if (Enum.TryParse(statusString, ignoreCase: true, out TEnum result))
            { if (Enum.IsDefined(typeof(TEnum), result)) return result; }

            return default;
        }

        public static bool CanConvertToEnumElement<TEnum>(string statusString)
            where TEnum : struct, Enum
        {
            if (Enum.TryParse(statusString, ignoreCase: true, out TEnum result))
            { if (Enum.IsDefined(typeof(TEnum), result)) return true; }

            return false;
        }

        public static string GenerateRandomPassword(string chars, int length = 8) =>
            new string
            (
                Enumerable.Range(0, length)
                .Select(_ => chars[RandomNumberGenerator.GetInt32(chars.Length)])
                .ToArray()
            );
    }
}
