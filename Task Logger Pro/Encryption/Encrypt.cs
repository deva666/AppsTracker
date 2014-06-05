using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Task_Logger_Pro.Encryption
{
    class Encrypt
    {

        private static byte[] GetHashSHA2(string inputString)
        {
            HashAlgorithm algorithm = SHA256.Create();
            return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
        }

        public static string GetEncryptedString(string inputString)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in GetHashSHA2(inputString))
                sb.Append(b.ToString("X2"));

            return sb.ToString();
        }

    }
}
