using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PocketGems.Parameters.Common.Editor;
using PocketGems.Parameters.Common.Operations.Editor;
using PocketGems.Parameters.Common.Util.Editor;
using PocketGems.Parameters.DataGeneration.Operation.Editor;
using PocketGems.Parameters.DataGeneration.Validation.Editor;
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

            // validate assets
            var assetErrors = AssetValidator.ValidateScriptableObjects(context.ScriptableObjectMetadatas);
            for (int i = 0; i < assetErrors?.Count; i++)
            {
                var validationError = assetErrors[i];
                context.AllValidationErrors.Add(validationError);
                if (validationError.ErrorSeverity == ValidationError.Severity.Error)
                    Error(assetErrors[i]);
            }

            // validate parameters
            IParameterManager parameterManager = EditorParams.ParameterManager;
            var parameterErrors = InvokeParamsValidation(context, parameterManager);
            for (int i = 0; i < parameterErrors?.Count; i++)
            {
                var validationError = parameterErrors[i];
                context.AllValidationErrors.Add(validationError);
                if (validationError.ErrorSeverity == ValidationError.Severity.Error)
                    Error(parameterErrors[i]);
            }
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
