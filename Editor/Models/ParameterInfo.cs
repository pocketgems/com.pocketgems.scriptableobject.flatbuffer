using System;
using System.Linq;
using System.Text.RegularExpressions;
using PocketGems.Parameters.Interface;
using PocketGems.Parameters.Util;
using UnityEngine.TestTools;

namespace PocketGems.Parameters.Models
{
    public class ParameterInfo : ParameterInterface, IParameterInfo
    {
        public ParameterInfo(Type type) : base(type)
        {
        }

        protected override Regex NamingPattern() => EditorParameterConstants.Interface.InfoNameRegex;
        protected override Type RequiredBaseType() => typeof(IBaseInfo);

        public string ScriptableObjectClassName(bool includeExtension) => NamingUtil.ScriptableObjectClassNameFromBaseName(BaseName, includeExtension);

        private Type _scriptableObjectType;
        [ExcludeFromCoverage]
        public Type ScriptableObjectType()
        {
            if (_scriptableObjectType != null)
                return _scriptableObjectType;

            // we must use reflection here because the generated class & assembly isn't guaranteed to exist
            // therefore compilation will fail
            var generatedEditorAssemblyName = EditorParameterConstants.GeneratedCode.EditorAssemblyName;
            var assembly = AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault(assembly =>
                assembly.GetName().Name == generatedEditorAssemblyName);
            if (assembly == null)
                throw new ArgumentException($"Couldn't find assembly {generatedEditorAssemblyName}");

            string generatedNamespace = ParameterConstants.GeneratedNamespace;

            var typePath = $"{generatedNamespace}.{ScriptableObjectClassName(false)}";
            _scriptableObjectType = assembly.GetType(typePath);
            if (_scriptableObjectType == null)
                throw new ArgumentException($"Cannot type {typePath}");
            return _scriptableObjectType;
        }
    }
}
