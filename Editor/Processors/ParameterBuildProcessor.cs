using System;
using PocketGems.Parameters.Editor.Operation;
using PocketGems.Parameters.Util;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.TestTools;

namespace PocketGems.Parameters.Processors
{
    /// <summary>
    /// Regenerate all parameters into a single flat buffer prior to starting build.
    /// </summary>
    [ExcludeFromCoverage]
    internal class ParameterBuildProcessor : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        /// <summary>
        /// Parameters are generated during player build.
        /// </summary>
        /// <param name="report"></param>
        public void OnPreprocessBuild(BuildReport report)
        {
            if (!BuildAndValidateParameters())
                throw new BuildFailedException("Errors with parameter data validation or generation.  See logs for errors.");
        }

        internal static bool BuildAndValidateParameters()
        {
            ParameterDebug.Log($"{typeof(ParameterBuildProcessor)} generating parameters.");
            var success = EditorParameterDataManager.GenerateData(GenerateDataType.All, out var failedOperation);
            ParameterDebug.Log($"{typeof(ParameterBuildProcessor)} finished generating parameters.");

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
                                // ClientPlatformCI scans for this prefix
                                Console.Error.WriteLine($"Parameter Generation Error: {error.Message}");
                                break;
                            case OperationError.ErrorType.Validation:
                                // ClientPlatformCI scans for this prefix
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
