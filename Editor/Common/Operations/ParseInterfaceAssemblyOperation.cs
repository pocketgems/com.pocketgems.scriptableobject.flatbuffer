using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using PocketGems.Parameters.Common.Editor;
using PocketGems.Parameters.Common.Models.Editor;
using PocketGems.Parameters.Common.Operation.Editor;
using PocketGems.Parameters.Common.Util.Editor;
using PocketGems.Parameters.Interface;
using ParameterInfo = PocketGems.Parameters.Common.Models.Editor.ParameterInfo;

namespace PocketGems.Parameters.Common.Operations.Editor
{
    internal class ParseInterfaceAssemblyOperation<T> : BasicOperation<T> where T : ICommonOperationContext
    {
        public override void Execute(T context)
        {
            base.Execute(context);
            Execute(context);
        }

        private void Execute(ICommonOperationContext context)
        {
            var assemblyName = context.InterfaceAssemblyName;
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            Assembly assembly = null;
            for (int i = 0; i < assemblies.Length; i++)
            {
                var name = assemblies[i].GetName().Name;
                if (name == assemblyName)
                {
                    assembly = assemblies[i];
                    break;
                }
            }

            if (assembly == null)
            {
                var interfaceFolder = context.InterfaceDirectoryRootPath;
                if (!Directory.Exists(interfaceFolder))
                {
                    ShortCircuit();
                    ParameterDebug.Log($"No data to generate, {interfaceFolder} doesn't exist.");
                    return;
                }
                Error($"Couldn't find assembly {assemblyName}");
                return;
            }

            var types = assembly.GetTypes();
            for (int i = 0; i < types.Length; i++)
            {
                var type = types[i];
                if (type.IsEnum)
                {
                    context.ParameterEnums.Add(new ParameterEnum(type));
                    continue;
                }

                if (type.IsInterface)
                {
                    bool isBaseInfo = typeof(IBaseInfo).IsAssignableFrom(type);
                    bool isStructInfo = typeof(IBaseStruct).IsAssignableFrom(type);
                    if (isBaseInfo && isStructInfo)
                    {
                        Error($"{type} in assembly {assemblyName} is both a {nameof(IBaseInfo)} and {nameof(IBaseStruct)}.  This isn't allowed.");
                        continue;
                    }
                    if (isBaseInfo)
                    {
                        context.ParameterInfos.Add(new ParameterInfo(type));
                        continue;
                    }
                    if (isStructInfo)
                    {
                        context.ParameterStructs.Add(new ParameterStruct(type));
                        continue;
                    }
                }

                // generated types from Roslyn analyzers and source generators
                // https://docs.unity3d.com/Manual/roslyn-analyzers.html
                var fullName = type.FullName;
                if (fullName != null && (fullName.Contains("PrivateImplementationDetails") ||
                                         fullName.Contains("UnitySourceGeneratedAssemblyMonoScriptTypes") ||
                                         fullName.Contains("Microsoft.CodeAnalysis") ||
                                         fullName.Contains("System.Runtime.CompilerServices") ||
                                         fullName.Contains("ParameterInterfaces.AssemblyHelper")))
                    continue;

                Error($"{type} in assembly {assemblyName} isn't valid -  only enums, {nameof(IBaseInfo)}, and {nameof(IBaseStruct)} are allowed.");
            }

            // GetTypes() order follows the compiler's source file order, which differs between Unity's compiler and
            // a standalone build of the same interfaces. These lists set the FlatBuffer root slot order, so sort them
            // by name or the data writer and reader can disagree depending on which generator produced them.
            context.ParameterEnums.Sort((a, b) => string.CompareOrdinal(a.Type.FullName, b.Type.FullName));
            context.ParameterInfos.Sort((a, b) => string.CompareOrdinal(a.Type.FullName, b.Type.FullName));
            context.ParameterStructs.Sort((a, b) => string.CompareOrdinal(a.Type.FullName, b.Type.FullName));

            context.InterfaceAssemblyHash = InterfaceAssemblyHash(context.ParameterEnums, context.ParameterInfos,
                context.ParameterStructs);
            ParameterDebug.LogVerbose($"InterfaceAssemblyHash: {context.InterfaceAssemblyHash}");
        }

        private static string InterfaceAssemblyHash(List<IParameterEnum> enums, List<IParameterInfo> parameterInfos,
            List<IParameterStruct> parameterStructs)
        {
            StringBuilder s = new StringBuilder();
            IOrderedEnumerable<IParameterEnum> sortedEnums = enums.OrderBy(e => e.Type.Name, StringComparer.Ordinal);
            foreach (var parameterEnum in sortedEnums)
            {
                s.AppendOSAgnosticNewLine();
                s.Append($"{parameterEnum.Type.Name} : {Enum.GetUnderlyingType(parameterEnum.Type)}");
                foreach (object attribute in parameterEnum.Type.GetCustomAttributes(true)
                             .OrderBy(a => a.ToString(), StringComparer.Ordinal))
                {
                    s.AppendOSAgnosticNewLine();
                    s.Append($"  {attribute}");
                }
                foreach (var value in Enum.GetValues(parameterEnum.Type))
                {
                    s.AppendOSAgnosticNewLine();
                    s.Append($"  {value}");
                }
            }

            void AppendPropertyType(Type propertyType, StringBuilder s)
            {
                s.Append(propertyType.Name);
                if (propertyType.IsGenericType)
                {
                    s.Append("<");
                    var genericArgs = propertyType.GetGenericArguments();
                    for (int i = 0; i < genericArgs.Length; i++)
                        AppendPropertyType(genericArgs[i], s);
                    s.Append(">");
                }
            }

            void AppendParameterInterface(IParameterInterface parameterInterface)
            {
                s.AppendOSAgnosticNewLine();
                s.Append($"{parameterInterface.InterfaceName}");
                var baseTypes = parameterInterface.OrderedBaseInterfaceTypes;
                for (int i = 0; i < baseTypes.Count; i++)
                {
                    s.Append(i == 0 ? " : " : ", ");
                    s.Append(baseTypes[i].Name);
                }

                for (int i = 0; i < parameterInterface.PropertyTypes.Count; i++)
                {
                    // scriptable object attribute additions
                    var propertyType = parameterInterface.PropertyTypes[i];
                    var propertyInfo = propertyType.PropertyInfo;
                    var attributes = propertyType.ScriptableObjectFieldAttributesCode();
                    for (int j = 0; j < attributes?.Count; j++)
                    {
                        s.AppendOSAgnosticNewLine();
                        s.Append($"  {attributes[j]}:");
                    }

                    // this will catch if a localization key was added to a string and code gen was not ran
                    var localizationCode = propertyType.ScriptableObjectCollectLocalizationStringsCode("str1", "str2");
                    if (!string.IsNullOrWhiteSpace(localizationCode))
                    {
                        s.AppendOSAgnosticNewLine();
                        s.Append("  Localization");
                    }

                    // append properties
                    s.AppendOSAgnosticNewLine();
                    s.Append($"  {propertyInfo.Name}:");
                    AppendPropertyType(propertyInfo.PropertyType, s);
                }
            }

            // Hash in root slot order (the lists are already sorted in Execute) so that any change to slot order
            // changes the hash and the loader refuses mismatched data instead of misreading it.
            foreach (IParameterInfo paramInterface in parameterInfos)
                AppendParameterInterface(paramInterface);

            foreach (IParameterStruct paramInterface in parameterStructs)
                AppendParameterInterface(paramInterface);

            s.AppendOSAgnosticNewLine();

#if ADDRESSABLE_PARAMS
            s.Append("Asset build for Addressables");
#else
            s.Append("Asset build for Resources");
#endif

            s.AppendOSAgnosticNewLine();
            s.Append(EditorParameterConstants.InterfaceHashSalt);
            return HashUtil.MD5Hash(s.ToString());
        }
    }

    internal static class StringBuilderExt
    {
        // append the same newline regardless of OS so that the interface hash is consistent across OS.
        public static void AppendOSAgnosticNewLine(this StringBuilder s) => s.Append('\n');
    }
}
