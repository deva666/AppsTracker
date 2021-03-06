﻿#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System.Security.Cryptography;
using System.Text;

namespace AppsTracker.Common.Utils
{
    public static class Hashing
    {
        private static byte[] GetHashSHA2(string inputString)
        {
            var algorithm = SHA256.Create();
            return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
        }

        public static string EncryptString(string inputString)
        {
            var sb = new StringBuilder();
            foreach (byte b in GetHashSHA2(inputString))
                sb.Append(b.ToString("X2"));

            return sb.ToString();
        }
    }
}
