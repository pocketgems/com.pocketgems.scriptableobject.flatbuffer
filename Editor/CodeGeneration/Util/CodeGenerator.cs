using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using PocketGems.Parameters.AssetLoader;
using PocketGems.Parameters.Common.Editor;
using PocketGems.Parameters.Common.Models.Editor;
using PocketGems.Parameters.Common.Util.Editor;
using PocketGems.Parameters.Interface;
using UnityEngine.TestTools;

namespace PocketGems.Parameters.CodeGeneration.Util.Editor
{
    internal static class CodeGenerator
    {
        /// <summary>
        /// Generate and write the default AssemblyInfo a template.
        /// </summary>
        /// <param name="filePath">The file path to write the AssemblyInfo code to.</param>
        /// <returns>filepath of the file written</returns>
        public static string GenerateAssemblyInfo(string filePath)
        {
            var templateFileName = EditorParameterConstants.AssemblyInfo.TemplateFileName;
            ScribanHelper.GenerateClass(templateFileName, filePath, new Dictionary<string, object>());
            return filePath;
        }

        /// <summary>
        /// Generate and write code for games to call to load the Parameter Manager.
        /// </summary>
        /// <param name="outputDirectory">Directory to write the file too.</param>
        /// <returns>filepath of the file written</returns>
        public static string GenerateParamsSetup(string outputDirectory)
        {
            var className = EditorParameterConstants.ParamsSetupClass.ClassName;
            var args = new Dictionary<string, object>
            {
                { "className", className },
            };
            var filePath = Path.Combine(outputDirectory, $"{className}.cs");
            var templateFileName = EditorParameterConstants.ParamsSetupClass.TemplateFileName;
            ScribanHelper.GenerateClass(templateFileName, filePath, args);
            return filePath;
        }

        /// <summary>
        /// Generate and write code for games to call to load the Parameter Manager.
        /// </summary>
        /// <param name="parameterInfos">Info interfaces to validate from the generated params code.</param>
        /// <param name="outputDirectory">Directory to write the file too.</param>
        /// <returns>filepath of the file written</returns>
        public static string GenerateParamsValidation(List<IParameterInfo> parameterInfos, string outputDirectory)
        {
            var interfaces = new List<Dictionary<string, object>>();
            void AddParameterInterface(IParameterInterface parameterInterface)
            {
                var iDict = new Dictionary<string, object>();
                iDict["InterfaceName"] = parameterInterface.InterfaceName;
                interfaces.Add(iDict);
            }

            for (int i = 0; i < parameterInfos.Count; i++)
                AddParameterInterface(parameterInfos[i]);

            var className = EditorParameterConstants.ParamsValidationClass.ClassName;
            var args = new Dictionary<string, object>
            {
                { "isAddressable", IsAddressable() },
                { "className", className },
                { "interfaces", interfaces },
            };

            var filePath = Path.Combine(outputDirectory, $"{className}.cs");
            var templateFileName = EditorParameterConstants.ParamsValidationClass.TemplateFileName;
            ScribanHelper.GenerateClass(templateFileName, filePath, args);
            return filePath;
        }

        public static bool AttemptGenerateValidationFile(IParameterInfo parameterInfo, string outputDirectory)
        {
            var templateFileName = EditorParameterConstants.ValidatorClass.InfoTemplateFileName;
            return AttemptGenerateValidationFile(parameterInfo, templateFileName, outputDirectory);
        }

        public static bool AttemptGenerateValidationFile(IParameterStruct parameterStruct, string outputDirectory)
        {
            var templateFileName = EditorParameterConstants.ValidatorClass.StructTemplateFileName;
            return AttemptGenerateValidationFile(parameterStruct, templateFileName, outputDirectory);
        }

        /// <summary>
        /// Generate the Validation file for an interface if it doesn't exist.
        /// </summary>
        /// <param name="parameterInterface">Interface to write the validation file for.</param>
        /// <param name="templateFileName">Scriban template file to use</param>
        /// <param name="outputDirectory">Directory to write the validation file to.</param>
        /// <returns>true if a new file needed to be generated</returns>
        private static bool AttemptGenerateValidationFile(IParameterInterface parameterInterface,
            string templateFileName, string outputDirectory)
        {
            if (!Directory.Exists(outputDirectory))
                Directory.CreateDirectory(outputDirectory);

            var fileName = parameterInterface.ValidatorClassName(true);
            var filePath = Path.Combine(outputDirectory, fileName);
            if (File.Exists(filePath))
                return false;

            var args = new Dictionary<string, object>
            {
                { "isAddressable", IsAddressable() },
                { "className", parameterInterface.ValidatorClassName(false) },
                { "interfaceName", parameterInterface.InterfaceName }
            };

            ScribanHelper.GenerateClass(templateFileName, filePath, args);
            return true;
        }

        /// <summary>
        /// Generate the Scriptable Object class file that implements the interface.
        /// </summary>
        /// <param name="parameterInfo">Interface to write the class for.</param>
        /// <param name="order">The menu order.</param>
        /// <param name="outputDirectory">Directory to write the class to.</param>
        /// <returns>filepath of the file written</returns>
        public static string GenerateScriptableObjectFile(IParameterInfo parameterInfo, int order, string outputDirectory)
        {
            if (!Directory.Exists(outputDirectory))
                Directory.CreateDirectory(outputDirectory);

            var properties = new List<string>();
            var propertyTypes = parameterInfo.PropertyTypes;
            for (int i = 0; i < propertyTypes.Count; i++)
            {
                var attributes = propertyTypes[i].ScriptableObjectFieldAttributesCode();
                for (int j = 0; j < attributes?.Count; j++)
                    properties.Add(attributes[j]);
                var fieldSource = propertyTypes[i].ScriptableObjectFieldDefinitionCode();
                if (fieldSource != null)
                    properties.Add(fieldSource);
                properties.Add(propertyTypes[i].ScriptableObjectPropertyImplementationCode());
            }

            var args = new Dictionary<string, object>
            {
                { "isAddressable", IsAddressable() },
                { "disableSymbol", EditorParameterConstants.Interface.DisableImplementationSymbol },
                { "namespace", parameterInfo.GeneratedNameSpace },
                { "baseName", parameterInfo.BaseName },
                { "className", parameterInfo.ScriptableObjectClassName(false) },
                { "interfaceName", parameterInfo.InterfaceName },
                { "properties", properties },
                { "order", order },
            };
            var fileName = parameterInfo.ScriptableObjectClassName(true);
            var filePath = Path.Combine(outputDirectory, fileName);
            var templateFileName = EditorParameterConstants.ScriptableObjectClass.TemplateFileName;
            ScribanHelper.GenerateClass(templateFileName, filePath, args);
            return fileName;
        }

        /// <summary>
        /// Generate the Struct file that implements the interface.
        /// </summary>
        /// <param name="parameterStruct">Interface to write the struct for.</param>
        /// <param name="outputDirectory">Directory to write the struct to.</param>
        /// <returns>filepath of the file written</returns>
        public static string GenerateStructFile(IParameterStruct parameterStruct, string outputDirectory)
        {
            if (!Directory.Exists(outputDirectory))
                Directory.CreateDirectory(outputDirectory);

            var properties = new List<string>();
            var propertyTypes = parameterStruct.PropertyTypes;
            for (int i = 0; i < propertyTypes.Count; i++)
            {
                var attributes = propertyTypes[i].ScriptableObjectFieldAttributesCode();
                for (int j = 0; j < attributes?.Count; j++)
                    properties.Add(attributes[j]);
                var fieldSource = propertyTypes[i].ScriptableObjectFieldDefinitionCode();
                if (fieldSource != null)
                    properties.Add(fieldSource);
                properties.Add(propertyTypes[i].ScriptableObjectPropertyImplementationCode());
            }

            var args = new Dictionary<string, object>
            {
                { "isAddressable", IsAddressable() },
                { "disableSymbol", EditorParameterConstants.Interface.DisableImplementationSymbol },
                { "namespace", parameterStruct.GeneratedNameSpace },
                { "baseName", parameterStruct.BaseName },
                { "structName", parameterStruct.StructName(false) },
                { "interfaceName", parameterStruct.InterfaceName },
                { "properties", properties },
            };
            var fileName = parameterStruct.StructName(true);
            var filePath = Path.Combine(outputDirectory, fileName);
            var templateFileName = EditorParameterConstants.Struct.TemplateFileName;
            ScribanHelper.GenerateClass(templateFileName, filePath, args);
            return fileName;
        }

        /// <summary>
        /// Generate the FlatBuffer class file that implements the interface.
        /// </summary>
        /// <param name="parameterInterface">Interface to write the class for.</param>
        /// <param name="outputDirectory">Directory to write the class to.</param>
        /// <returns>filepath of the file written</returns>
        public static string GenerateFlatBufferClassFile(IParameterInterface parameterInterface, bool isInfo, string outputDirectory)
        {
            if (!Directory.Exists(outputDirectory))
                Directory.CreateDirectory(outputDirectory);

            var propertyTypeDicts = new List<Dictionary<string, string>>();
            var propertyTypes = parameterInterface.PropertyTypes;
            for (int i = 0; i < propertyTypes.Count; i++)
            {
                var propertyType = propertyTypes[i];
                var propertyTypeDict = new Dictionary<string, string>();
                propertyTypeDicts.Add(propertyTypeDict);

                propertyTypeDict["propertyName"] = propertyType.PropertyInfo.Name;

                // field definitions
                var fieldDefinition = propertyType.FlatBufferFieldDefinitionCode();
                if (!string.IsNullOrWhiteSpace(fieldDefinition))
                    propertyTypeDict["fieldDefinitionCode"] = fieldDefinition;

                // property implementation
                propertyTypeDict["propertyImplementationCode"] = propertyType.FlatBufferPropertyImplementationCode();

                // edit property
                propertyTypeDict["editPropertyCode"] = propertyType.FlatBufferEditPropertyCode("value");

                // remove edit property
                var removeEdit = propertyType.FlatBufferRemoveEditCode();
                if (!string.IsNullOrWhiteSpace(removeEdit))
                    propertyTypeDict["removeEditCode"] = removeEdit;
            }

            var args = new Dictionary<string, object>
            {
                { "isAddressable", IsAddressable() },
                { "isInfo", isInfo },
                { "disableSymbol", EditorParameterConstants.Interface.DisableImplementationSymbol },
                { "namespace", parameterInterface.GeneratedNameSpace },
                { "className", parameterInterface.FlatBufferClassName(false) },
                { "interfaceName", parameterInterface.InterfaceName },
                { "flatBufferStructName", parameterInterface.FlatBufferStructName(false) },
                { "propertyTypeDicts", propertyTypeDicts },
                { "parameterManagerType", nameof(IParameterManager) },
            };
            var fileName = parameterInterface.FlatBufferClassName(true);
            var filePath = Path.Combine(outputDirectory, fileName);
            var templateFileName = EditorParameterConstants.FlatBufferClass.TemplateFileName;
            ScribanHelper.GenerateClass(templateFileName, filePath, args);
            return fileName;
        }

        /// <summary>
        /// Uses the flatc binary to generate all of the structs for the data types.
        /// </summary>
        /// <param name="schemaFilePath">FlatBuffer schema file</param>
        /// <param name="outputDirectory">source code output directory</param>
        /// <param name="errors">errors with generation</param>
        /// <returns>true if successful</returns>
        [ExcludeFromCoverage] // excluded since unit test will need permissions approved in order for the binary to be executed
        public static bool GenerateFlatBufferStructs(string schemaFilePath, string outputDirectory, out List<string> errors)
        {
            errors = new List<string>();
            if (!File.Exists(schemaFilePath))
            {
                errors.Add($"Schema file doesn't exist {schemaFilePath}");
                return false;
            }

            /*
             * We do not know which files the flatc will create so we delete all ".cs" files first.
             * Allow the creation of the .cs files, then remove any unused meta files to preserve the guids.
             */
            if (Directory.Exists(outputDirectory))
                UnusedFileRemover.RemoveFiles(outputDirectory, "cs");
            else
                Directory.CreateDirectory(outputDirectory);

            // default packages path
            string AttemptToCreateDirPath(string directoryPath, string packageNamePattern)
            {
                var directories = Directory.GetDirectories(directoryPath, packageNamePattern, SearchOption.TopDirectoryOnly);
                if (directories.Length == 1)
                {
                    string[] executeDirectoryStrings =
                    {
                        Directory.GetCurrentDirectory(), directories[0],
                        "Editor", "CodeGeneration", "ThirdParty", "FlatBuffer"
                    };
                    return Path.Combine(executeDirectoryStrings);
                }
                return null;
            }

            bool isOnMac = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
            // always search the PackageCache folder first since queries to the Packages folder can find packages
            // that are physically in the in the PackageCache folder
            var executeDirectory =
                AttemptToCreateDirPath(Path.Combine(EditorParameterConstants.SanitizedDataPath(), "..", "Library", "PackageCache"), $"{ParameterConstants.PackageName}@*") ??
                AttemptToCreateDirPath(Path.Combine(EditorParameterConstants.SanitizedDataPath(), "..", "Packages"), ParameterConstants.PackageName);

            var executePath = Path.Combine(executeDirectory, isOnMac ? "flatc" : "flatc.exe");
            Process process = new Process();
            process.StartInfo.FileName = executePath;
            process.StartInfo.WorkingDirectory = outputDirectory;
            process.StartInfo.Arguments = $"--csharp \"{Path.Combine(Directory.GetCurrentDirectory(), schemaFilePath)}\""; // quotes around path in case of spaces
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.UseShellExecute = false;
            process.Start();

            // wait for process to finish with a timeout just in case
            process.WaitForExit(10000);
            string output = process.StandardOutput.ReadToEnd();
            var exitCode = process.ExitCode;
            process.Close();

            if (exitCode != 0)
            {
                errors.Add($"Issue executing binary {executePath}");
                if (!string.IsNullOrEmpty(output))
                    errors.Add(output);
                if (isOnMac)
                    errors.Add("Check your OS's System Preferences --> Security & Privacy --> Allow \"flatc\"");
                return false;
            }

            // convert files to mac newlines so that the files do now show a git diff between mac & windows users
            string[] files = Directory.GetFiles(outputDirectory, "*.cs");
            for (int i = 0; i < files.Length; i++)
                ConvertFileToMacNewlines(files[i]);

            UnusedFileRemover.RemoveUnusedMetaFiles(outputDirectory);
            return true;
        }

        /// <summary>
        /// Generate and write a class to take Scriptable Objects and build the FlatBuffer.
        /// </summary>
        /// <param name="parameterInfos">parameter infos to generate code with.</param>
        /// <param name="parameterStructs">parameter structs to generate code with.</param>
        /// <param name="outputDirectory">directory where the class will be written to.</param>
        public static void GenerateFlatBufferBuilder(List<IParameterInfo> parameterInfos,
            List<IParameterStruct> parameterStructs, string outputDirectory)
        {
            var fileRemover = new UnusedFileRemover(outputDirectory);
            if (!Directory.Exists(outputDirectory))
                Directory.CreateDirectory(outputDirectory);

            var className = EditorParameterConstants.FlatBufferBuilderClass.ClassName;

            Dictionary<string, object> CreateInterfacePartialClass(IParameterInterface parameterInterface,
                string templateFilename, string dataObjectName)
            {
                var flatBufferBuilderPrepSources = new List<string>();
                var flatBufferBuilderSources = new List<string>();
                var propertyTypes = parameterInterface.PropertyTypes;
                for (int j = 0; j < propertyTypes.Count; j++)
                {
                    var propertyType = propertyTypes[j];
                    var builderSource = propertyType.FlatBufferBuilderCode(parameterInterface.FlatBufferStructName(false));
                    if (builderSource != null)
                    {
                        flatBufferBuilderSources.Add($"// {propertyType.PropertyInfo.Name}");
                        flatBufferBuilderSources.Add(builderSource);
                    }

                    var builderPrepSource = propertyType.FlatBufferBuilderPrepareCode(parameterInterface.FlatBufferStructName(false));
                    if (builderPrepSource != null)
                    {
                        flatBufferBuilderPrepSources.Add($"// {propertyType.PropertyInfo.Name}");
                        flatBufferBuilderPrepSources.Add(builderPrepSource);
                    }
                }

                var iDict = new Dictionary<string, object>();
                iDict["BaseName"] = parameterInterface.BaseName;
                iDict["InterfaceName"] = parameterInterface.InterfaceName;
                iDict["DataObjectName"] = dataObjectName;
                iDict["FlatBufferStructName"] = parameterInterface.FlatBufferStructName(false);
                iDict["FlatBufferBuilderSources"] = flatBufferBuilderSources;
                iDict["FlatBufferBuilderPrepSources"] = flatBufferBuilderPrepSources;

                var interfaceArgs = new Dictionary<string, object>
                {
                    { "isAddressable", IsAddressable() },
                    { "disableSymbol", EditorParameterConstants.Interface.DisableImplementationSymbol },
                    { "namespace", ParameterConstants.GeneratedNamespace },
                    { "collectionStructName", ParameterConstants.CollectionTypeName },
                    { "className", className },
                    { "interface", iDict },
                };
                var interfaceFilename = $"{className}_{parameterInterface.BaseName}.cs";
                var interfaceFilePath = Path.Combine(outputDirectory, interfaceFilename);
                ScribanHelper.GenerateClass(templateFilename, interfaceFilePath, interfaceArgs);
                fileRemover.UsedFile(interfaceFilename);
                return iDict;
            }

            var infoInterfaces = new List<Dictionary<string, object>>();
            var infoTemplateFileName = EditorParameterConstants.FlatBufferBuilderClass.InfoTemplateFileName;
            for (int i = 0; i < parameterInfos.Count; i++)
            {
                var parameterInterface = parameterInfos[i];
                var dataObjectName = parameterInterface.ScriptableObjectClassName(false);
                infoInterfaces.Add(CreateInterfacePartialClass(parameterInterface, infoTemplateFileName, dataObjectName));
            }

            var structInterfaces = new List<Dictionary<string, object>>();
            var structTemplateFileName = EditorParameterConstants.FlatBufferBuilderClass.StructTemplateFileName;
            for (int i = 0; i < parameterStructs.Count; i++)
            {
                var parameterInterface = parameterStructs[i];
                var dataObjectName = parameterInterface.StructName(false);
                structInterfaces.Add(CreateInterfacePartialClass(parameterInterface, structTemplateFileName, dataObjectName));
            }

            var args = new Dictionary<string, object>
            {
                { "isAddressable", IsAddressable() },
                { "disableSymbol", EditorParameterConstants.Interface.DisableImplementationSymbol },
                { "namespace", ParameterConstants.GeneratedNamespace },
                { "collectionStructName", ParameterConstants.CollectionTypeName },
                { "className", className },
                { "infoInterfaces", infoInterfaces },
                { "structInterfaces", structInterfaces },
            };

            var fileName = $"{className}.cs";
            var filePath = Path.Combine(outputDirectory, fileName);
            var templateFileName = EditorParameterConstants.FlatBufferBuilderClass.TemplateFileName;
            ScribanHelper.GenerateClass(templateFileName, filePath, args);
            fileRemover.UsedFile(fileName);
            fileRemover.RemoveUnusedFiles();
        }

        /// <summary>
        /// Generate and write a class to update CSVs from Scriptable Objects and vice-versa
        /// </summary>
        /// <param name="parameterInfos">parameter infos to generate code with.</param>
        /// <param name="parameterStructs">parameter structs to generate code with.</param>
        /// <param name="outputDirectory">directory where the class will be written to.</param>
        public static void GenerateCSVBridge(List<IParameterInfo> parameterInfos,
            List<IParameterStruct> parameterStructs, string outputDirectory)
        {
            var fileRemover = new UnusedFileRemover(outputDirectory);
            if (!Directory.Exists(outputDirectory))
                Directory.CreateDirectory(outputDirectory);

            var className = EditorParameterConstants.CSVBridgeClass.ClassName;

            Dictionary<string, object> CreateInterfacePartialClass(IParameterInterface parameterInterface,
                string templateFilename, string dataObjectName)
            {
                var csvColumnNames = new List<string>();
                var csvColumnTypes = new List<string>();
                var propertyTypes = parameterInterface.PropertyTypes;
                var propertyTypeDicts = new List<Dictionary<string, object>>();
                for (int j = 0; j < propertyTypes.Count; j++)
                {
                    var propertyType = propertyTypes[j];
                    csvColumnNames.Add(propertyType.CSVBridgeColumnNameText);
                    csvColumnTypes.Add(propertyType.CSVBridgeColumnTypeText);

                    var tDict = new Dictionary<string, object>();
                    tDict["PropertyName"] = propertyType.PropertyInfo.Name;
                    tDict["CSVBridgeReadFromCSVSource"] = propertyType.CSVBridgeReadFromCSVCode($"rowData[{j}]");
                    tDict["CSVBridgeUpdateCSVRowSource"] = propertyType.CSVBridgeUpdateCSVRowCode($"rowData[{j}]");
                    propertyTypeDicts.Add(tDict);
                }
                var iDict = new Dictionary<string, object>();
                iDict["BaseName"] = parameterInterface.BaseName;
                iDict["InterfaceName"] = parameterInterface.InterfaceName;
                iDict["DataObjectName"] = dataObjectName;
                iDict["CSVColumnNames"] = csvColumnNames;
                iDict["CSVColumnTypes"] = csvColumnTypes;
                iDict["PropertyTypeDicts"] = propertyTypeDicts;

                var interfaceArgs = new Dictionary<string, object>
                {
                    { "isAddressable", IsAddressable() },
                    { "disableSymbol", EditorParameterConstants.Interface.DisableImplementationSymbol },
                    { "namespace", ParameterConstants.GeneratedNamespace },
                    { "className", className },
                    { "interface", iDict }
                };
                var interfaceFileName = $"{className}_{parameterInterface.BaseName}.cs";
                var interfaceFilePath = Path.Combine(outputDirectory, interfaceFileName);
                ScribanHelper.GenerateClass(templateFilename, interfaceFilePath, interfaceArgs);
                fileRemover.UsedFile(interfaceFileName);
                return iDict;
            }

            var infoInterfaces = new List<Dictionary<string, object>>();
            var infoTemplateFileName = EditorParameterConstants.CSVBridgeClass.InfoTemplateFileName;
            for (int i = 0; i < parameterInfos.Count; i++)
            {
                var parameterInterface = parameterInfos[i];
                var dataObjectName = parameterInterface.ScriptableObjectClassName(false);
                infoInterfaces.Add(CreateInterfacePartialClass(parameterInterface, infoTemplateFileName, dataObjectName));
            }

            var structInterfaces = new List<Dictionary<string, object>>();
            var structTemplateFileName = EditorParameterConstants.CSVBridgeClass.StructTemplateFileName;
            for (int i = 0; i < parameterStructs.Count; i++)
            {
                var parameterInterface = parameterStructs[i];
                var dataObjectName = parameterInterface.StructName(false);
                structInterfaces.Add(CreateInterfacePartialClass(parameterInterface, structTemplateFileName, dataObjectName));
            }

            var args = new Dictionary<string, object>
            {
                { "isAddressable", IsAddressable() },
                { "disableSymbol", EditorParameterConstants.Interface.DisableImplementationSymbol },
                { "namespace", ParameterConstants.GeneratedNamespace },
                { "className", className },
                { "checkSchemaMethodName", EditorParameterConstants.CSVBridgeClass.CheckSchemaMethodName },
                { "readMethodName", EditorParameterConstants.CSVBridgeClass.ReadMethodName },
                { "updateMethodName", EditorParameterConstants.CSVBridgeClass.UpdateMethodName },
                { "infoInterfaces", infoInterfaces },
                { "structInterfaces", structInterfaces },
            };

            var fileName = $"{className}.cs";
            var filePath = Path.Combine(outputDirectory, fileName);
            var templateFileName = EditorParameterConstants.CSVBridgeClass.RootTemplateFileName;
            ScribanHelper.GenerateClass(templateFileName, filePath, args);
            fileRemover.UsedFile(fileName);
            fileRemover.RemoveUnusedFiles();
        }

        /// <summary>
        /// Generate the root file that is used at runtime to load all parameter data.
        /// </summary>
        /// <param name="parameterInfos">parameter infos to generate code with.</param>
        /// <param name="parameterStructs">parameter structs to generate code with.</param>
        /// <param name="outputDirectory">directory where the class will be written to.</param>
        public static void GenerateDataLoader(string hash, List<IParameterInfo> parameterInfos,
            List<IParameterStruct> parameterStructs, string outputDirectory)
        {
            if (!Directory.Exists(outputDirectory))
                Directory.CreateDirectory(outputDirectory);

            var dataLoaderClassName = EditorParameterConstants.DataLoaderClass.ClassName;
            // order by name so that generated code is in a deterministic order
            var orderedInfos = parameterInfos.OrderBy(t => t.BaseName);
            var infoInterfaces = new List<Dictionary<string, object>>();
            foreach (var parameterInterface in orderedInfos)
            {
                var dict = new Dictionary<string, object>();
                dict["InterfaceName"] = parameterInterface.InterfaceName;
                dict["FlatBufferStructName"] = parameterInterface.FlatBufferStructName(false);
                dict["FlatBufferClassName"] = parameterInterface.FlatBufferClassName(false);

                var baseInterfaceTypes = parameterInterface.OrderedBaseInterfaceTypes;
                var baseInterfaceNames = new List<string>();
                for (int j = 0; j < baseInterfaceTypes.Count; j++)
                {
                    var baseType = baseInterfaceTypes[j];
                    if (baseType != typeof(IBaseInfo))
                        baseInterfaceNames.Add(baseType.Name);
                }
                dict["BaseInterfaceNames"] = baseInterfaceNames;

                infoInterfaces.Add(dict);
            }

            // order by name so that generated code is in a deterministic order
            var orderedStructs = parameterStructs.OrderBy(t => t.BaseName);
            var structInterfaces = new List<Dictionary<string, object>>();
            foreach (var parameterInterface in orderedStructs)
            {
                var dict = new Dictionary<string, object>();
                dict["InterfaceName"] = parameterInterface.InterfaceName;
                dict["FlatBufferStructName"] = parameterInterface.FlatBufferStructName(false);
                dict["FlatBufferClassName"] = parameterInterface.FlatBufferClassName(false);
                structInterfaces.Add(dict);
            }

            var args = new Dictionary<string, object>
            {
                { "isAddressable", IsAddressable() },
                { "namespace", ParameterConstants.GeneratedNamespace },
                { "dataLoaderClassName", dataLoaderClassName },
                { "interfaceType", typeof(IParameterDataLoader).ToString() },
                { "parameterManagerType", typeof(IMutableParameterManager).ToString() },
                { "hashFieldName", EditorParameterConstants.DataLoaderClass.HashFieldName },
                { "hash", hash },
                { "collectionStructName", ParameterConstants.CollectionTypeName },
                { "infoInterfaces", infoInterfaces },
                { "structInterfaces", structInterfaces }
            };

            var filePath = Path.Combine(outputDirectory, $"{dataLoaderClassName}.cs");
            var templateFileName = EditorParameterConstants.DataLoaderClass.TemplateFileName;
            ScribanHelper.GenerateClass(templateFileName, filePath, args);
        }

        /// <summary>
        /// Reads a file and defines a macro to disable interface implementations.
        /// </summary>
        /// <param name="filePath">source code file path</param>
        public static bool DisableInterfaceImplementations(string filePath)
        {
            var defineString = $"#define {EditorParameterConstants.Interface.DisableImplementationSymbol}";
            var lines = File.ReadAllLines(filePath);
            if (lines[0] == defineString)
                return false;

            string[] newLines = new string[lines.Length + 1];
            newLines[0] = defineString;
            for (int i = 0; i < lines.Length; i++)
            {
                newLines[i + 1] = lines[i];
            }

            File.WriteAllLines(filePath, newLines);
            return true;
        }

        public static void ConvertFileToMacNewlines(string filePath)
        {
            var contents = File.ReadAllText(filePath);
            contents = Regex.Replace(contents, "\r\n|\r", "\n");
            File.WriteAllText(filePath, contents);
        }

        private static bool IsAddressable()
        {
#if ADDRESSABLE_PARAMS
            return true;
#else
            return false;
#endif
        }
    }
}
