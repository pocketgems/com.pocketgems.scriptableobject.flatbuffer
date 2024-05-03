using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using PocketGems.Parameters.Models;
using UnityEngine;
using UnityEngine.TestTools;

namespace PocketGems.Parameters
{
    [ExcludeFromCoverage]
    internal static class EditorParameterConstants
    {
        /// <summary>
        /// Random GUID salt created.  This is hashed with values to generate the interface hash.
        /// If there are code generation changes in our package, changing this will will force a re-generation
        /// of sources files for people who pull the latest package.
        ///
        /// Ideally it would be the most convenient to use the package version but that requires file I/O to
        /// the package.json which can be costly if we're doing it all of the time.
        /// </summary>
        public const string InterfaceHashSalt = "383a77c7-da21-469c-880d-d8aa0a64faf2";

        public static string SanitizedDataPath()
        {
            var dataPath = Application.dataPath;
            // for dataPath, the string returned on a PC will use a forward slash as a folder separator per Unity documentation.
            // we need to convert it back to effectively work with it for other operations (create/delete).
            return dataPath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        }

        /// <summary>
        /// Directory and names for generated code
        /// </summary>
        public static class GeneratedCode
        {
            // assembly names
            public const string AssemblyName = "GeneratedParameters";
            public const string EditorAssemblyName = "GeneratedParameters.Editor";

            // assembly infos
            public static string AssemblyInfoPath => Path.Combine(new[] { RootDir, AssemblyInfo.FileName });

            public static string AssemblyInfoEditorPath => Path.Combine(new[] { EditorDir, AssemblyInfo.FileName });

            // directories
            public static string RootDir =>
                Path.Combine(new[] { SanitizedDataPath(), "Parameters", "GeneratedCode" });
            public static string FlatBufferClassesDir =>
                Path.Combine(new[] { RootDir, "FlatBufferClasses" });
            public static string FlatBufferStructsDir =>
                Path.Combine(new[] { RootDir, "FlatBufferStructs" });
            public static string EditorDir =>
                Path.Combine(new[] { RootDir, "Editor" });
            public static string ScriptableObjectsDir =>
                Path.Combine(new[] { EditorDir, "ScriptableObjects" });
            public static string FlatBufferBuilderDir =>
                Path.Combine(new[] { EditorDir, "FlatBufferBuilder" });
            public static string StructsDir =>
                Path.Combine(new[] { EditorDir, "Structs" });
            public static string CSVBridgeDir =>
                Path.Combine(new[] { EditorDir, "CSVBridge" });
            public static string ValidatorsDir =>
                Path.Combine(new[] { SanitizedDataPath(), "Parameters", "Validation" });

            // flatbuffer schema
            public static string SchemaFilePath =>
                Path.Combine(new[] { SanitizedDataPath(), "Parameters", "Schema.fbs" });
        }

#if ADDRESSABLE_PARAMS
        public static class Addressables
        {
            /// <summary>
            /// addressable group name
            /// </summary>
            public static string GroupName => ParameterConstants.PackageName;

            /// <summary>
            /// Because the generated .byte file isn't source controlled, the guid can change.
            ///
            /// We hardcode the guid for the Parameter.byte file to keep the guid addressable group reference to it
            /// static since the addressable group is source controlled.
            /// </summary>
            public const string HardCodedGuid = "7fbfc27c0c6a9464282fe35122eef552";

            /// <summary>
            /// Template for the parameter meta file.
            /// </summary>
            public static string MetaTemplateFileName => "ParameterMeta.template";
        }
#endif

        /// <summary>
        ///  Generated asset
        /// </summary>
        public static class GeneratedAsset
        {
            public static string FileName =>
                $"{ParameterConstants.GeneratedAssetName}{ParameterConstants.GeneratedParameterAssetFileExtension}";

            public static string AdditiveFileName(IParameterInterface parameterInterface, UnityEngine.ScriptableObject scriptableObject)
            {
                var ext = ParameterConstants.GeneratedParameterAssetFileExtension;
                return $"{parameterInterface.Type.Name}_{scriptableObject.name}{ext}";
            }

            /// <summary>
            /// File that holds the hash of the Interface assembly at the time of generation.
            ///
            /// If this differs from the current Interface assembly hash, the previously generated data
            /// is no longer compatible.  It is hidden because it doesn't need to be sourced controlled or visible
            /// to Unity.
            /// </summary>
            public static string HashFilePath => Path.Combine(new[] { ParameterConstants.GeneratedAsset.RootDirectory, ".param_hash" });
        }

        /// <summary>
        ///  Parameter interface folder
        /// </summary>
        public static class Interface
        {
            public const string DisableImplementationSymbol = "PARAMS_DISABLE_INTERFACE_IMPLEMENTATION";

            public static string DirRoot =>
                Path.Combine(new[] { SanitizedDataPath(), "Parameters", "Interfaces" });
            public const string AssemblyName = "ParameterInterface";
            public static string AssemblyInfoPath => Path.Combine(new[] { DirRoot, AssemblyInfo.FileName });

            private static string InfoNameRegexString = @"^I([A-Z]+[A-Za-z0-9]*Info)$";
            public static readonly Regex InfoNameRegex = new Regex(InfoNameRegexString, RegexOptions.Compiled);

            private static string StructNameRegexString = @"^I([A-Z]+[A-Za-z0-9]*Struct)$";
            public static readonly Regex StructNameRegex = new Regex(StructNameRegexString, RegexOptions.Compiled);

            public static string PropertyNameRegexString = @"^[A-Z]+[A-Za-z0-9]*$";
            public static readonly Regex PropertyNameRegex = new Regex(PropertyNameRegexString, RegexOptions.Compiled);

            public static HashSet<string> InvalidReservedPropertyNames = new HashSet<string> { "short", "int", "long", "float", "ushort", "uint", "ulong", "bool", "enum" };
        }

        public static class FlatBufferBuilderClass
        {
            public const string ClassName = "FlatBufferBuilder";
            /// <summary>
            /// Template files for code generation
            /// </summary>
            public static string TemplateFileName => $"{ClassName}.template";
            public static string InfoTemplateFileName => $"{ClassName}_Info.template";
            public static string StructTemplateFileName => $"{ClassName}_Struct.template";

            /// <summary>
            /// Class name of the generated class to construct the FlatBuffer.
            /// </summary>
            public const string MethodName = "Generate";

            /// <summary>
            /// FlatBuffer guid field name for every Scriptable Object
            /// </summary>
            public const string FlatBufferGUIDFieldName = "GUID";
        }

        /// <summary>
        /// Assembly names
        /// </summary>
        public static class AssemblyName
        {
            public const string ParametersRuntime = "PocketGems.Parameters.Runtime";
            public const string ParametersEditor = "PocketGems.Parameters.Editor";
            public const string Addressables = "Unity.Addressables";
        }

        /// <summary>
        /// Scriban template file directory
        /// </summary>
        public static class Template
        {
            public static string RootDirPath =>
                Path.Combine(new[] { "Packages", ParameterConstants.PackageName, "Editor", "Templates" });
        }

        /// <summary>
        /// Data loader file generation
        /// </summary>
        public static class DataLoaderClass
        {
            public const string ClassName = "ParameterDataLoader";
            public const string TemplateFileName = "DataLoader.template";
            public const string HashFieldName = "DataHash";
        }

        public static class ParamsSetupClass
        {
            public const string ClassName = "ParamsSetup";
            public const string TemplateFileName = "ParamsSetup.template";
        }

        public static class ParamsValidationClass
        {
            public const string ClassName = "ParamsValidation";
            public const string MethodName = "Validate";
            public const string TemplateFileName = "ParamsValidation.template";
        }

        public static class ValidatorClass
        {
            public const string InfoTemplateFileName = "Validator_Info.template";
            public const string StructTemplateFileName = "Validator_Struct.template";
        }

        public static class ScriptableObjectClass
        {
            public const string TemplateFileName = "ScriptableObject.template";
        }

        public static class Struct
        {
            public const string TemplateFileName = "Struct.template";
        }

        public static class FlatBufferClass
        {
            public const string TemplateFileName = "FlatBufferClass.template";
        }

        public static class AssemblyInfo
        {
            public const string TemplateFileName = "AssemblyInfo.template";
            public const string FileName = "AssemblyInfo.cs";
        }

        public static class CSVBridgeClass
        {
            public const string ClassName = "CSVBridge";

            /// <summary>
            /// Template files for code generation
            /// </summary>
            public static string RootTemplateFileName => $"{ClassName}.template";
            public static string InfoTemplateFileName => $"{ClassName}_Info.template";
            public static string StructTemplateFileName => $"{ClassName}_Struct.template";

            /// <summary>
            /// Method names for reading from and writing to CSVs
            /// </summary>
            public static string DefineSchemaMethodName(string baseName) => $"DefineSchema{baseName}";
            public const string CheckSchemaMethodName = "CheckSchema";
            public const string UpdateMethodName = "UpdateFromScriptableObjects";
            public const string ReadMethodName = "ReadToScriptableObjects";
        }

        public static class CSV
        {
            public static string Dir => Path.Combine(new[] { "Assets", "Parameters", "LocalCSV" });
            public const string FileExtension = ".csv";
        }
    }
}
