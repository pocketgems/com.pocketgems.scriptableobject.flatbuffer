using System;
using System.IO;
using PocketGems.Parameters.Common.Operation.Editor;
using PocketGems.Parameters.Common.Util.Editor;
using PocketGems.Parameters.DataGeneration.Operation.Editor;
using PocketGems.Parameters.Editor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.TestTools;

namespace PocketGems.Parameters.Processors.Editor
{
    /// <summary>
    /// Regenerate all parameters into a single flat buffer prior to starting build.
    /// </summary>
    [ExcludeFromCoverage]
    public class ParameterBuildProcessor : IPreprocessBuildWithReport
    {
        /// <summary>
        /// Called before parameter building occurs for any parameters modifiations/baking needed.
        /// </summary>
        public static event Action OnPreBuild;

        public int callbackOrder => 0;

        /// <summary>
        /// Parameters are generated during player build, except with addressable parameters: the parameter file ships in
        /// the addressable content, so it must already be generated (e.g. via CommandLineBuild.GenerateParameters)
        /// before the addressables build and is only checked for here.
        /// </summary>
        /// <param name="report"></param>
        public void OnPreprocessBuild(BuildReport report)
        {
#if ADDRESSABLE_PARAMS
            // Parameters must be generated before addressables are built, since the parameter file ships in the
            // addressable content. Generating parameters here during the player build is too late: the addressable
            // content is already built, so newly generated data would not reach the build.
            var path = ParameterConstants.GeneratedAsset.MainAssetPath;
            if (!File.Exists(path))
                throw new BuildFailedException($"Missing parameter file {path}.  Generate parameters before building the player.");
            Console.WriteLine($"{typeof(ParameterBuildProcessor)} skipping parameter generation, using existing {path}.");
#else
            if (!BuildAndValidateParameters())
                throw new BuildFailedException("Errors with parameter data validation or generation.  See logs for errors.");
#endif
        }

        internal static bool BuildAndValidateParameters()
        {
            try
            {
                if (OnPreBuild != null)
                {
                    Console.WriteLine($"{typeof(ParameterBuildProcessor)} calling prebuild event start.");
                    OnPreBuild.Invoke();
                    Console.WriteLine($"{typeof(ParameterBuildProcessor)} calling prebuild event finished.");
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
                return false;
            }

            Console.WriteLine($"{typeof(ParameterBuildProcessor)} generating parameters.");
            var success = EditorParameterDataManager.GenerateData(GenerateDataType.All, out var failedOperation);
            Console.WriteLine($"{typeof(ParameterBuildProcessor)} finished generating parameters.");

            // in batch mode, write out to console for readability in console logs
            if (Application.isBatchMode && !success)
            {
                if (failedOperation.OperationState == OperationState.Error)
                {
                    Console.Error.WriteLine();
                    Console.Error.WriteLine("********************************************************");
                    Console.Error.WriteLine("PARAMETER ERRORS");
                    Console.Error.WriteLine("********************************************************");
                    for (int i = 0; i < failedOperation.Errors.Count; i++)
                    {
                        if (i > 0) Console.Error.WriteLine();
                        var error = failedOperation.Errors[i];
                        switch (error.Type)
                        {
                            case OperationError.ErrorType.General:
                                // CI scans for this prefix
                                Console.Error.WriteLine($"Parameter Generation Error: {error.Message}");
                                break;
                            case OperationError.ErrorType.Validation:
                                // CI scans for this prefix
                                Console.Error.WriteLine($"Parameter Validation Error: {error.ValidationError}");
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                    Console.Error.WriteLine("********************************************************");
                    Console.Error.WriteLine();
                }

                if (failedOperation.OperationState == OperationState.Canceled)
                {
                    Console.Error.WriteLine();
                    Console.Error.WriteLine("********************************************************");
                    Console.Error.WriteLine("PARAMETER GENERATION CANCELED");
                    Console.Error.WriteLine("********************************************************");
                    if (failedOperation.CancelMessage != null)
                    {
                        Console.Error.WriteLine(failedOperation.CancelMessage);
                        Console.Error.WriteLine("********************************************************");
                    }
                    Console.Error.WriteLine();
                }
            }

            return success;
        }
    }
}
