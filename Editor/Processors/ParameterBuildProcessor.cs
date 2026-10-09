using System;
using System.IO;
using PocketGems.Parameters.Common.Editor;
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
            // A project without parameter interfaces doesn't use parameters (GenerateData skips it too), so there's no
            // parameter file to require.
            if (!Directory.Exists(EditorParameterConstants.Interface.DirRoot))
            {
                Log($"{typeof(ParameterBuildProcessor)} no parameter interfaces found, skipping parameter file check.");
                return;
            }

            // Parameters must be generated before addressables are built, since the parameter file ships in the
            // addressable content. Generating parameters here during the player build is too late: the addressable
            // content is already built, so newly generated data would not reach the build.
            var path = ParameterConstants.GeneratedAsset.MainAssetPath;
            if (!File.Exists(path))
                throw new BuildFailedException($"Missing parameter file {path}.  Generate parameters before building the player.");
            Log($"{typeof(ParameterBuildProcessor)} skipping parameter generation, using existing {path}.");
#else
            if (!BuildAndValidateParameters())
                throw new BuildFailedException("Errors with parameter data validation or generation.  See logs for errors.");
#endif
        }

        /// <summary>
        /// Invokes <see cref="OnPreBuild"/>, then generates and validates all parameter data.
        /// Errors are logged to the console in batch mode and to the Unity log otherwise.
        /// </summary>
        /// <returns>true if generation and validation succeeded, false otherwise</returns>
        public static bool BuildAndValidateParameters()
        {
            try
            {
                if (OnPreBuild != null)
                {
                    Log($"{typeof(ParameterBuildProcessor)} calling prebuild event start.", canLogToUnity: true);
                    OnPreBuild.Invoke();
                    Log($"{typeof(ParameterBuildProcessor)} calling prebuild event finished.", canLogToUnity: true);
                }
            }
            catch (Exception e)
            {
                LogError(e.Message, canLogToUnity: true);
                return false;
            }

            Log($"{typeof(ParameterBuildProcessor)} generating parameters.", canLogToUnity: true);
            var success = EditorParameterDataManager.GenerateData(GenerateDataType.All, out var failedOperation);
            Log($"{typeof(ParameterBuildProcessor)} finished generating parameters.", canLogToUnity: true);

            if (!success)
                LogFailedOperation(failedOperation);

            return success;
        }

        private static void LogFailedOperation(IParameterOperation<IDataOperationContext> failedOperation)
        {
            if (failedOperation.OperationState == OperationState.Error)
            {
                LogError("");
                LogError("********************************************************");
                LogError("PARAMETER ERRORS");
                LogError("********************************************************");
                for (int i = 0; i < failedOperation.Errors.Count; i++)
                {
                    if (i > 0) LogError("");
                    var error = failedOperation.Errors[i];
                    switch (error.Type)
                    {
                        case OperationError.ErrorType.General:
                            // CI scans for this prefix. Not sent to the Unity log, since GenerateData
                            // already logs general errors there.
                            LogError($"Parameter Generation Error: {error.Message}");
                            break;
                        case OperationError.ErrorType.Validation:
                            // CI scans for this prefix
                            LogError($"Parameter Validation Error: {error.ValidationError}", canLogToUnity: true);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                LogError("********************************************************");
                LogError("");
            }

            if (failedOperation.OperationState == OperationState.Canceled)
            {
                LogError("");
                LogError("********************************************************");
                LogError("PARAMETER GENERATION CANCELED");
                LogError("********************************************************");
                if (failedOperation.CancelMessage != null)
                {
                    LogError(failedOperation.CancelMessage, canLogToUnity: true);
                    LogError("********************************************************");
                }
                LogError("");
            }
        }

        /// <summary>
        /// Writes to the console in batch mode (for readable CI logs). Outside batch mode, writes to the Unity log only
        /// when <paramref name="canLogToUnity"/> is true.
        /// </summary>
        private static void Log(string message, bool canLogToUnity = false)
        {
            if (Application.isBatchMode)
                Console.WriteLine(message);
            else if (canLogToUnity)
                Debug.Log(message);
        }

        /// <summary>
        /// Writes to the console error stream in batch mode (for readable CI logs). Outside batch mode, writes to the
        /// Unity log only when <paramref name="canLogToUnity"/> is true, so only real errors reach the Unity console.
        /// </summary>
        private static void LogError(string message, bool canLogToUnity = false)
        {
            if (Application.isBatchMode)
                Console.Error.WriteLine(message);
            else if (canLogToUnity)
                Debug.LogError(message);
        }
    }
}
