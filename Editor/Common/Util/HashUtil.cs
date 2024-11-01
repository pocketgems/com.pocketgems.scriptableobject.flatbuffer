using System.Security.Cryptography;
using System.Text;

namespace PocketGems.Parameters.Common.Util.Editor
{
    public static class HashUtil
    {
        /// <summary>
        /// MD5 hash of the input string.
        /// </summary>
        /// <param name="input">input string</param>
        /// <returns>MD5 hash of string (32 lowercase hex chars). Null if the input is null.</returns>
        public static string MD5Hash(string input)
        {
            if (input == null)
                return null;

            using (var md5 = MD5.Create())
            {
                byte[] data = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
                var sBuilder = new StringBuilder();
                for (int i = 0; i < data.Length; i++)
                    sBuilder.Append(data[i].ToString("x2"));

                return sBuilder.ToString();
            }
        }
    }
}
