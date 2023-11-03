using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PocketGems.Parameters.Interface;

namespace PocketGems.Parameters.Validation
{
    /// <summary>
    /// Static cache for caching results for data needed through reflection about objects & properties.
    ///
    /// Since the results are derived from Reflection, the data should never be invalid for the currently loaded assemblies.
    /// </summary>
    internal static class ParameterManagerValidatorCache
    {
        public static IReadOnlyList<ValidationError> Errors => s_errors;

        public static void Initialize()
        {
            if (s_dataValidators == null)
                s_dataValidators = SearchForAllValidators();
            if (s_allInterfaceTypes == null)
                s_allInterfaceTypes = new Dictionary<Type, List<Type>>();
            if (s_typeToReferenceInfos == null)
                s_typeToReferenceInfos = new Dictionary<Type, List<StructReferenceInfo>>();
        }

        /// <summary>
        /// Get all DataValidators.
        ///
        /// Maps parameter interface type to it's respective validator, either IDataValidator or IDataValidatorStruct
        /// </summary>
        /// <returns></returns>
        public static Dictionary<Type, Type> GetDataValidatorMap()
        {
            Initialize();
            return s_dataValidators;
        }

        /// <summary>
        /// Gets all StructReferenceInfo for the current type (doesn't include StructReferenceInfos from super interfaces)
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IReadOnlyList<StructReferenceInfo> GetStructReferenceInfos(Type type)
        {
            Initialize();
            return SearchAllStructReferences(type);
        }

        /// <summary>
        /// Return all parameter interfaces including the input type
        /// </summary>
        /// <param name="type">input type</param>
        /// <returns>all parameter super interfaces of type including type</returns>
        public static IReadOnlyList<Type> GetAllInterfaceTypes(Type type)
        {
            Initialize();
            if (!s_allInterfaceTypes.TryGetValue(type, out var allTypes))
            {
                allTypes = SearchAllInterfaceTypes(type);
                s_allInterfaceTypes[type] = allTypes;
            }
            return allTypes;
        }

        /// <summary>
        /// Clears the Cache.  Mainly used for unit testing.
        /// </summary>
        public static void Clear()
        {
            s_dataValidators = null;
            s_allInterfaceTypes?.Clear();
            s_errors.Clear();
            s_typeToReferenceInfos?.Clear();
        }

        private static Dictionary<Type, List<Type>> s_allInterfaceTypes;
        private static Dictionary<Type, Type> s_dataValidators;
        private static Dictionary<Type, List<StructReferenceInfo>> s_typeToReferenceInfos;
        private static readonly List<ValidationError> s_errors = new List<ValidationError>();

        private static List<Type> SearchAllInterfaceTypes(Type type)
        {
            HashSet<Type> visited = new HashSet<Type>();
            var interfaces = new List<Type>();

            void Visit(Type innerType)
            {
                if (innerType == typeof(IBaseInfo) || innerType == typeof(IBaseStruct))
                    return;
                if (visited.Contains(innerType))
                    return;
                visited.Add(innerType);
                interfaces.Add(innerType);
                var superInterfaces = innerType.GetInterfaces();
                for (int i = 0; i < superInterfaces.Length; i++)
                    Visit(superInterfaces[i]);
            }

            Visit(type);
            return interfaces;
        }

        private static IReadOnlyList<StructReferenceInfo> SearchAllStructReferences(Type type)
        {
            if (!s_typeToReferenceInfos.ContainsKey(type))
            {
                HashSet<Type> toVisitTypes = new HashSet<Type>();
                toVisitTypes.Add(type);

                /*
                 * Iterate through Type and grab all properties that reference a Struct and populate typeToReferenceInfos
                 */
                while (toVisitTypes.Count > 0)
                {
                    var currentType = toVisitTypes.First();
                    toVisitTypes.Remove(currentType);
                    // base interfaces - exit
                    if (currentType == typeof(IBaseStruct) || currentType == typeof(IBaseInfo))
                        continue;

                    // already visited
                    if (s_typeToReferenceInfos.ContainsKey(currentType))
                        continue;

                    var structReferenceInfos = new List<StructReferenceInfo>();
                    s_typeToReferenceInfos[currentType] = structReferenceInfos;

                    // iterate through all properties and find struct references
                    PropertyInfo[] props = currentType.GetProperties();
                    for (int i = 0; i < props.Length; i++)
                    {
                        bool isList = false;
                        Type parameterStructRefPropertyType = null;

                        var prop = props[i];
                        var propertyType = prop.PropertyType;
                        // Check ParameterStructReference<>
                        if (propertyType.IsGenericType &&
                            propertyType.GetGenericTypeDefinition() == typeof(ParameterStructReference<>))
                        {
                            isList = false;
                            parameterStructRefPropertyType = propertyType;
                        }

                        // Check IReadOnlyList<ParameterStructReference<>>
                        if (propertyType.IsGenericType &&
                            propertyType.GetGenericTypeDefinition() == typeof(IReadOnlyList<>))
                        {
                            var innerPropertyType = propertyType.GetGenericArguments()[0];
                            if (innerPropertyType.IsGenericType &&
                                innerPropertyType.GetGenericTypeDefinition() == typeof(ParameterStructReference<>))
                            {
                                isList = true;
                                parameterStructRefPropertyType = innerPropertyType;
                            }
                        }

                        if (parameterStructRefPropertyType == null)
                            continue;

                        var methodName = nameof(ParameterStructReference<IBaseStruct>.GetStruct);
                        var methodInfo = parameterStructRefPropertyType.GetMethod(methodName,
                            BindingFlags.NonPublic | BindingFlags.Instance);
                        if (methodInfo == null)
                        {
                            var error = new ValidationError(null, null, null, $"Unable to find methodInfo for {methodName}");
                            s_errors.Add(error);
                            continue;
                        }

                        // populate data structures
                        var structType = parameterStructRefPropertyType.GetGenericArguments()[0];
                        var refInfo = new StructReferenceInfo(prop, structType, isList, methodInfo);
                        structReferenceInfos.Add(refInfo);
                        toVisitTypes.Add(structType);
                    }

                    // grab super interfaces
                    var interfaces = currentType.GetInterfaces();
                    for (int j = 0; j < interfaces.Length; j++)
                        toVisitTypes.Add(interfaces[j]);
                }
            }

            return s_typeToReferenceInfos[type];
        }

        /// <summary>
        /// Helper function to find all implementations of IDataValidator and construct them.
        ///
        /// WARNING: this is a costly operation to run at runtime for live users since it scans all assemblies & types.
        /// </summary>
        /// <returns>interface type to type of class implementing the IDataValidator</returns>
        private static Dictionary<Type, Type> SearchForAllValidators()
        {
            // interface type -> type implementing ITypedDataValidator
            // e.g. ICurrencyInfo -> CurrencyInfoDataValidator
            Dictionary<Type, Type> validatorTypes = new Dictionary<Type, Type>();

            // get all types
            AppDomain currentDomain = AppDomain.CurrentDomain;
            Assembly[] assemblies = currentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; i++)
            {
                var assembly = assemblies[i];
                // GetType() dies when iterating through BrotliSharpLib due to a struct being over 1MB
                if (assembly.FullName.StartsWith("BrotliSharpLib"))
                    continue;
                var types = assembly.GetTypes();
                for (int j = 0; j < types.Length; j++)
                {
                    var type = types[j];
                    if (type.IsAbstract)
                        continue;
                    var interfaces = type.GetInterfaces();
                    for (int k = 0; k < interfaces.Length; k++)
                    {
                        var interfaceType = interfaces[k];
                        if (!interfaceType.IsGenericType)
                            continue;
                        var genericTypeDefinition = interfaceType.GetGenericTypeDefinition();
                        if (genericTypeDefinition != typeof(ITypedDataValidator<>) &&
                            genericTypeDefinition != typeof(ITypedDataValidatorStruct<>))
                            continue;
                        var genericType = interfaceType.GetGenericArguments()[0];
                        if (validatorTypes.ContainsKey(genericType))
                        {
                            var error = new ValidationError(null, null, null, $"Found more than one validator for {genericType}");
                            s_errors.Add(error);
                        }

                        validatorTypes[genericType] = type;
                    }
                }
            }

            return validatorTypes;
        }
    }
}
