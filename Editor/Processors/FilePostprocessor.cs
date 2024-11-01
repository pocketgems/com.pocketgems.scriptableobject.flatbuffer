using System.Collections.Generic;
using UnityEditor;

namespace PocketGems.Parameters.Processors.Editor
{
    internal delegate bool IsValidFile(string filePath);
    internal delegate void OnFilesChanged(List<string> createdOrChanged, List<string> deleted,
        List<string> movedTo, List<string> movedFrom);

    /// <summary>
    /// Parameter file watcher to observe ".csv" file changes.
    /// </summary>
    internal class FilePostprocessor : AssetPostprocessor
    {
        private static List<(IsValidFile, OnFilesChanged)> s_callbackDelegates;

        /// <summary>
        /// Adds an observer for ".csv" file changes in the specified directory path.
        /// </summary>
        /// <param name="fileCheckDelegate">Delegate to check if the file is a valid one to trigger a file change event.</param>
        /// <param name="callbackDelegate">Delegate to call upon file changes.</param>
        public static void AddObserver(IsValidFile fileCheckDelegate, OnFilesChanged callbackDelegate)
        {
            if (s_callbackDelegates == null)
                s_callbackDelegates = new List<(IsValidFile, OnFilesChanged)>();

            s_callbackDelegates.Add((fileCheckDelegate, callbackDelegate));
        }

        /// <summary>
        /// Remove an observer.
        /// </summary>
        /// <param name="fileCheckDelegate">Delegate to remove.</param>
        /// <param name="callbackDelegate">Delegate to remove.</param>
        public static bool RemoveObserver(IsValidFile fileCheckDelegate, OnFilesChanged callbackDelegate)
        {
            for (int i = 0; i < s_callbackDelegates.Count; i++)
            {
                var d = s_callbackDelegates[i];
                if (d.Item1 == fileCheckDelegate && d.Item2 == callbackDelegate)
                {
                    s_callbackDelegates.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// The callback from the AssetPostprocessor from unity that informs us of chagnes.
        /// </summary>
        /// <param name="importedAssets">assets that were created or modified</param>
        /// <param name="deletedAssets">deleted assets</param>
        /// <param name="movedAssets">path where an assets moved to</param>
        /// <param name="movedFromAssets">complementing paths where assets moved from</param>
        internal static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets,
            string[] movedFromAssets)
        {
            if (s_callbackDelegates == null) return;
            for (int i = 0; i < s_callbackDelegates.Count; i++)
            {
                var d = s_callbackDelegates[i];
                Process(importedAssets, deletedAssets, movedAssets, movedFromAssets, d.Item1, d.Item2);
            }
        }

        private static void Process(string[] importedAssets, string[] deletedAssets, string[] movedAssets,
            string[] movedFromAssets, IsValidFile fileCheckDelegate, OnFilesChanged callbackDelegate)
        {
            List<string> createdOrChanged = null;
            List<string> deleted = null;
            List<string> movedFrom = null;
            List<string> movedTo = null;

            foreach (string assetPath in importedAssets)
            {
                if (fileCheckDelegate(assetPath))
                {
                    if (createdOrChanged == null)
                        createdOrChanged = new List<string>();
                    createdOrChanged.Add(assetPath);
                }
            }

            foreach (string assetPath in deletedAssets)
            {
                if (fileCheckDelegate(assetPath))
                {
                    if (deleted == null)
                        deleted = new List<string>();
                    deleted.Add(assetPath);
                }
            }

            for (int i = 0; i < movedAssets.Length; i++)
            {
                var movedToAssetPath = movedAssets[i];
                var movedFromAssetPath = movedFromAssets[i];
                if (fileCheckDelegate(movedToAssetPath) ||
                    fileCheckDelegate(movedFromAssetPath))
                {
                    if (movedTo == null || movedFrom == null)
                    {
                        movedFrom = new List<string>();
                        movedTo = new List<string>();
                    }

                    movedTo.Add(movedToAssetPath);
                    movedFrom.Add(movedFromAssetPath);
                }
            }

            if (createdOrChanged != null || deleted != null || movedTo != null || movedFrom != null)
            {
                callbackDelegate(createdOrChanged, deleted, movedTo, movedFrom);
            }
        }
    }
}
