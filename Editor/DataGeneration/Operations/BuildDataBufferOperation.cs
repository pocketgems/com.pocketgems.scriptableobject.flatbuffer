using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using PocketGems.Parameters.Common.Editor;
using PocketGems.Parameters.Common.Models.Editor;
using PocketGems.Parameters.Common.Operations.Editor;
using PocketGems.Parameters.Common.Util.Editor;
using PocketGems.Parameters.DataGeneration.Operation.Editor;
using UnityEditor;
using UnityEngine.TestTools;

namespace PocketGems.Parameters.DataGeneration.Operations.Editor
{
    /// <summary>
    /// Builds various flatbuffer byte files.
    /// </summary>
    [ExcludeFromCoverage]
    internal class BuildDataBufferOperation : BasicOperation<IDataOperationContext>
    {
        #region IParameterOperation

        public override void Execute(IDataOperationContext context)
        {
            base.Execute(context);

            var outputDirectory = context.GeneratedAssetDirectory;
            if (context.GenerateDataType == GenerateDataType.All)
            {
                var outputFilename = context.GeneratedAssetFileName;
                // delete all other intermediate parameter files since this is re-generating all data into one byte file
                if (Directory.Exists(outputDirectory))
                    FileUtil.DeleteFileOrDirectory(outputDirectory);

                var outputPath = Path.Combine(outputDirectory, outputFilename);

                var typeToSoDict = context.ScriptableObjectMetadatas.ToDictionary(kvp => kvp.Key.Type, kvp => kvp.Value);
                var log = GenerateAndWrite(context, outputPath, typeToSoDict);
                ParameterDebug.Log(log);
            }
            else if (context.GenerateDataType == GenerateDataType.ScriptableObjectDiff ||
                     context.GenerateDataType == GenerateDataType.CSVDiff)
            {
                List<string> logs = new List<string>();
                /*
                 * generate one data file for each Scriptable Object
                 *
                 * this goes hand in hand with ResourcesParameterDataLoader.cs which searches for each individual
                 * file only in the editor.
                 */
                foreach (var typeToObjects in context.ScriptableObjectMetadatas)
                {
                    var scriptableObjects = typeToObjects.Value;
                    for (int i = 0; i < scriptableObjects.Count; i++)
                    {
                        var metadata = scriptableObjects[i];
                        var scriptableObject = metadata.ScriptableObject;
                        var fileName = EditorParameterConstants.GeneratedAsset.AdditiveFileName(typeToObjects.Key, scriptableObject);
                        var outputPath = Path.Combine(outputDirectory, fileName);

                        var soDict = new Dictionary<Type, List<IScriptableObjectMetadata>>();
                        soDict[typeToObjects.Key.Type] = new List<IScriptableObjectMetadata> { metadata };

                        var log = GenerateAndWrite(context, outputPath, soDict);
                        logs.Add(log);
                    }
                }

                // if the string has too many new lines, Unity's console throws a never ending error.
                // partition by this constant
                const int lineLimit = 50;
                if (logs.Count > 0)
                {
                    StringBuilder logBuilder = null;
                    var logCount = logs.Count;
                    var totalParts = Math.Ceiling((float)logCount / lineLimit);
                    int part = 0;
                    for (int i = 0; i < logs.Count; i++)
                    {
                        if (i % lineLimit == 0)
                        {
                            part++;
                            if (logBuilder != null)
                                ParameterDebug.Log(logBuilder.ToString());
                            if (totalParts > 1)
                                logBuilder = new StringBuilder($"Generated Data Diff (Part {part}):\n");
                            else
                                logBuilder = new StringBuilder("Generated Data Diff:\n");
                        }
                        logBuilder.AppendLine(logs[i]);
                    }
                    ParameterDebug.Log(logBuilder.ToString());
                }
            }
        }

        #endregion

        /// <summary>
        /// Use reflection to invoke a static class in the auto generated class to populate the FlatBuffer
        /// </summary>
        /// <param name="context">the execution context</param>
        /// <param name="outputFile">flat buffer byte file to write to</param>
        /// <param name="scriptableObjectsMetadatas">objects to populate the buffer with</param>
        /// <returns></returns>
        private int InvokeGenerate(IDataOperationContext context,
            string outputFile,
            Dictionary<Type, List<IScriptableObjectMetadata>> scriptableObjectsMetadatas)
        {
            var editorAssemblyName = EditorParameterConstants.CodeGeneration.EditorAssemblyName;
            // we must use reflection here because the generated class & assembly isn't guaranteed to exist
            // therefore compilation will fail
            var assembly = AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault(assembly =>
                assembly.GetName().Name == editorAssemblyName);

            if (assembly == null)
            {
                Error($"Couldn't find assembly {editorAssemblyName}");
                return 0;
            }

            string generatedNamespace = ParameterConstants.GeneratedNamespace;
            string className = EditorParameterConstants.FlatBufferBuilderClass.ClassName;
            string methodName = EditorParameterConstants.FlatBufferBuilderClass.MethodName;

            var typeString = $"{generatedNamespace}.{className}";
            var type = assembly.GetType(typeString);
            if (type == null)
            {
                Error($"Cannot find class [{typeString}]. Your code might be in an intermediate state.  " +
                      $"Try to run from the menu \"{MenuItemConstants.RegenerateCodePath}\" to fix this.");
                return 0;
            }

            var methodInfo = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static);
            if (methodInfo == null)
            {
                Error($"Cannot find method {methodName} in type {type}");
                return 0;
            }

            var args = new object[] { outputFile, context.InterfaceAssemblyHash, scriptableObjectsMetadatas };
            return (int)methodInfo.Invoke(null, args);
        }

        private string GenerateAndWrite(IDataOperationContext context,
            string outputFile,
            Dictionary<Type, List<IScriptableObjectMetadata>> scriptableObjectsMetadatas)
        {
            var stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();
            int fileBytes = InvokeGenerate(context, outputFile, scriptableObjectsMetadatas);
            stopWatch.Stop();

            context.GeneratedFilePaths.Add(outputFile);

            var fileSizeString = $"{fileBytes}B";
            if (fileBytes > 1000000)
                fileSizeString = $"{(fileBytes / 1000000.0):N2}MB";
            else if (fileBytes > 1000)
                fileSizeString = $"{(fileBytes / 1000.0):N2}KB";
            var relativePath = NamingUtil.RelativePath(outputFile);
            return $"Generated Data {relativePath} ({fileSizeString}) in {stopWatch.ElapsedMilliseconds}ms";
        }
    }
}
