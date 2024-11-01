using System.Collections.Generic;
using System.IO;
using System.Text;
using PocketGems.Parameters.Common.Models.Editor;
using PocketGems.Parameters.Common.Operations.Editor;
using PocketGems.Parameters.Common.Util.Editor;
using PocketGems.Parameters.DataGeneration.LocalCSV.Editor;
using PocketGems.Parameters.DataGeneration.Operation.Editor;

namespace PocketGems.Parameters.DataGeneration.Operations.Editor
{
    internal class WriteLocalCSVOperation : BasicOperation<IDataOperationContext>
    {
        private IDataOperationContext _context;

        public override void Execute(IDataOperationContext context)
        {
            _context = context;
            base.Execute(context);

            if (context.GenerateDataType == GenerateDataType.IfNeeded)
                return;

            var infoCSVFileCache = context.InfoCSVFileCache;
            var structCSVFileCache = context.StructCSVFileCache;
            var directory = context.GeneratedLocalCSVDirectory;
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            if (context.GenerateDataType == GenerateDataType.All)
            {
                // write all scriptable objects to CSVs
                infoCSVFileCache.ClearCache();
                infoCSVFileCache.AttemptLoadExistingOnLoad = false;
                structCSVFileCache.ClearCache();
                structCSVFileCache.AttemptLoadExistingOnLoad = false;

                for (int i = 0; i < context.ParameterInfos.Count; i++)
                {
                    var parameterInterface = context.ParameterInfos[i];
                    List<IScriptableObjectMetadata> metadatas;
                    context.ScriptableObjectMetadatas.TryGetValue(parameterInterface, out metadatas);
                    UpdateCSV(parameterInterface, metadatas);
                }

                // load structs that might've not been populated due to empty struct lists to write out
                for (int i = 0; i < context.ParameterStructs.Count; i++)
                {
                    var parameterInterface = context.ParameterStructs[i];
                    context.StructCSVFileCache.Load(parameterInterface);
                }

                if (Errors.Count == 0)
                    WriteAllCSVs("Wrote all Scriptable Objects to CSV(s)");
            }
            else if (context.GenerateDataType == GenerateDataType.ScriptableObjectDiff ||
                     context.GenerateDataType == GenerateDataType.CSVDiff)
            {
                infoCSVFileCache.AttemptLoadExistingOnLoad = true;
                structCSVFileCache.AttemptLoadExistingOnLoad = true;
                // write changes to scriptable objects to changed rows
                foreach (var kvp in context.ScriptableObjectMetadatas)
                {
                    var parameterInterface = kvp.Key;
                    UpdateCSV(parameterInterface, kvp.Value);
                }

                if (Errors.Count == 0)
                    WriteAllCSVs("Wrote modifications in Scriptable Objects to CSV(s)");
            }
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

            StringBuilder logBuilder = new StringBuilder($"{description}: ");
            if (filesWrote.Count > 1)
                logBuilder.AppendLine();
            for (int i = 0; i < filesWrote.Count; i++)
                logBuilder.AppendLine(filesWrote[i]);
            ParameterDebug.Log(logBuilder.ToString());
        }
    }
}
