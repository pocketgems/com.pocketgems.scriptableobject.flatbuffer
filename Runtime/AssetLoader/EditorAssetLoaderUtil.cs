#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace PocketGems.Parameters.AssetLoader
{
    internal static class EditorAssetLoaderUtil
    {
        /// <summary>
        /// API used by editor tools to hot load parameters into the app at runtime.
        /// </summary>
        /// <param name="editorFilePath">The file path to teh flatbuffer to load.</param>
        /// <param name="parameterDataLoader">Data Loader to use.</param>
        /// <param name="parameterManager">Parameter manage to load into.</param>
        public static void HotLoadResource(string editorFilePath, IParameterDataLoader parameterDataLoader,
            IMutableParameterManager parameterManager)
        {
            // This loads directly from file rather than asset data base.  This ie because there seems to be a delay
            // with AssetDatabase update even when using force import synchronously options.
            var bytes = File.ReadAllBytes(editorFilePath);
            parameterDataLoader.LoadData(parameterManager, bytes);
        }

        /// <summary>
        /// Gets parameter files that were generated
        /// </summary>
        /// <param name="directoryPath">the directory to search</param>
        /// <param name="getMainFile">only return the path to the main (non iteration) parameter file</param>
        /// <returns></returns>
        public static List<string> GetGeneratedParameterFiles(string directoryPath, bool getMainFile)
        {
            /*
             * In the editor we search for all other .byte files generated that isn't the root parameter file.
             *
             * During iteration in the editor, the BuildDataBufferOperation.cs regenerates .byte files for
             * parameter files that had changed data to optimize iteration speed.  This is done in
             * BuildDataBufferOperation.cs.
             */
            var files = new List<string>();
            if (!Directory.Exists(directoryPath))
                return files;
            var editorFiles = Directory.GetFiles(directoryPath);
            var extension = ParameterConstants.GeneratedParameterAssetFileExtension;
            for (int i = 0; i < editorFiles.Length; i++)
            {
                var editorFile = editorFiles[i];

                // non parameter file (e.g. .meta file)
                if (Path.GetExtension(editorFile) != extension)
                    continue;

                // check if it's the main resource
                var editorResourceName = Path.GetFileNameWithoutExtension(editorFile);
                if (getMainFile && editorResourceName != ParameterConstants.GeneratedAssetName)
                    continue;
                if (!getMainFile && editorResourceName == ParameterConstants.GeneratedAssetName)
                    continue;
                files.Add(editorFile);
            }

            return files;
        }

        public static bool DirectlyLoadMainFile(IParameterHotLoader parameterHotLoader, string directoryPath)
        {
            if (parameterHotLoader.Status == ParameterAssetLoaderStatus.Failed)
                return false;
            var files = GetGeneratedParameterFiles(directoryPath, true);
            if (files.Count != 1)
            {
                Debug.LogError("Couldn't find exactly one main parameter file.");
                return false;
            }

            parameterHotLoader.HotLoadResource(files[0]);
            return true;
        }

        public static bool DirectlyLoadIterationFiles(IParameterHotLoader parameterHotLoader, string directoryPath, bool prompt)
        {
            if (parameterHotLoader.Status == ParameterAssetLoaderStatus.Failed)
            {
                if (prompt)
                {
                    EditorUtility.DisplayDialog("Parameter Error",
                        $"Parameter data file was not generated at {ParameterConstants.GeneratedAsset.MainAssetPath}.  " +
                        $"To regenerate, go to Menu --> Pocket Gems --> Parameters -> Config --> Generate Data.  " +
                        $"If this continues to be a recurring event, please contact an engineer.",
                        "Okay");
                }
                return false;
            }

            var files = GetGeneratedParameterFiles(directoryPath, false);
            foreach (var file in files)
                parameterHotLoader.HotLoadResource(file);
            return true;
        }
    }
}
#endif
