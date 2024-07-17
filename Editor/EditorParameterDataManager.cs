using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using PocketGems.Parameters.CodeGeneration.Operation.Editor;
using PocketGems.Parameters.CodeGeneration.Util.Editor;
using PocketGems.Parameters.Common.Editor;
using PocketGems.Parameters.Common.Models.Editor;
using PocketGems.Parameters.Common.Operation.Editor;
using PocketGems.Parameters.Common.Operations.Editor;
using PocketGems.Parameters.Common.Util.Editor;
using PocketGems.Parameters.DataGeneration.Operation.Editor;
using PocketGems.Parameters.DataGeneration.Operations.Editor;
using PocketGems.Parameters.DataGeneration.Util.Editor;
using PocketGems.Parameters.Editor.Validation.Editor;
using PocketGems.Parameters.Interface;
using PocketGems.Parameters.Processors.Editor;
using PocketGems.Parameters.Validation;
using Unity.Profiling;
using UnityEditor;
#if ADDRESSABLE_PARAMS
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build.DataBuilders;
#endif
using UnityEditor.Compilation;
using UnityEngine;
using UnityEngine.TestTools;

namespace PocketGems.Parameters.Editor
{
    /// <summary>
    /// The main public interface for interacting with parameter generation.
    /// </summary>
    [ExcludeFromCoverage]
    public static class EditorParameterDataManager
    {
        private static bool s_disabledInterfaceImplementations;

        private static bool s_waitingGenerateCode;
        private static bool s_runningGenerateCode;
        private static bool s_waitingGenerateData;
        private static bool s_runningGenerateData;

        private static List<Action> s_editorUpdateActions;

        /// <summary>
        /// Listens for Unity editor launch or script recompilation.
        ///
        /// Upon Editor launch: setup file observing for file changes & generate
        /// Upon Script Recompilation: setup file observing for file changes & check if parameters need to be
        ///     regenerated.  Parameters need to be regenerated after regeneration & recompilation of new files.
        /// </summary>
        [InitializeOnLoadMethod]
        private static void OnLoad()
        {
            ParameterDebug.LogVerbose($"{typeof(EditorParameterDataManager)} OnLoad");

            if (Application.isBatchMode)
                return;

            if (EditorApplicationObserver.ApplicationHasFinishedInitializingSession())
            {
                Setup();
                return;
            }

            // generate all data on app launch because we do not know what data has changed
            EditorApplicationObserver.ApplicationLaunched += () => Setup(GenerateDataType.All);
        }

        private static void Setup(GenerateDataType generateDataType = GenerateDataType.IfNeeded)
        {
            ParameterDebug.LogVerbose($"{typeof(EditorParameterDataManager)} Setup");

            // if there was setup done, exit early so that the code can re-compile & re-init
            if (InitialSetup())
                return;

            SetupAssetFileWatching();

            // check to generate code if we are not using an external project to do so
            bool isUsingExternalCodeGeneration = Directory.Exists(EditorParameterConstants.CodeGeneration.ExternalProjectDir);
            bool isUnityCodeGenEnabled = ParameterPrefs.AutoGenerateCodeOnCompilation && !isUsingExternalCodeGeneration;

            if (isUnityCodeGenEnabled)
                CompilationPipeline.assemblyCompilationFinished += HandleCompilationErrors;

            // This code path will reach here if the project is set up to reload assemblies upon editor play.
            // There is no need to recheck code generation in this situation.
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            void DispatchGeneration()
            {
                if (isUnityCodeGenEnabled)
                {
                    s_waitingGenerateCode = true;
                }

                /* Defer generation from OnLoad() until the next update loop.  Attempting to generate upon
                 * this function through [InitializeOnLoadMethod] causes inconsistent data between the AssetDatabase
                 * and and reading the data again for validation through Resources.
                 */
                InvokeOnNextEditorUpdate(() =>
                {

                    bool isFocused = UnityEditorInternal.InternalEditorUtility.isApplicationActive;
                    // If the editor doesn't have focus, the developer can still be coding
                    // so do not check to generate code yet.
                    if (!isFocused)
                    {
                        ParameterDebug.LogVerbose("Delaying Generation: Editor doesn't have focus");
                        DispatchGeneration();
                        return;
                    }

                    if (isUnityCodeGenEnabled)
                    {
                        s_waitingGenerateCode = false;
                        GenerateCodeFiles(GenerateCodeType.IfNeeded, generateDataType);
                    }
                    else
                    {
                        s_waitingGenerateData = false;
                        GenerateData(generateDataType, out _);
                    }
                });
            }

            DispatchGeneration();
        }

        public static bool InitialSetup(bool force = false)
        {
            if (ParameterSetup.SetupEnvironment(force))
            {
                ParameterPrefs.ShouldGenerateAllData = true;
                ParameterDebug.Log("Setup Parameter Environment");
                return true;
            }
            return false;
        }

        internal static void InvokeEditorUpdateActions()
        {
            if (s_editorUpdateActions == null || s_editorUpdateActions.Count == 0)
                return;

            List<Action> editorUpdateActions = new List<Action>(s_editorUpdateActions);
            s_editorUpdateActions.Clear();
            EditorApplication.update -= InvokeEditorUpdateActions;

            for (int i = 0; i < editorUpdateActions.Count; i++)
                editorUpdateActions[i].Invoke();
        }

        private static void InvokeOnNextEditorUpdate(Action action)
        {
            if (s_editorUpdateActions == null)
                s_editorUpdateActions = new List<Action>();
            s_editorUpdateActions.Add(action);

            if (s_editorUpdateActions.Count == 1)
                EditorApplication.update += InvokeEditorUpdateActions;
        }

        private static void SetupAssetFileWatching()
        {
            bool CheckFile(string filePath)
            {
                var fileExt = Path.GetExtension(filePath);

                // csv
                if (fileExt == EditorParameterConstants.CSV.FileExtension)
                    // must come from specific folder
                    return NamingUtil.RelativePath(filePath).StartsWith(EditorParameterConstants.CSV.Dir);

                // not asset
                if (fileExt != ParameterConstants.ScriptableObject.FileExtension)
                    return false;

                var type = AssetDatabase.GetMainAssetTypeAtPath(filePath);

                // if the type is null, it's been deleted cannot tell it's type - try to process it.
                if (type == null)
                    return true;

                return typeof(ParameterScriptableObject).IsAssignableFrom(type);
            }

            FilePostprocessor.AddObserver(CheckFile, OnFileEvent);
        }

        private static void HandleCompilationErrors(string path, CompilerMessage[] messages)
        {
            if (messages == null || messages.Length == 0)
                return;

            var dllName = Path.GetFileNameWithoutExtension(path);
            if (dllName != EditorParameterConstants.CodeGeneration.AssemblyName)
                return;

            bool interfaceImplementationError = false;
            var soPath = NamingUtil.RelativePath(EditorParameterConstants.CodeGeneration.ScriptableObjectsDir);
            var fbPath = NamingUtil.RelativePath(EditorParameterConstants.CodeGeneration.FlatBufferClassesDir);

            for (int i = 0; i < messages.Length; i++)
            {
                var message = messages[i];
                if (message.type != CompilerMessageType.Error)
                    continue;

                if (!message.file.StartsWith(soPath) && !message.file.StartsWith(fbPath))
                    continue;

                // error due to interface deletions
                // CS0246: The type or namespace name '...' could not be found

                // error due to interface modifications
                // CS0535: ...does not implement interface member...

                // error due to interface modifications
                // CS0738: ...does not implement interface member...
                var messageString = message.message;
                if (messageString.Contains("CS0246") || messageString.Contains("CS0535") || messageString.Contains("CS0738"))
                {
                    interfaceImplementationError = true;
                    ParameterDebug.LogVerbose($"Params detected Error in file {message.file}: \n{message.message}");
                }
            }

            if (!interfaceImplementationError)
                return;

            DisableInterfaceImplementations();
        }

        // TODO MOVE TO OPERATION
        public static void DisableInterfaceImplementations()
        {
            if (s_disabledInterfaceImplementations)
            {
                ParameterDebug.LogError($"Already called {nameof(TemporaryAttemptToResolveCompilationErrors)} once.");
                return;
            }
            s_disabledInterfaceImplementations = true;

            Stopwatch stopwatch = Stopwatch.StartNew();

            // null out current generated code hash
            var interfaceHash = InterfaceHash.Create();
            interfaceHash.AssemblyInfoHash = null;
            interfaceHash.AssemblyInfoEditorHash = null;
            ParameterDebug.LogVerbose("nulling assembly hashes");

            // disable classes trying to implement the interfaces causing incomplete implementations
            void TemporaryAttemptToResolveCompilationErrors(string directoryPath, bool disableInterfaceImplementation)
            {
                if (!Directory.Exists(directoryPath))
                    return;

                var filePath = Directory.GetFiles(directoryPath, "*.cs");
                for (int i = 0; i < filePath.Length; i++)
                {
                    if (disableInterfaceImplementation)
                    {
                        if (CodeGenerator.DisableInterfaceImplementations(filePath[i]))
                            ParameterDebug.LogVerbose($"Disable interface implementation in: {filePath[i]}");
                        continue;
                    }
                    File.WriteAllText(filePath[i], "// temporary commented out - do not commit changes");
                    ParameterDebug.LogVerbose($"Commented out file: {filePath[i]}");
                }
            }

            TemporaryAttemptToResolveCompilationErrors(EditorParameterConstants.CodeGeneration.ScriptableObjectsDir, true);
            TemporaryAttemptToResolveCompilationErrors(EditorParameterConstants.CodeGeneration.FlatBufferClassesDir, true);
            TemporaryAttemptToResolveCompilationErrors(EditorParameterConstants.CodeGeneration.StructsDir, true);
            TemporaryAttemptToResolveCompilationErrors(EditorParameterConstants.CodeGeneration.CSVBridgeDir, false);
            TemporaryAttemptToResolveCompilationErrors(EditorParameterConstants.CodeGeneration.FlatBufferBuilderDir, false);

            // generate an empty data files so it doesn't reference interfaces
            var context = new CodeOperationContext();
            CodeGenerator.GenerateDataLoader("", new List<IParameterInfo>(), new List<IParameterStruct>(), context.GeneratedCodeDir);
            CodeGenerator.GenerateParamsValidation(new List<IParameterInfo>(), context.GeneratedCodeDir);

            stopwatch.Stop();

            ParameterDebug.LogVerbose($"{nameof(TemporaryAttemptToResolveCompilationErrors)} duration: {stopwatch.ElapsedMilliseconds}ms");
            ParameterDebug.Log($"Commented out code in [{EditorParameterConstants.CodeGeneration.RootDir}] to assist with interface changes.");

            // Refresh on next update loop so that editor detects code changes.
            // Attempting to refresh it in this call stack doesn't seem to kick it off right away.
            InvokeOnNextEditorUpdate(AssetDatabase.Refresh);
        }

        /// <summary>
        /// Root function to kick off generation of source files from current interface files.
        /// </summary>
        /// <param name="dispatchDataGeneration">True if data should be generated after the source files are created.</param>
        public static void GenerateCodeFiles(GenerateCodeType generateCodeType, GenerateDataType generateDataType)
        {
            s_runningGenerateCode = true;
            Stopwatch stopwatch = Stopwatch.StartNew();
            var profilerMarker = new ProfilerMarker($"Parameters.{nameof(GenerateCodeFiles)}");
            profilerMarker.Begin();
            var executor = CodeGeneration.Editor.CodeGeneration.Generate(generateCodeType);
            profilerMarker.End();
            stopwatch.Stop();
            ParameterDebug.LogVerbose($"{nameof(GenerateCodeFiles)} duration: {stopwatch.ElapsedMilliseconds}ms");

            if (executor.ExecutorState == ExecutorState.Finished && generateDataType != GenerateDataType.None)
            {
                if (executor.ShortCircuited)
                {
                    // no new coded was generated, attempt to generate data immediately
                    GenerateData(generateDataType, out _);
                }
                else
                {
                    // code changes occured.  data generation will occur on the next OnLoad call.
                    ParameterPrefs.ShouldGenerateAllData = true;
                    /*
                     * Call refresh so that any newly created/modified assets during this process are imported
                     * now while s_runningGenerateCode is true.  This will prevent trying to generate data
                     * files that do not need it.
                     */
                    AssetDatabase.Refresh();
                }
            }

            DisplayExecutionResults(executor);

            s_runningGenerateCode = false;
        }

        /// <summary>
        /// Root function to create the flatbuffer .byte file data generation.
        /// </summary>
        /// <param name="generateType">type of data generation to attempt</param>
        /// <param name="failedOperation">operation that failed during generation, null if there was no failure</param>
        /// <param name="soCreatedOrChanged">scriptable object paths for those that were created or changed</param>
        /// <param name="csvCreatedOrChanged">csv paths for those that were created or changed</param>
        /// <returns>true if the generation was successful, false otherwise. if false is returned, failedOperation will have the failed operation</returns>
        public static bool GenerateData(GenerateDataType generateType,
            out IParameterOperation<IDataOperationContext> failedOperation,
            List<string> soCreatedOrChanged = null,
            List<string> csvCreatedOrChanged = null)
        {
            s_runningGenerateData = true;
            failedOperation = null;
            Stopwatch stopwatch = Stopwatch.StartNew();
            var profilerMarker = new ProfilerMarker($"Parameters.{nameof(GenerateData)}");
            profilerMarker.Begin();
            ParameterDebug.LogVerbose("Generating Runtime Data");

            if (ParameterPrefs.ShouldGenerateAllData)
                generateType = GenerateDataType.All;

            var context = new DataOperationContext();
            context.GenerateDataType = generateType;
            context.ModifiedScriptableObjectPaths = soCreatedOrChanged;
            context.ModifiedCSVPaths = csvCreatedOrChanged;
            var executor = new OperationExecutor<IDataOperationContext>();
            executor.ExecuteOperations(new List<IParameterOperation<IDataOperationContext>>()
            {
                // clean unused directories from past implementations
                new FolderCleanupOperation(),
                // find assembly & fetch all types
                new ParseInterfaceAssemblyOperation<IDataOperationContext>(),
                // read local CSVs if needed
                new ReadLocalCSVOperation(),
                // check the hash of the previous parameter generation
                new CheckGenerateDataTypeOperation(),
                // load all needed ScriptableObjects into memory
                new ScriptableObjectLoaderOperation(),
                // update scriptable objects from CSVs
                new UpdateScriptableObjectsOperation(),
                // call methods on classes to construct the a flatbuffer byte structure
                // that holds all parameter data in Parameter.bytes
                new BuildDataBufferOperation(),
#if ADDRESSABLE_PARAMS
                // configure parameter addressable group
                new BuildAddressableGroupsOperation(),
#endif
                // save the latest hash for the generated parameter
                new SaveParamHashOperation(),
                // saves changes back to the local CSV
                new WriteLocalCSVOperation(),
                // if the game is running, notify the resource loader to hot load changed parameters
                new ParameterLoadOperation(),
                // run app specific validation over the generated flat buffer(s).
                // this needs to be done after hot loading to validate against any loaded parameter managers.
                new DataValidationOperation(),
            }, context);

            var lastOperation = executor.LastOperation;

            /*************************
             * ShouldGenerateAllData flag
             *************************/
            // reset flag if we finished successfully
            bool resetGenerateAllDataFlag = executor.ExecutorState == ExecutorState.Finished;
            if (executor.ExecutorState == ExecutorState.Error)
            {
                // reset flag if we generated data successfully but failed validation
                // - no need to regenerate the whole FlatBuffer the next time around.
                bool areAllValidationErrors = true;
                for (int i = 0; i < lastOperation.Errors?.Count; i++)
                    areAllValidationErrors &= lastOperation.Errors[i].Type == OperationError.ErrorType.Validation;
                resetGenerateAllDataFlag = areAllValidationErrors;
            }
            if (resetGenerateAllDataFlag)
                ParameterPrefs.ShouldGenerateAllData = false;

            /*************************
             * process execution state
             *************************/
            failedOperation = null;
            bool success = false;
            bool generateAllAgain = false;
            switch (executor.ExecutorState)
            {
                case ExecutorState.StateError:
                case ExecutorState.Error:
                case ExecutorState.Canceled:
                    failedOperation = lastOperation;
                    break;
                case ExecutorState.Finished:
                    success = true;
                    generateAllAgain = context.GenerateAllAgain;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var generateDataDuration = stopwatch.ElapsedMilliseconds;

            /*
             * Call refresh so that any newly created/modified assets during this process are imported
             * now while s_runningGenerateData is true.  This will prevent trying to generate data again
             * for files we changed during this process since s_runningGenerateData is still true.
             */
            stopwatch.Restart();
            AssetDatabase.Refresh();
            stopwatch.Stop();
            profilerMarker.End();
            if (stopwatch.ElapsedMilliseconds > 3000)
                ParameterDebug.LogVerbose($"AssetDatabase.Refresh duration: {stopwatch.ElapsedMilliseconds}ms");

            /*************************
             * output/display results
             *************************/
            DisplayExecutionResults(executor);

            s_runningGenerateData = false;
            ParameterDebug.LogVerbose($"{nameof(GenerateData)} duration: {generateDataDuration}ms");

            if (generateAllAgain)
                GenerateData(GenerateDataType.All, out _);

            return success;
        }

        /// <summary>
        /// Helper method to analyze all parameter files and output all localization keys.
        /// </summary>
        /// <returns>List of unique localization key strings.</returns>
        public static List<string> CollectLocalizationKeys()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            ParameterDebug.LogVerbose("Collecting Localization keys: Started");
            var localizationKeys = new List<string>();
            var executor = new OperationExecutor<IDataOperationContext>();
            var context = new DataOperationContext();
            executor.ExecuteOperations(new List<IParameterOperation<IDataOperationContext>>()
            {
                // read all scriptable object files
                new ScriptableObjectLoaderOperation(),
                // iterate and collect localization keys
                new LocalizationScraperOperation(localizationKeys),
            }, context);

            DisplayExecutionResults(executor);

            ParameterDebug.LogVerbose("Collecting Localization keys: Finished");

            stopwatch.Stop();
            ParameterDebug.LogVerbose($"{nameof(CollectLocalizationKeys)} duration: {stopwatch.ElapsedMilliseconds}ms");

            return localizationKeys;
        }

        private static void DisplayExecutionResults<T>(OperationExecutor<T> executor)
        {
            var operation = executor.LastOperation;

            if (executor.ExecutorState == ExecutorState.StateError)
            {
                EditorUtility.DisplayDialog("Parameter Operation Error",
                    $"Operation {executor.LastOperation} finished in invalid state", "Okay");
                return;
            }

            if (executor.ExecutorState == ExecutorState.Canceled)
            {
                EditorUtility.DisplayDialog("Parameter Operation Canceled", operation.CancelMessage, "Okay");
                return;
            }

            if (executor.ExecutorState == ExecutorState.Error)
            {
                List<string> errorMessages = new List<string>();
                List<ValidationError> validationErrors = new List<ValidationError>();
                for (int i = 0; i < operation.Errors?.Count; i++)
                {
                    var error = operation.Errors[i];
                    switch (error.Type)
                    {
                        case OperationError.ErrorType.General:
                            ParameterDebug.LogError(error.Message);
                            errorMessages.Add(error.Message);
                            break;
                        case OperationError.ErrorType.Validation:
                            validationErrors.Add(error.ValidationError);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                if (errorMessages.Count == 1)
                    EditorUtility.DisplayDialog("Parameter Error", errorMessages[0], "Okay");
                else if (errorMessages.Count > 1)
                    EditorUtility.DisplayDialog("Parameter Error",
                        $"There are {errorMessages.Count} parameter errors. See console for error logs.",
                        "Okay");


                if (Application.isBatchMode)
                {
                    for (int i = 0; i < validationErrors.Count; i++)
                        ParameterDebug.LogError(validationErrors[i].ToString());
                }
                else if (validationErrors.Count > 0)
                {
                    ParameterDebug.LogError("Parameter errors: see Parameter Validation window");

                    ValidationWindow.SerializeToStorage(validationErrors);
                    var window = ValidationWindow.GetWindow(true);
                    window.UpdateValidationResult(validationErrors, executor.ExecuteMilliseconds);
                }
            }

            if (executor.ExecutorState == ExecutorState.Finished)
            {
                if (typeof(T) == typeof(IDataOperationContext) && !executor.ShortCircuited &&
                    ParameterPrefs.AutoValidateDataOnAssetChange)
                {
                    ValidationWindow.SerializeToStorage(null);
                    if (ValidationWindow.HasOpenInstance())
                    {
                        var window = ValidationWindow.GetWindow(false);
                        window.UpdateValidationResult(null, executor.ExecuteMilliseconds);
                    }
                }
            }
        }

        private static void OnFileEvent(List<string> createdOrChanged, List<string> deleted, List<string> movedTo, List<string> movedFrom)
        {
            List<string> soFiles = null;
            List<string> csvFiles = null;
            GenerateDataType generateDataType = GenerateDataType.ScriptableObjectDiff;
            if (createdOrChanged?.Count > 0)
            {
                soFiles = new List<string>();
                csvFiles = new List<string>();
                for (int i = 0; i < createdOrChanged.Count; i++)
                {
                    var filePath = createdOrChanged[i];
                    ParameterDebug.LogVerbose($"Created or Changed: {filePath}");
                    if (Path.GetExtension(filePath) == EditorParameterConstants.CSV.FileExtension)
                    {
                        generateDataType = GenerateDataType.CSVDiff;
                        csvFiles.Add(filePath);
                    }
                    else
                    {
                        soFiles.Add(filePath);
                    }
                }
            }
            for (int i = 0; i < deleted?.Count; i++)
                ParameterDebug.LogVerbose($"Deleted: {deleted[i]}");
            for (int i = 0; i < movedFrom?.Count; i++)
                ParameterDebug.LogVerbose($"MovedFrom: {movedFrom[i]} MovedTo: {movedTo[i]}");

            if (UnitTestListener.AreUnitTestsRunning)
            {
                ParameterDebug.LogVerbose("Skipping generating parameter data.  Running Unit Tests.");
                return;
            }
            if (!ParameterPrefs.AutoGenerateDataOnAssetChange)
            {
                ParameterDebug.LogVerbose("Skipping generating parameter data.  Auto generation disabled.");
                return;
            }
            if (s_runningGenerateData)
            {
                ParameterDebug.LogVerbose("Skipping processing file changes, in the middle of data generation.");
                return;
            }
            if (s_waitingGenerateData)
            {
                // inform the data generation to generate all because we do not know what the union of these changes
                // and the dispatched changes are with out more work and book keeping
                ParameterPrefs.ShouldGenerateAllData = true;
                ParameterDebug.LogVerbose("Skipping processing file changes, already dispatched generation.");
                return;
            }
            if (s_waitingGenerateCode || s_runningGenerateCode)
            {
                ParameterPrefs.ShouldGenerateAllData = true;
                ParameterDebug.LogVerbose("Skipping processing file changes, in the middle of generating code.");
                return;
            }
            if (!EditorApplicationObserver.ApplicationHasFinishedInitializingSession())
            {
                ParameterDebug.LogVerbose("Skipping generating parameter data.  Application still launching.");
                ParameterPrefs.ShouldGenerateAllData = true;
                return;
            }

            // if both Scriptable Object & CSV files changed, regenerate parameters with Scriptable Objects as the source of truth.
            if (soFiles?.Count > 0 && csvFiles?.Count > 0)
                generateDataType = GenerateDataType.All;
            else if (deleted?.Count > 0 || movedFrom?.Count > 0)
                generateDataType = GenerateDataType.All;

#if ADDRESSABLE_PARAMS
            // if we're using remote bundles, always generate the whole file so that it can be uploaded with to addressables
            var editorDataBuilder = AddressableAssetSettingsDefaultObject.Settings.ActivePlayModeDataBuilder;
            bool isUsingRemoteBundles =  !(editorDataBuilder is BuildScriptFastMode || editorDataBuilder is BuildScriptVirtualMode);
            if (isUsingRemoteBundles)
            {
                generateDataType = GenerateDataType.All;
            }
#endif

            /*
             * Dispatch the GenerateData call on the next update loop.
             *
             * The call stack for this code path comes from FilePostprocessor during asset imports.
             * Calling GenerateData directly from here will prevent any subsequent callbacks to the FilePostprocessor
             * from occuring immediately since we're already inside an import call stack.  This will defer any
             * import until a later time which will cause us to thrash and handle unnecessary asset imports for params.
             */
            if (!s_waitingGenerateData)
            {
                s_waitingGenerateData = true;
                ParameterDebug.LogVerbose($"Dispatching {nameof(GenerateData)}");
                InvokeOnNextEditorUpdate(() =>
                {
                    s_waitingGenerateData = false;
                    GenerateData(generateDataType, out _, soFiles, csvFiles);
                });
            }
        }
    }
}
