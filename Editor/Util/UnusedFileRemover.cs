using System.Collections.Generic;
using System.IO;

namespace PocketGems.Parameters.Util
{
    /// <summary>
    /// Convenience class to remove unused files.
    ///
    /// Only works for flat folder structures (no subfolders)
    /// </summary>
    internal class UnusedFileRemover
    {
        private const string kMetaSuffix = ".meta";
        private string _directory;
        private HashSet<string> _modifiedFiles;

        public UnusedFileRemover(string directory)
        {
            _directory = directory;
            _modifiedFiles = new HashSet<string>();
        }

        public void UsedFile(string file)
        {
            _modifiedFiles.Add(file);
            if (!file.EndsWith(kMetaSuffix))
                _modifiedFiles.Add(file + kMetaSuffix);
        }

        public void RemoveUnusedFiles()
        {
            if (!Directory.Exists(_directory))
                return;

            string[] filePaths = Directory.GetFiles(_directory);
            for (int i = 0; i < filePaths.Length; i++)
            {
                string filePath = filePaths[i];
                string filename = Path.GetFileName(filePath);
                if (_modifiedFiles.Contains(filename))
                    continue;
                File.Delete(filePath);
            }
        }

        public static void RemoveFiles(string directory, string fileExt)
        {
            string[] filePaths = Directory.GetFiles(directory, $"*.{fileExt}");
            for (int i = 0; i < filePaths.Length; i++)
            {
                string file = filePaths[i];
                File.Delete(file);
            }
        }

        public static void RemoveUnusedMetaFiles(string directory)
        {
            string[] filePaths = Directory.GetFiles(directory);
            HashSet<string> nonMetaFiles = new HashSet<string>();
            for (int i = 0; i < filePaths.Length; i++)
            {
                string file = Path.GetFileName(filePaths[i]);
                if (file.EndsWith(kMetaSuffix))
                    continue;
                nonMetaFiles.Add(file);
            }

            for (int i = 0; i < filePaths.Length; i++)
            {
                string filepath = filePaths[i];
                string file = Path.GetFileName(filepath);
                if (!file.EndsWith(kMetaSuffix))
                    continue;
                var nonMetaFileName = file.Substring(0, file.Length - kMetaSuffix.Length);
                if (!nonMetaFiles.Contains(nonMetaFileName))
                    File.Delete(filepath);
            }
        }
    }
}
