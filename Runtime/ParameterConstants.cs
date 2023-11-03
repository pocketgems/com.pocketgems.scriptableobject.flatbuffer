using System;
using System.IO;
using UnityEngine.TestTools;

namespace PocketGems.Parameters
{
    [ExcludeFromCoverage]
    public static class ParameterConstants
    {
        /// <summary>
        ///  The Package Name
        /// </summary>
        public const string PackageName = "com.pocketgems.scriptableobject.flatbuffer";
    
        /// <summary>
        /// Namespace of generated extension files.
        /// </summary>
        public const string GeneratedNamespace = "Parameters";

        /// <summary>
        /// The root flatbuffer struct type in the generated parameter assembly that contains references to all parameters.
        /// </summary>
        public const string CollectionTypeName = "RootCollection";

        /// <summary>
        /// Asset name of the root parameter file.
        /// </summary>
        public const string GeneratedAssetName = "Parameter";

        /// <summary>
        /// The Asset folder where the GeneratedResourceName is located.
        /// </summary>
        public const string GeneratedResourceDirectory = "GeneratedParameterData";

#if ADDRESSABLE_PARAMS
        public static class Addressables
        {
            /// <summary>
            /// Parameter file address.
            /// </summary>
            public static string GeneratedAssetAddress => $"{PackageName}_parameter";
        }
#endif

#if UNITY_EDITOR
        /// <summary>
        /// Location where all scriptable objects must live under
        /// </summary>
        public static class ScriptableObject
        {
            public const string FileExtension = ".asset";
            public static string[] Folders => new[] { "Assets", "Parameters", "ScriptableObjects" };
            public static string Dir => Path.Combine(Folders);
        }

        /// <summary>
        /// File type used to store the FlatBuffer byte file.  Used at both runtime & in the editor assembly.
        /// </summary>
        internal const string GeneratedParameterAssetFileExtension = ".bytes";

        internal static class GeneratedAsset
        {
            /// <summary>
            /// Root directory holding all of the parameter files.
            /// </summary>
            public static string RootDirectory => Path.Combine(new[] { "Assets", "Parameters", "GeneratedAssets" });

            /// <summary>
            /// Subdirectory holding all of the generated parameter resources.
            /// </summary>
            public static string SubDirectory
            {
                get
                {
    #if ADDRESSABLE_PARAMS
                    const string folderName = "Assets";
    #else
                    const string folderName = "Resources";
    #endif
                    var intermediateFolder = Path.Combine(RootDirectory, folderName);
                    return Path.Combine(intermediateFolder, GeneratedResourceDirectory);
                }
            }

            internal static string MainAssetPath
            {
                get
                {
                    var fileName = GeneratedAssetName + GeneratedParameterAssetFileExtension;
                    return Path.Combine(SubDirectory, fileName);
                }
            }
        }
#endif
    }
}
