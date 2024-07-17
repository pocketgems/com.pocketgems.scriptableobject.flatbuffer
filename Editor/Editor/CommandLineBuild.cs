using System;
using PocketGems.Parameters.Processors.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace PocketGems.Parameters.Editor.Editor
{
    /// <summary>
    /// Static function to be called from batch mode to construct parameters prior to addressables building.
    /// </summary>
    [ExcludeFromCoverage]
    internal static class CommandLineBuild
    {
        public static void GenerateParameters()
        {
            bool success = ParameterBuildProcessor.BuildAndValidateParameters();
            if (!Application.isBatchMode)
                return;
            EditorApplication.Exit(success ? 0 : 1);
        }
    }
}

// leave the old function & namespace for backwards compatibility reasons
namespace PocketGems.Parameters.Editor
{
    [Obsolete]
    [ExcludeFromCoverage]
    internal static class CommandLineBuild
    {
        public static void GenerateParameters() => Editor.CommandLineBuild.GenerateParameters();
    }
}
