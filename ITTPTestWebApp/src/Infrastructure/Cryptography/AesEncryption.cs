using System;
using System.Linq;
using System.Text;
using System.Numerics;
using System.Security.Cryptography;

namespace ITTPTestWebApp.Cryptography
{
    static class AesEncryption
    {
        public static string Encrypt(string plainText, string key)
        {
            byte[] keyBytes = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(key));

            using (Aes aes = Aes.Create())
            {
                aes.Key = keyBytes;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.GenerateIV();

                using (MemoryStream ms = new MemoryStream())
                {
                    ms.Write(aes.IV, 0, aes.IV.Length);

                    using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    using (var sw = new StreamWriter(cs)) { sw.Write(plainText); }

                    byte[] cipherBytes = ms.ToArray();
                    return Base62Encode(cipherBytes);
                }
            }
        }

        public static string Decrypt(string cipherText, string key)
        {
            byte[] fullCipher = Base62Decode(cipherText);
            byte[] keyBytes = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(key));

            using (Aes aes = Aes.Create())
            {
                aes.Key = keyBytes;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                int ivLength = aes.BlockSize / 8;
                byte[] iv = new byte[ivLength];
                Array.Copy(fullCipher, 0, iv, 0, iv.Length);
                aes.IV = iv;

                using (MemoryStream ms = new MemoryStream(fullCipher, ivLength, fullCipher.Length - ivLength))
                using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read))
                using (StreamReader sr = new StreamReader(cs)) { return sr.ReadToEnd(); }
            }
        }

        private const string Base62Alphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

        private static string Base62Encode(byte[] data)
        {
            byte[] dataLE = data.Reverse().ToArray();
            byte[] dataWithZero = new byte[dataLE.Length + 1];
            dataLE.CopyTo(dataWithZero, 0);
            dataWithZero[dataWithZero.Length - 1] = 0;
            BigInteger value = new BigInteger(dataWithZero);
            if (value == 0) return Base62Alphabet[0].ToString();

            StringBuilder sb = new StringBuilder();
            while (value > 0)
            {
                value = BigInteger.DivRem(value, 62, out BigInteger remainder);
                sb.Insert(0, Base62Alphabet[(int)remainder]);
            }
            return sb.ToString();
        }

        private static byte[] Base62Decode(string s)
        {
            BigInteger value = 0;
            foreach (char c in s)
            {
                int index = Base62Alphabet.IndexOf(c);
                if (index < 0) { throw new ArgumentException("Недопустимый символ в Base62 строке."); }
                value = value * 62 + index;
            }

            byte[] dataLE = value.ToByteArray();

            Array.Reverse(dataLE);
            if (dataLE.Length > 0 && dataLE[0] == 0)
            { dataLE = dataLE.Skip(1).ToArray(); }
            return dataLE;
        }
    }
}
