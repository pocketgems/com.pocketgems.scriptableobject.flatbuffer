using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using PocketGems.Parameters.PropertyTypes;
using PocketGems.Parameters.Util;

namespace PocketGems.Parameters.Models
{
    public abstract class ParameterInterface : IParameterInterface
    {
        protected ParameterInterface(Type type)
        {
            _type = type;
            _errors = new List<string>();

            Initialize();
        }

        public override string ToString() => $"{GetType().Name}:{_type.Name}";

        public Type Type => _type;

        public string InterfaceName => _type.Name;
        public string BaseName => NamingPattern().Matches(_type.Name)[0].Groups[1].Captures[0].Value;
        public string GeneratedNameSpace => ParameterConstants.GeneratedNamespace;
        public string FlatBufferClassName(bool includeExtension) => NamingUtil.FlatBufferClassNameFromBaseName(BaseName, includeExtension);
        public string FlatBufferStructName(bool includeExtension) => NamingUtil.FlatBufferStructNameFromBaseName(BaseName, includeExtension);
        public string ValidatorClassName(bool includeExtension) => NamingUtil.ValidatorClassNameFromBaseName(BaseName, includeExtension);

        public IReadOnlyList<Type> OrderedBaseInterfaceTypes { get; private set; }

        protected abstract Regex NamingPattern();
        protected abstract Type RequiredBaseType();

        public virtual bool Validate(out IReadOnlyList<string> outErrors)
        {
            var errors = new List<string>();
            errors.AddRange(_errors);

            // validate setup
            var regex = NamingPattern();

            if (regex.Matches(_type.Name).Count != 1)
                errors.Add($"Interface [{_type.Name}] must follow naming pattern {regex}.");
            if (!_type.IsPublic)
                errors.Add($"Interface [{_type.Name}] must be Public.");
            if (_type.Namespace != null)
                errors.Add($"Interface [{_type.Name}] must not have a namespace.");

            // validate interface sub interfaces
            var requiredInterface = RequiredBaseType();
            if (!requiredInterface.IsAssignableFrom(_type))
                errors.Add($"Interface [{_type.Name}] must be a sub interface of [{requiredInterface}].");
            for (int i = 0; i < OrderedBaseInterfaceTypes.Count; i++)
            {
                var baseInterface = OrderedBaseInterfaceTypes[i];
                if (baseInterface == requiredInterface || requiredInterface.IsAssignableFrom(baseInterface))
                    continue;
                errors.Add($"Interface [{_type.Name}] cannot derive from [{baseInterface}].");
            }

            // check for non-allowed definitions
            var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
            var methodInfos = _type.GetMethods(bindingFlags);
            for (int i = 0; i < methodInfos?.Length; i++)
            {
                if (!methodInfos[i].IsSpecialName)
                    errors.Add($"Interface [{_type.Name}] cannot define a method [{methodInfos[i].Name}].");
            }
            var propertyInfos = _type.GetProperties(bindingFlags);
            for (int i = 0; i < propertyInfos.Length; i++)
            {
                var propertyInfo = propertyInfos[i];
                if (propertyInfo.GetSetMethod() != null)
                    errors.Add($"Property [{propertyInfo.Name}] in interface [{_type.Name}] cannot define a setter.");
                var methodInfo = propertyInfo.GetMethod;
                if (!methodInfo.IsPublic || methodInfo.IsFamily || methodInfo.IsAssembly)
                    errors.Add($"Property [{propertyInfo.Name}] in interface [{_type.Name}] must be public.");
            }

            // validate properties
            HashSet<string> propertyNames = new HashSet<string>();
            for (int i = 0; i < PropertyTypes.Count; i++)
            {
                var propertyType = PropertyTypes[i];
                var propertyName = propertyType.PropertyInfo.Name;
                if (EditorParameterConstants.Interface.PropertyNameRegex.Matches(propertyName).Count != 1)
                    errors.Add($"Property [{propertyName}] in interface [{_type.Name}] must follow naming pattern {EditorParameterConstants.Interface.PropertyNameRegexString}.");
                if (propertyNames.Contains(propertyName))
                    errors.Add($"Duplicate property [{propertyName}] in interface [{_type.Name}].");
                propertyNames.Add(propertyName);
            }

            outErrors = errors;
            return errors.Count == 0;
        }

        public IReadOnlyList<IPropertyType> PropertyTypes { get; private set; }

        private void Initialize()
        {
            var interfaces = new List<Type>();

            void DFS(Type t)
            {
                var baseInterfaces = t.GetInterfaces().OrderBy(t => t.Name).ToList();
                foreach (var baseInterface in baseInterfaces)
                {
                    if (!interfaces.Contains(baseInterface))
                        DFS(baseInterface);
                }

                interfaces.Add(t);
            }

            DFS(_type);

            // iterate all properties of current interface & super interfaces
            List<IPropertyType> propertyTypes = new List<IPropertyType>();
            for (int i = 0; i < interfaces.Count; i++)
            {
                var propertyInfos = interfaces[i].GetProperties();
                for (int j = 0; j < propertyInfos.Length; j++)
                {
                    var propertyInfo = propertyInfos[j];
                    var propertyType = PropertyTypeFactory.Create(this, propertyInfo, out string error);
                    if (error != null || propertyType == null)
                    {
                        _errors.Add($"Property [{propertyInfo.Name}] of type [{propertyInfo.PropertyType.Name}] in interface [{_type.Name}] is not supported.");
                        continue;
                    }
                    propertyTypes.Add(propertyType);
                }
            }

            // remove the current type since it's not a base interface
            interfaces.Remove(_type);

            OrderedBaseInterfaceTypes = interfaces;
            PropertyTypes = propertyTypes;
        }

        private readonly Type _type;
        private readonly List<string> _errors;
    }
}
