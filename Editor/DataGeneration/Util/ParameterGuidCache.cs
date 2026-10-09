using System.Collections.Generic;
using System.IO;
using PocketGems.Parameters.Common.Editor;

namespace PocketGems.Parameters.DataGeneration.Util.Editor
{
    /// <summary>
    /// Guids of the Scriptable Objects seen by the last parameter generations, saved to disk.
    ///
    /// A deleted asset's type can no longer be read, so this is used to tell whether it was a parameter.
    /// </summary>
    internal static class ParameterGuidCache
    {
        private static string s_filePath = EditorParameterConstants.GeneratedAsset.GuidsFilePath;
        private static HashSet<string> s_guids;

        /// <summary>
        /// File the guids are saved to.  Setting it drops the loaded guids so they are read again.
        /// </summary>
        internal static string FilePath
        {
            get => s_filePath;
            set
            {
                s_filePath = value;
                s_guids = null;
            }
        }

        /// <summary>
        /// Checks if an asset could be a parameter.
        /// </summary>
        /// <param name="guid">guid of the asset</param>
        /// <returns>false only if the guids are saved and this one wasn't a parameter in the last generations</returns>
        public static bool MightBeParameter(string guid)
        {
            var guids = Load();
            return guids == null || guids.Contains(guid);
        }

        /// <summary>
        /// Replaces all saved guids.  Called after loading all Scriptable Objects.
        /// </summary>
        public static void Replace(IEnumerable<string> guids)
        {
            s_guids = new HashSet<string>(guids);
            Save();
        }

        /// <summary>
        /// Adds to the saved guids.  Called after loading only the changed Scriptable Objects.
        /// </summary>
        public static void Add(IEnumerable<string> guids)
        {
            s_guids = Load() ?? new HashSet<string>();
            var newGuids = new List<string>();
            foreach (var guid in guids)
                if (s_guids.Add(guid))
                    newGuids.Add(guid);
            if (newGuids.Count == 0)
                return;

            // append only the new guids unless the file is gone, so it doesn't end up with only these
            if (File.Exists(s_filePath))
                File.AppendAllLines(s_filePath, newGuids);
            else
                Save();
        }

        private static HashSet<string> Load()
        {
            if (s_guids == null && File.Exists(s_filePath))
                s_guids = new HashSet<string>(File.ReadAllLines(s_filePath));
            return s_guids;
        }

        private static void Save()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(s_filePath));
            File.WriteAllLines(s_filePath, s_guids);
        }
    }
}
