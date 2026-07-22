using System.Collections.Generic;
using System.IO;
using System.Text;
using PocketGems.Parameters.Common.Editor;
using PocketGems.Parameters.Common.Models.Editor;
using PocketGems.Parameters.Common.Operations.Editor;
using PocketGems.Parameters.Common.Util.Editor;
using PocketGems.Parameters.DataGeneration.LocalCSV.Editor;
using PocketGems.Parameters.DataGeneration.Operation.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace PocketGems.Parameters.DataGeneration.Operations.Editor
{
    internal class WriteLocalCSVOperation : BasicOperation<IDataOperationContext>
    {
        private IDataOperationContext _context;

        public override void Execute(IDataOperationContext context)
        {
            _context = context;
            base.Execute(context);

            // A full regeneration is already queued (a CSV row deletion removed Scriptable Objects). That pass
            // regenerates all CSVs from scratch (GenerateCSVs), so skip the redundant CSV write here.
            if (context.GenerateAllAgain)
                return;

            if (context.GenerateDataType == GenerateDataType.IfNeeded)
                return;

            // Explicit "Generate CSVs" (menu): (re)write all CSVs from the current Scriptable Objects.
            if (context.GenerateCSVs)
            {
                WriteAllScriptableObjectsToCSVs(context);
                return;
            }

            if (context.GenerateDataType == GenerateDataType.CSVDiff)
            {
                // The developer is editing CSVs directly - sync their changes back into the CSV file(s).
                WriteChangedScriptableObjectsToCSVs(context);
                return;
            }

            // A Scriptable Object changed. CSVs are no longer written automatically and are now out of
            // date, so delete them (prompting first). CSVs are an opt-in working copy regenerated on
            // demand via the "Generate CSVs" menu.
            DeleteLocalCSVs(context);
        }

        private void WriteAllScriptableObjectsToCSVs(IDataOperationContext context)
        {
            EnsureCSVDirectoryExists(context);

            var infoCSVFileCache = context.InfoCSVFileCache;
            var structCSVFileCache = context.StructCSVFileCache;

            // write all scriptable objects to CSVs
            infoCSVFileCache.ClearCache();
            infoCSVFileCache.AttemptLoadExistingOnLoad = false;
            structCSVFileCache.ClearCache();
            structCSVFileCache.AttemptLoadExistingOnLoad = false;

            for (int i = 0; i < context.ParameterInfos.Count; i++)
            {
                var parameterInterface = context.ParameterInfos[i];
                context.ScriptableObjectMetadatas.TryGetValue(parameterInterface, out var metadatas);
                UpdateCSV(parameterInterface, metadatas);
            }

            // load structs that might've not been populated due to empty struct lists to write out
            for (int i = 0; i < context.ParameterStructs.Count; i++)
                context.StructCSVFileCache.Load(context.ParameterStructs[i]);

            if (Errors.Count == 0)
                WriteAllCSVs("Generated CSV(s) from Scriptable Objects");
        }

        private void WriteChangedScriptableObjectsToCSVs(IDataOperationContext context)
        {
            EnsureCSVDirectoryExists(context);

            context.InfoCSVFileCache.AttemptLoadExistingOnLoad = true;
            context.StructCSVFileCache.AttemptLoadExistingOnLoad = true;
            // write changes to scriptable objects to changed rows
            foreach (var kvp in context.ScriptableObjectMetadatas)
                UpdateCSV(kvp.Key, kvp.Value);

            if (Errors.Count == 0)
                WriteAllCSVs("Wrote modifications in Scriptable Objects to CSV(s)");
        }

        /// <summary>
        /// Deletes all local CSVs because the Scriptable Objects (the source of truth) changed and the CSVs
        /// would now be stale. Prompts first (outside of automated contexts) so a developer with unsynced CSV
        /// edits can bail out and save them.
        /// </summary>
        private void DeleteLocalCSVs(IDataOperationContext context)
        {
            var directory = context.GeneratedLocalCSVDirectory;
            if (!Directory.Exists(directory))
                return;

            var csvFiles = Directory.GetFiles(directory,
                $"*{EditorParameterConstants.CSV.FileExtension}", SearchOption.AllDirectories);
            if (csvFiles.Length == 0)
                return;

            if (!ConfirmDeleteLocalCSVs())
                return;

            AssetDatabase.StartAssetEditing();
            try
            {
                for (int i = 0; i < csvFiles.Length; i++)
                    AssetDatabase.DeleteAsset(NamingUtil.RelativePath(csvFiles[i]));
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            ParameterDebug.Log($"Deleted {csvFiles.Length} local parameter CSV(s) because Scriptable Object(s) " +
                               "changed. Regenerate them via Pocket Gems → Parameters → Generate CSVs.");
        }

        /// <summary>
        /// Prompts the developer (via a modal dialog) to confirm deleting the outdated local CSVs, returning
        /// whether the deletion should proceed. The prompt is skipped - and deletion allowed - in contexts
        /// where a modal can't be answered (batch builds, unit tests).
        ///
        /// Excluded from coverage: the modal dialog can't be exercised in an automated test.
        /// </summary>
        [ExcludeFromCoverage]
        private bool ConfirmDeleteLocalCSVs()
        {
            if (Application.isBatchMode || UnitTestListener.AreUnitTestsRunning)
                return true;

            return EditorUtility.DisplayDialog(
                "Delete Local Parameter CSVs?",
                "A parameter Scriptable Object changed, so the local CSV(s) are now out of date.\n\n" +
                "If you don't edit the CSVs directly, it's safe to Delete them — the Scriptable " +
                "Objects are the source of truth, and CSVs can be regenerated anytime via " +
                "Pocket Gems → Parameters → Generate CSVs.\n\n" +
                "Only choose Keep if you have unsynced edits in the CSV(s) you still need. Save " +
                "the CSV(s) afterward to sync them back to the Scriptable Objects.",
                "Delete CSVs", "Keep Outdated CSVs");
        }

        private void EnsureCSVDirectoryExists(IDataOperationContext context)
        {
            var directory = context.GeneratedLocalCSVDirectory;
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
        }

        private void UpdateCSV(IParameterInfo parameterInfo, List<IScriptableObjectMetadata> scriptableObjectMetadatas)
        {
            var assemblyName = _context.GeneratedCodeEditorAssemblyName;
            var errors = CSVUtil.InvokeUpdateFromScriptableObjects(
                _context.InfoCSVFileCache,
                _context.StructCSVFileCache,
                assemblyName,
                parameterInfo.Type,
                scriptableObjectMetadatas);
            for (int i = 0; i < errors.Count; i++)
                Error(errors[i]);
        }

        private void WriteAllCSVs(string description)
        {
            List<string> filesWrote = new List<string>();
            void WriteCSVs(IReadOnlyDictionary<string, CSVFile> csvFiles)
            {
                foreach (var kvp in csvFiles)
                {
                    var baseName = kvp.Key;
                    var csvFile = kvp.Value;
                    if (!csvFile.IsDirty)
                        continue;

                    CSVUtil.InvokeDefineSchema(baseName, csvFile, _context.GeneratedCodeEditorAssemblyName);
                    csvFile.InterfaceHash = _context.InterfaceAssemblyHash;
                    if (csvFile.Write())
                        filesWrote.Add(csvFile.FilePath);
                }
            }

            WriteCSVs(_context.InfoCSVFileCache.LoadedFiles());
            WriteCSVs(_context.StructCSVFileCache.LoadedFiles());

            if (filesWrote.Count == 0)
                return;

            // Import the CSVs we just wrote so the AssetDatabase reflects them without a whole-project
            // refresh. Batched via Start/StopAssetEditing since a full generation can write many files.
            AssetDatabase.StartAssetEditing();
            try
            {
                for (int i = 0; i < filesWrote.Count; i++)
                    AssetDatabase.ImportAsset(NamingUtil.RelativePath(filesWrote[i]));
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            StringBuilder logBuilder = new($"{description}: ");
            if (filesWrote.Count > 1)
                logBuilder.AppendLine();
            for (int i = 0; i < filesWrote.Count; i++)
                logBuilder.AppendLine(filesWrote[i]);
            ParameterDebug.Log(logBuilder.ToString());
        }
    }
}
