using PocketGems.Parameters.Common.Operations.Editor;
using PocketGems.Parameters.Common.Util.Editor;
using PocketGems.Parameters.DataGeneration.LocalCSV.Editor;
using PocketGems.Parameters.DataGeneration.Operation.Editor;
using UnityEditor;
using UnityEngine.TestTools;

namespace PocketGems.Parameters.DataGeneration.Operations.Editor
{
    [ExcludeFromCoverage] // exclude for now TODO revisit
    internal class UpdateScriptableObjectsOperation : BasicOperation<IDataOperationContext>
    {
        public override void Execute(IDataOperationContext context)
        {
            base.Execute(context);

            if (context.GenerateDataType != GenerateDataType.CSVDiff)
                return;

            foreach (var kvp in context.ScriptableObjectMetadatas)
            {
                var assemblyName = context.GeneratedCodeEditorAssemblyName;
                var parameterInterface = kvp.Key;
                var metadatas = kvp.Value;
                var errors = CSVUtil.InvokeReadToScriptableObjects(
                    context.InfoCSVFileCache,
                    context.StructCSVFileCache,
                    assemblyName,
                    parameterInterface.Type,
                    metadatas);
                for (int i = 0; i < errors.Count; i++)
                    Error(errors[i]);
                for (int i = 0; i < metadatas.Count; i++)
                {
                    var so = metadatas[i].ScriptableObject;
                    EditorUtility.SetDirty(so);
                    ParameterDebug.LogVerbose($"Updated [{so}] from csv.");
                }
            }

            AssetDatabase.SaveAssets();

            // search for struct rows that did not match any scriptalbe objects
            var loadedCSVFiles = context.StructCSVFileCache.LoadedFiles();
            var readCSVRows = context.StructCSVFileCache.LoadRowHistory;
            foreach (var kvp in loadedCSVFiles)
            {
                var baseName = kvp.Key;
                var csvFile = kvp.Value;
                var rowDatas = csvFile.RowData;
                for (int i = 0; i < rowDatas.Count; i++)
                {
                    var rowData = rowDatas[i];
                    if (rowData.HashMatches)
                        continue;

                    if (!readCSVRows.TryGetValue(baseName, out var guids) ||
                        !guids.Contains(rowData.GUID))
                    {
                        Error($"{csvFile.FilePath} has key path that doesn't match any objects/properties {rowData.GUID}.  " +
                              $"If this has array indexes, ensure that they are all contiguous with the other indexes (don't skip).");
                    }
                }
            }
        }
    }
}
