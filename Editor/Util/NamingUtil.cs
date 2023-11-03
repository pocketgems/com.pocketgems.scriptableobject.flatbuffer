using System.IO;
using UnityEngine;

namespace PocketGems.Parameters.Util
{
    internal static class NamingUtil
    {
        #region file naming

        // base names from name
        public static string BaseNameFromInfoInterfaceName(string interfaceName)
        {
            interfaceName = Path.GetFileNameWithoutExtension(interfaceName);
            return EditorParameterConstants.Interface.InfoNameRegex.Matches(interfaceName)[0].Groups[1].Captures[0].Value;
        }
        public static string BaseNameFromStructInterfaceName(string interfaceName)
        {
            interfaceName = Path.GetFileNameWithoutExtension(interfaceName);
            return EditorParameterConstants.Interface.StructNameRegex.Matches(interfaceName)[0].Groups[1].Captures[0]
                .Value;
        }
        public static string BaseNameFromScriptableObjectClassName(string className)
        {
            className = Path.GetFileNameWithoutExtension(className);
            return className.Substring(0, className.Length - nameof(ScriptableObject).Length);
        }
        public static string BaseNameFromStructName(string structName) => Path.GetFileNameWithoutExtension(structName);
        public static string BaseNameFromCSVName(string csvName) =>
            Path.GetFileNameWithoutExtension(csvName);

        // names from base name
        public static string InfoInterfaceNameFromBaseName(string baseName) => $"I{baseName}";
        public static string StructInterfaceNameFromBaseName(string baseName) => $"I{baseName}";
        public static string ScriptableObjectClassNameFromBaseName(string baseName, bool includeExtension) =>
            baseName + nameof(ScriptableObject) + (includeExtension ? ".cs" : "");
        public static string StructNameFromBaseName(string baseName, bool includeExtension) =>
            baseName + (includeExtension ? ".cs" : "");
        public static string CSVFileNameFromBaseName(string baseName, bool includeExtension) =>
            includeExtension ? baseName + ".csv" : baseName;

        public static string FlatBufferClassNameFromBaseName(string baseName, bool includeExtension) =>
            baseName + "FlatBuffer" + (includeExtension ? ".cs" : "");
        public static string FlatBufferStructNameFromBaseName(string baseName, bool includeExtension) =>
            baseName + "FlatBufferStruct" + (includeExtension ? ".cs" : "");

        public static string ValidatorClassNameFromBaseName(string baseName, bool includeExtension) =>
            $"{baseName}Validator" + (includeExtension ? ".cs" : "");

        #endregion

        #region naming constants

        public const string IdentifierPropertyName = "Identifier";

        #endregion

        /// <summary>
        /// Converts absolute path to relative path to the project.  Does nothing if already a relative path.
        /// </summary>
        /// <param name="absolutePath">absolute path of the file</param>
        /// <returns>relative path</returns>
        public static string RelativePath(string absolutePath)
        {
            var projectPath = Directory.GetCurrentDirectory();

            // Make sure absolutePath uses the same directory separator character as GetCurrentDirectory().
            // Windows prefers to use '\' (but can use '/'), and macOS always uses '/'.
            absolutePath = absolutePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

            var relativePath = absolutePath.Replace(projectPath, "");
            if (absolutePath.Length == relativePath.Length)
            {
                // input was already a relative path
                return absolutePath;
            }

            // remove initial slash
            return relativePath.Substring(1, relativePath.Length - 1);
        }

        /// <summary>
        /// Converts the string to have it's first char lower case
        /// </summary>
        /// <param name="str">input string</param>
        /// <returns>formatted string.</returns>
        internal static string LowercaseFirstChar(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;
            string result = str.Substring(0, 1).ToLower();
            result += str.Substring(1, str.Length - 1);
            return result;
        }

        /// <summary>
        /// Converts the string to have it's first char upper case
        /// </summary>
        /// <param name="str">input string</param>
        /// <returns>formatted string.</returns>
        internal static string UppercaseFirstChar(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;
            string result = str.Substring(0, 1).ToUpper();
            result += str.Substring(1, str.Length - 1);
            return result;
        }

        /// <summary>
        /// Convert column name to snake case for flat buffer schema file.
        /// </summary>
        /// <param name="columnName">File column name</param>
        /// <returns>schema property name</returns>
        internal static string ToSnakeCase(this string columnName)
        {
            // count capitol letters not including first char
            int numUnderscores = 0;
            bool previousIsUnderscore = false;
            for (int i = 0; i < columnName.Length; i++)
            {
                var c = columnName[i];
                if (i > 0 && !previousIsUnderscore && 'A' <= c && c <= 'Z')
                {
                    numUnderscores++;
                }
                previousIsUnderscore = c == '_';
            }

            // build new string
            char[] newString = new char[numUnderscores + columnName.Length];
            int newStringIndex = 0;
            previousIsUnderscore = false;
            for (int i = 0; i < columnName.Length; i++, newStringIndex++)
            {
                var c = columnName[i];
                if ('A' <= c && c <= 'Z')
                {
                    if (i > 0 && !previousIsUnderscore)
                    {
                        newString[newStringIndex] = '_';
                        newStringIndex++;
                    }
                    c = (char)(c - 'A' + 'a');
                }
                newString[newStringIndex] = c;
                previousIsUnderscore = c == '_';
            }
            return new string(newString);
        }
    }
}
