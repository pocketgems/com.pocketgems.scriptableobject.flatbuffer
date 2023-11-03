using System;
using System.IO;
using PocketGems.Parameters.Util;
using UnityEditor;
using UnityEngine.TestTools;

namespace PocketGems.Parameters
{
    [ExcludeFromCoverage]
    internal class ParameterSetup
    {
        public static bool SetupEnvironment(bool force)
        {
            var setup = new ParameterSetup(force);
            return setup.Setup();
        }

        private readonly bool _force;
        private bool _unityAssetChanges;

        private ParameterSetup(bool force)
        {
            _force = force;
        }

        private void CreateFolder(string directoryPath)
        {
            if (Directory.Exists(directoryPath))
                return;

            Directory.CreateDirectory(directoryPath);
            _unityAssetChanges = true;

            ParameterDebug.Log($"Created Directory: {directoryPath}");
        }

        private void CreateAssemblyInfo(string filePath)
        {
            // do not force re-writing of assembly infos since game teams can modify these
            if (File.Exists(filePath))
                return;

            CodeGenerator.GenerateAssemblyInfo(filePath);
            _unityAssetChanges = true;

            ParameterDebug.Log($"Created Assembly Info: {filePath}");
        }

        private void CreateAssembly(string assemblyName, string directoryPath, Action<AssemblyDefinitionFile> setup = null)
        {
            var assemblyPath = Path.Combine(directoryPath, $"{assemblyName}.asmdef");
            if (!_force && File.Exists(assemblyPath))
                return;

            var assembly = new AssemblyDefinitionFile(assemblyName);
            assembly.autoReferenced = true;
            setup?.Invoke(assembly);
            assembly.WriteFile(directoryPath);
            _unityAssetChanges = true;

            ParameterDebug.Log($"Created Assembly: {assemblyPath}");
        }

        private void AddGitIgnore(string comment, string directoryPath)
        {
            // add generated folder & meta to git ignore
            var gitIgnoreFilePath = ".gitignore";
            var gitIgnoreContents = "";
            if (File.Exists(gitIgnoreFilePath))
                gitIgnoreContents = File.ReadAllText(gitIgnoreFilePath);

            var relDir = NamingUtil.RelativePath(directoryPath);
            // save as unix path for git ignore
            relDir = relDir.Replace(Path.DirectorySeparatorChar, '/');
            var relDirMeta = relDir + ".meta";
            if (!gitIgnoreContents.Contains(relDir))
            {
                gitIgnoreContents += $"\n# {comment}";
                gitIgnoreContents += $"\n{relDir}";
                gitIgnoreContents += $"\n{relDirMeta}";
                File.WriteAllText(gitIgnoreFilePath, gitIgnoreContents);
                // do not need to mark as true
                // Git ignore changes doesn't affect anything within unity - no need to wait for a re-import.
                // _unityAssetChanges = true;
            }
        }

        /// <summary>
        /// Returns true if asset changes were made and the editor will re-import.
        /// </summary>
        /// <param name="force">forces re-writing original setup files</param>
        /// <returns>true if unity assets changed</returns>
        private bool Setup(bool force = false)
        {
            var interfaceAssemblyName = EditorParameterConstants.Interface.AssemblyName;

            // scriptable object folder
            CreateFolder(ParameterConstants.ScriptableObject.Dir);

            // interface folder & assembly
            CreateFolder(EditorParameterConstants.Interface.DirRoot);
            CreateAssembly(interfaceAssemblyName,
                EditorParameterConstants.Interface.DirRoot, a =>
                {
                    a.references = new[]
                    {
                        EditorParameterConstants.AssemblyName.ParametersRuntime,
                        EditorParameterConstants.AssemblyName.Addressables
                    };
                });
            CreateAssemblyInfo(EditorParameterConstants.Interface.AssemblyInfoPath);

            // generated code folders & assemblies
            CreateFolder(EditorParameterConstants.GeneratedCode.RootDir);
            CreateFolder(EditorParameterConstants.GeneratedCode.EditorDir);
            CreateAssemblyInfo(EditorParameterConstants.GeneratedCode.AssemblyInfoPath);
            CreateAssemblyInfo(EditorParameterConstants.GeneratedCode.AssemblyInfoEditorPath);
            CreateAssembly(EditorParameterConstants.GeneratedCode.AssemblyName,
                EditorParameterConstants.GeneratedCode.RootDir, a =>
                {
                    a.references = new[]
                    {
                        interfaceAssemblyName,
                        EditorParameterConstants.AssemblyName.ParametersRuntime,
                        EditorParameterConstants.AssemblyName.Addressables
                    };
                });
            CreateAssembly(EditorParameterConstants.GeneratedCode.EditorAssemblyName,
                EditorParameterConstants.GeneratedCode.EditorDir, a =>
                {
                    a.references = new[]
                    {
                        interfaceAssemblyName,
                        EditorParameterConstants.AssemblyName.ParametersRuntime,
                        EditorParameterConstants.AssemblyName.ParametersEditor,
                        EditorParameterConstants.AssemblyName.Addressables,
                        EditorParameterConstants.GeneratedCode.AssemblyName
                    };
                    a.includePlatforms = new[] { "Editor" };
                });

            // add git ignores
            AddGitIgnore("Parameter Generated Data", ParameterConstants.GeneratedAsset.RootDirectory);
            AddGitIgnore("Parameter Local CSVs", EditorParameterConstants.CSV.Dir);

            if (_unityAssetChanges)
            {
                AssetDatabase.Refresh();
                _unityAssetChanges = false;
                return true;
            }

            return false;
        }
    }
}
