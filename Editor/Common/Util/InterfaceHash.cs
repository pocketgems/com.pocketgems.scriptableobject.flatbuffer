using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using PocketGems.Parameters.Common.Editor;
using UnityEngine.TestTools;

namespace PocketGems.Parameters.Common.Util.Editor
{
    public class InterfaceHash : IInterfaceHash
    {
        private readonly string _assemblyInfoPath;
        private readonly string _assemblyInfoEditorPath;
        private readonly string _generatedDataHashPath;

        private const string MacNewLine = "\n";
        private const string HashFormat = "// hash:[{0}]";
        private static readonly Regex s_hashRegex = new Regex(@"^// hash:\[(.*)\]$", RegexOptions.Compiled);

        /// <summary>
        /// Creates the Project's interface hash.
        /// </summary>
        /// <returns>default interface hash used at runtime</returns>
        public static InterfaceHash Create()
        {
            return new InterfaceHash(
                EditorParameterConstants.CodeGeneration.AssemblyInfoPath,
                EditorParameterConstants.CodeGeneration.AssemblyInfoEditorPath,
                EditorParameterConstants.GeneratedAsset.HashFilePath);
        }

        public InterfaceHash(string assemblyInfoPath, string assemblyInfoEditorPath, string generatedDataHashPath)
        {
            _assemblyInfoPath = assemblyInfoPath;
            _assemblyInfoEditorPath = assemblyInfoEditorPath;
            _generatedDataHashPath = generatedDataHashPath;
        }

        public string AssemblyInfoHash
        {
            get => GetHashFromFile(_assemblyInfoPath, out int _);
            set => WriteHashToFile(value, _assemblyInfoPath);
        }

        public string AssemblyInfoEditorHash
        {
            get => GetHashFromFile(_assemblyInfoEditorPath, out int _);
            set => WriteHashToFile(value, _assemblyInfoEditorPath);
        }

        public string GeneratedDataHash
        {
            get => GetHashFromFile(_generatedDataHashPath, out int _);
            set => WriteHashToFile(value, _generatedDataHashPath, false);
        }

        public string GeneratedDataLoaderHash
        {
            [ExcludeFromCoverage]
            get
            {
                var parameterDataLoader = EditorParams.CreateParameterDataLoader();
                FieldInfo fieldInfo = parameterDataLoader.GetType().GetField("s_expectedParameterDataHash", BindingFlags.NonPublic | BindingFlags.Static);
                if (fieldInfo == null)
                    return "";
                return (string)fieldInfo.GetValue(null);
            }
        }

        private string GetHashFromFile(string path, out int index)
        {
            index = 0;
            if (!File.Exists(path))
                return null;

            var lines = File.ReadAllLines(path);
            for (int i = lines.Length - 1; i >= 0; i--)
            {
                var line = lines[i];
                var matches = s_hashRegex.Matches(line);
                if (matches.Count == 1)
                {
                    index = i;
                    return matches[0].Groups[1].Captures[0].Value;
                }
            }
            return null;
        }

        private void WriteHashToFile(string hash, string path, bool append = true)
        {
            var hashLine = string.Format(HashFormat, hash);
            if (!append || !File.Exists(path))
            {
                // write over whole file
                File.WriteAllText(path, hashLine);
                return;
            }

            const string GitConflictMarker1 = "<<<<<<<";
            const string GitConflictMarker2 = "=======";
            const string GitConflictMarker3 = ">>>>>>>";

            // trim any white space, git conflicts or git hashes from the end
            var lines = File.ReadAllLines(path).ToList();
            for (int i = lines.Count - 1; i >= 0; i--)
            {
                var line = lines[i];
                if (string.IsNullOrWhiteSpace(line) ||
                    line.Contains(GitConflictMarker1) ||
                    line.Contains(GitConflictMarker2) ||
                    line.Contains(GitConflictMarker3) ||
                    s_hashRegex.Matches(line).Count == 1)
                {
                    lines.RemoveAt(i);
                }
                else
                {
                    break;
                }
            }

            // append new line
            lines.Add("");
            lines.Add(hashLine);
            lines.Add(""); // new line at end of file

            var text = string.Join(MacNewLine, lines);
            File.WriteAllText(path, text);
        }
    }
}
