using System.Collections.Generic;
using System.IO;
using PocketGems.Parameters.Editor.LocalCSV;
using PocketGems.Parameters.Editor.Operation;
using PocketGems.Parameters.Models;
using PocketGems.Parameters.Util;

namespace PocketGems.Parameters.Operations.Data
{
    internal class ReadLocalCSVOperation : BasicOperation<IDataOperationContext>
    {
        public override void Execute(IDataOperationContext context)
        {
            base.Execute(context);

            // we're going to regenerate all CSVs anyways - exit early
            if (context.GenerateDataType != GenerateDataType.CSVDiff ||
                context.GenerateDataType == GenerateDataType.All)
                return;

            // csv directory
            var directory = context.GeneratedLocalCSVDirectory;
            if (!Directory.Exists(directory))
            {
                ParameterDebug.LogVerbose($"CSV directory doesn't exist: regenerating");
                context.GenerateDataType = GenerateDataType.All;
                return;
            }

            HashSet<string> missingCSVForInterface = new HashSet<string>();

            // get all interface names
            Dictionary<string, IParameterInfo> parameterInfoNames = new Dictionary<string, IParameterInfo>();
            for (int i = 0; i < context.ParameterInfos.Count; i++)
            {
                missingCSVForInterface.Add(context.ParameterInfos[i].InterfaceName);
                parameterInfoNames[context.ParameterInfos[i].InterfaceName] = context.ParameterInfos[i];
            }

            Dictionary<string, IParameterStruct> parameterStructNames = new Dictionary<string, IParameterStruct>();
            for (int i = 0; i < context.ParameterStructs.Count; i++)
            {
                missingCSVForInterface.Add(context.ParameterStructs[i].InterfaceName);
                parameterStructNames[context.ParameterStructs[i].InterfaceName] = context.ParameterStructs[i];
            }

            // delete any CSVs that doesn't map to interface & find missing CSVs
            var files = Directory.GetFiles(directory,
                $"*{EditorParameterConstants.CSV.FileExtension}",
                SearchOption.AllDirectories);

            for (int i = 0; i < files.Length; i++)
            {
                var csvFilePath = files[i];
                var csvFilename = Path.GetFileName(csvFilePath);
                var baseName = NamingUtil.BaseNameFromCSVName(csvFilename);
                var infoInterfaceName = NamingUtil.InfoInterfaceNameFromBaseName(baseName);
                var structInterfaceName = NamingUtil.StructInterfaceNameFromBaseName(baseName);
                if (missingCSVForInterface.Contains(infoInterfaceName) ||
                    missingCSVForInterface.Contains(structInterfaceName))
                {
                    missingCSVForInterface.Remove(infoInterfaceName);
                    missingCSVForInterface.Remove(structInterfaceName);
                    continue;
                }

                File.Delete(csvFilePath);
                ParameterDebug.Log($"Deleting CSV that doesn't match any interfaces [{csvFilePath}]");
            }

            if (missingCSVForInterface.Count > 0)
            {
                foreach (var interfaceName in missingCSVForInterface)
                    ParameterDebug.LogVerbose($"CSV for interface [{interfaceName}] doesn't exist: regenerating");
                context.GenerateDataType = GenerateDataType.All;
                return;
            }

            // read & map modified CSVs
            var csvFilePaths = context.ModifiedCSVPaths;
            for (int i = 0; i < csvFilePaths.Count; i++)
            {
                // TODO catch for exceptions? - format can be messed up
                var fileName = Path.GetFileNameWithoutExtension(csvFilePaths[i]);
                var baseName = NamingUtil.BaseNameFromCSVName(fileName);
                var interfaceName = NamingUtil.InfoInterfaceNameFromBaseName(baseName);
                if (parameterInfoNames.TryGetValue(interfaceName, out IParameterInfo parameterInfo))
                {
                    context.InfoCSVFileCache.Load(parameterInfo);
                    continue;
                }

                var structName = NamingUtil.StructInterfaceNameFromBaseName(baseName);
                if (parameterStructNames.TryGetValue(structName, out IParameterStruct parameterStruct))
                {
                    context.StructCSVFileCache.Load(parameterStruct);
                    continue;
                }

                ParameterDebug.LogError($"Info or Struct cannot be found for CSV {fileName}");
                context.GenerateDataType = GenerateDataType.All;
                return;
            }

            // check loaded CSVs for schema changes
            var assemblyName = context.GeneratedCodeEditorAssemblyName;
            var errors = CSVUtil.InvokeCheckSchema(
                context.InfoCSVFileCache, context.StructCSVFileCache, assemblyName);
            if (errors?.Count > 0)
            {
                foreach (var error in errors)
                    ParameterDebug.LogError(error);
                context.GenerateDataType = GenerateDataType.All;
                return;
            }

            void CheckSchemaModified<T, G>(ICSVFileCache<T, G> csvFileCache) where G : IParameterInterface
            {
                foreach (var kvp in csvFileCache.LoadedFiles())
                {
                    if (kvp.Value.ColumnSchemaModified)
                    {
                        ParameterDebug.LogError($"CSV {kvp.Value.Filename} columns moved: regenerating");
                        context.GenerateDataType = GenerateDataType.All;
                        return;
                    }
                }
            }

            CheckSchemaModified(context.InfoCSVFileCache);
            CheckSchemaModified(context.StructCSVFileCache);
        }
    }
}
