using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using PocketGems.Parameters.Editor.Operation;
using PocketGems.Parameters.Interface;
using PocketGems.Parameters.Models;
using PocketGems.Parameters.Util;
using ParameterInfo = PocketGems.Parameters.Models.ParameterInfo;

namespace PocketGems.Parameters.Operations
{
    internal class ParseInterfaceAssemblyOperation : BasicMultiOperation<ICodeOperationContext, IDataOperationContext>
    {
        public override void Execute(ICodeOperationContext context)
        {
            base.Execute(context);
            Execute(context);
        }

        public override void Execute(IDataOperationContext context)
        {
            base.Execute(context);
            Execute(context);
        }

        private void Execute(ICommonOperationContext context)
        {
            var assemblyName = context.InterfaceAssemblyName;
            var interfaceFolder = context.InterfaceDirectoryRootPath;
            if (!Directory.Exists(interfaceFolder))
            {
                ShortCircuit();
                ParameterDebug.Log($"No data to generate, {interfaceFolder} doesn't exist.");
                return;
            }

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            Assembly assembly = null;
            for (int i = 0; i < assemblies.Length; i++)
            {
                if (assemblies[i].GetName().Name == assemblyName)
                {
                    assembly = assemblies[i];
                    break;
                }
            }

            if (assembly == null)
            {
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
                                         fullName.Contains("UnitySourceGeneratedAssemblyMonoScriptTypes")))
                    continue;

                Error($"{type} in assembly {assemblyName} isn't valid -  only enums, {nameof(IBaseInfo)}, {nameof(IBaseStruct)} and are allowed.");
            }

            context.InterfaceAssemblyHash = InterfaceAssemblyHash(context.ParameterEnums, context.ParameterInfos,
                context.ParameterStructs);
            ParameterDebug.LogVerbose($"InterfaceAssemblyHash: {context.InterfaceAssemblyHash}");
        }

        private static string InterfaceAssemblyHash(List<IParameterEnum> enums, List<IParameterInfo> parameterInfos,
            List<IParameterStruct> parameterStructs)
        {
            StringBuilder s = new StringBuilder();
            var sortedEnums = enums.OrderBy(e => e.Type.Name);
            foreach (var parameterEnum in sortedEnums)
            {
                s.AppendOSAgnosticNewLine();
                s.Append($"{parameterEnum.Type.Name} : {Enum.GetUnderlyingType(parameterEnum.Type)}");
                foreach (var attribute in parameterEnum.Type.GetCustomAttributes(true))
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

                    // append properties
                    s.AppendOSAgnosticNewLine();
                    s.Append($"  {propertyInfo.Name}:");
                    AppendPropertyType(propertyInfo.PropertyType, s);
                }
            }

            // parameter infos
            var sortedInfoInterfaces = parameterInfos.OrderBy(i => i.InterfaceName);
            foreach (var paramInterface in sortedInfoInterfaces)
                AppendParameterInterface(paramInterface);

            // struct infos
            var sortedStructInterfaces = parameterStructs.OrderBy(i => i.InterfaceName);
            foreach (var paramInterface in sortedStructInterfaces)
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
