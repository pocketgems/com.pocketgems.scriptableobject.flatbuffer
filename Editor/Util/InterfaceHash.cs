using System.IO;
using System.Text.RegularExpressions;

namespace PocketGems.Parameters.Util
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
                EditorParameterConstants.GeneratedCode.AssemblyInfoPath,
                EditorParameterConstants.GeneratedCode.AssemblyInfoEditorPath,
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

        private string GetHashFromFile(string path, out int index)
        {
            index = 0;
            if (!File.Exists(path))
                return null;

            var lines = File.ReadAllLines(path);
            for (int i = 0; i < lines.Length; i++)
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

            var oldHash = GetHashFromFile(path, out int index);
            string newContent = null;
            if (oldHash != null)
            {
                // replace old line
                var lines = File.ReadAllLines(path);
                lines[index] = hashLine;
                newContent = string.Join(MacNewLine, lines);
                if (lines[lines.Length - 1].Length != 0)
                    newContent += MacNewLine; // new line at end of file
            }
            else
            {
                // append new line
                newContent = File.ReadAllText(path);
                newContent += MacNewLine;
                newContent += hashLine;
                newContent += MacNewLine;  // new line at end of file
            }

            File.WriteAllText(path, newContent);
        }
    }
}
