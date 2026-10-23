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
        /// <summary>
        /// Command line flag that also writes all local CSVs from the Scriptable Objects.
        /// </summary>
        public const string GenerateCSVsArg = "-generateCSVs";

        public static void GenerateParameters()
        {
            bool generateCSVs = Array.IndexOf(Environment.GetCommandLineArgs(), GenerateCSVsArg) >= 0;
            bool success = ParameterBuildProcessor.BuildAndValidateParameters(generateCSVs);
            if (!Application.isBatchMode)
                return;
            EditorApplication.Exit(success ? 0 : 1);
        }
    }
}
