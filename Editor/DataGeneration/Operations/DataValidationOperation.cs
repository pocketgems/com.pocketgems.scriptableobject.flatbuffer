using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PocketGems.Parameters.Common.Editor;
using PocketGems.Parameters.Common.Operations.Editor;
using PocketGems.Parameters.Common.Util.Editor;
using PocketGems.Parameters.DataGeneration.Operation.Editor;
using PocketGems.Parameters.DataGeneration.Validation.Editor;
using PocketGems.Parameters.Interface;
using PocketGems.Parameters.Validation;
using UnityEngine.TestTools;

namespace PocketGems.Parameters.DataGeneration.Operations.Editor
{
    [ExcludeFromCoverage]
    internal class DataValidationOperation : BasicOperation<IDataOperationContext>
    {
        public override void Execute(IDataOperationContext context)
        {
            base.Execute(context);

            if (!ParameterPrefs.AutoValidateDataOnAssetChange)
                return;

            // skip validation now if generation will occur again (validation will occur on the next run).
            if (context.GenerateAllAgain)
                return;

            var prevKeyDelegate = ParameterLocalizationHandler.GlobalTranslateLocalizationKeyDelegate;
            var prevScriptDelegate = ParameterLocalizationHandler.GlobalTranslateLocalizableScriptDelegate;
            ParameterLocalizationHandler.GlobalTranslateLocalizationKeyDelegate = localizationKey => localizationKey;
            ParameterLocalizationHandler.GlobalTranslateLocalizableScriptDelegate = localizableScript => localizableScript;

            // validate assets
            var assetErrors = AssetValidator.ValidateScriptableObjects(context.ScriptableObjectMetadatas);
            for (int i = 0; i < assetErrors?.Count; i++)
            {
                var validationError = assetErrors[i];
                context.AllValidationErrors.Add(validationError);
                if (validationError.ErrorSeverity == ValidationError.Severity.Error)
                    Error(assetErrors[i]);
            }

            IReadOnlyList<ValidationError> parameterErrors;
            if (context.GenerateDataType == GenerateDataType.ScriptableObjectDiff ||
                context.GenerateDataType == GenerateDataType.CSVDiff)
            {
                // Only validate the changed Scriptable Object(s) rather than re-running validation
                // over the entire parameter set (which is a fixed, multi-second cost regardless of how little changed).
                parameterErrors = ValidateChangedScriptableObjects(context);
            }
            else
            {
                IParameterManager parameterManager = EditorParams.ParameterManager;
                parameterErrors = InvokeParamsValidation(context, parameterManager);
            }
            for (int i = 0; i < parameterErrors?.Count; i++)
            {
                var validationError = parameterErrors[i];
                context.AllValidationErrors.Add(validationError);
                if (validationError.ErrorSeverity == ValidationError.Severity.Error)
                    Error(parameterErrors[i]);
            }

            ParameterLocalizationHandler.GlobalTranslateLocalizationKeyDelegate = prevKeyDelegate;
            ParameterLocalizationHandler.GlobalTranslateLocalizableScriptDelegate = prevScriptDelegate;
        }

        /// <summary>
        /// Validates only the ScriptableObjects that changed in this run (populated in the context by
        /// <see cref="ScriptableObjectLoaderOperation"/>), using each object's own per-object validation
        /// (<see cref="ParameterScriptableObject.ValidationErrors"/>) — the same path the custom inspector uses.
        /// </summary>
        /// <param name="context">the current context</param>
        /// <returns>validation errors for the changed objects</returns>
        private IReadOnlyList<ValidationError> ValidateChangedScriptableObjects(IDataOperationContext context)
        {
            List<ValidationError> errors = new();
            foreach (var kvp in context.ScriptableObjectMetadatas)
            {
                var metadatas = kvp.Value;
                for (int i = 0; i < metadatas.Count; i++)
                {
                    var scriptableObject = metadatas[i].ScriptableObject;
                    if (scriptableObject == null)
                        continue;

                    ValidationError[] objectErrors;
                    try
                    {
                        objectErrors = scriptableObject.ValidationErrors();
                    }
                    catch (Exception e)
                    {
                        errors.Add(new ValidationError(scriptableObject.GetType(), scriptableObject.name, null,
                            $"{nameof(ParameterScriptableObject.ValidationErrors)} threw an exception. See console."));
                        UnityEngine.Debug.LogError(e);
                        continue;
                    }

                    if (objectErrors != null)
                        errors.AddRange(objectErrors);
                }
            }
            return errors;
        }

        /// <summary>
        /// Use reflection to invoke a static class in the auto generated class to run validation
        /// </summary>
        /// <param name="context">the current context</param>
        /// <param name="parameterManager">parameter manager with data to validate</param>
        /// <returns></returns>
        private IReadOnlyList<ValidationError> InvokeParamsValidation(IDataOperationContext context,
            IParameterManager parameterManager)
        {
            // we must use reflection here because the generated class & assembly isn't guaranteed to exist
            // therefore compilation will fail
            var assemblyName = EditorParameterConstants.CodeGeneration.AssemblyName;
            var assembly = AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault(assembly =>
                assembly.GetName().Name == assemblyName);
            if (assembly == null)
            {
                Error($"Couldn't find assembly {assemblyName}");
                return null;
            }

            string generatedNamespace = ParameterConstants.GeneratedNamespace;
            string className = EditorParameterConstants.ParamsValidationClass.ClassName;
            string methodName = EditorParameterConstants.ParamsValidationClass.MethodName;

            var typeName = $"{generatedNamespace}.{className}";
            var type = assembly.GetType(typeName);
            if (type == null)
            {
                Error($"Cannot find type {typeName}");
                return null;
            }

            var methodInfo = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static);
            if (methodInfo == null)
            {
                Error($"Cannot find method {methodName} in type {type}");
                return null;
            }

            var args = new object[] { parameterManager };
            return (IReadOnlyList<ValidationError>)methodInfo.Invoke(null, args);
        }
    }
}
