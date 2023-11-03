using System;
using System.Collections.Generic;

namespace PocketGems.Parameters.Parser
{
    public static class StringDictParser
    {
        /// <summary>
        ///  Converts a string to the specified dictionary types delimited by "|" and ":".
        /// </summary>
        /// <example>
        ///     string : a:1|b:2
        ///     dict : a:1
        ///            b:2
        /// </example>
        /// <param name="str">the string to convert</param>
        /// <returns>the parsed dictionary</returns>
        public static Dictionary<int, int> ParseIntInt(string str)
        {
            return Parse(str, int.Parse, int.Parse);
        }

        ///<inheritdoc cref="ParseIntInt"/>
        public static Dictionary<string, int> ParseStringInt(string str)
        {
            string StringParse(string s) => s;
            return Parse(str, StringParse, int.Parse);
        }

        ///<inheritdoc cref="ParseIntInt"/>
        public static Dictionary<string, float> ParseStringFloat(string str)
        {
            string StringParse(string s) => s;
            return Parse(str, StringParse, float.Parse);
        }

        ///<inheritdoc cref="ParseIntInt"/>
        public static Dictionary<string, string> ParseStringString(string str)
        {
            string StringParse(string s) => s;
            return Parse(str, StringParse, StringParse);
        }

        private static Dictionary<TKey, TValue> Parse<TKey, TValue>(string str, Func<string, TKey> parseKey,
            Func<string, TValue> parseValue)
        {
            var dict = new Dictionary<TKey, TValue>();
            str = str.Trim();
            if (!string.IsNullOrEmpty(str))
            {
                var split = str.Split('|');
                for (int i = 0; i < split.Length; i++)
                {
                    var keyValue = split[i].Split(':');
                    if (keyValue.Length != 2)
                        throw new FormatException("string not properly delimited with one :");
                    var key = parseKey(keyValue[0]);
                    if (dict.ContainsKey(key))
                        throw new ArgumentException("string contains two duplicate keys");
                    dict[key] = parseValue(keyValue[1]);
                }
            }

            return dict;
        }
    }
}
