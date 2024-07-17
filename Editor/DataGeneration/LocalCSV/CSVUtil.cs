using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PocketGems.Parameters.Common.Editor;
using PocketGems.Parameters.Common.Models.Editor;
using PocketGems.Parameters.DataGeneration.LocalCSV.Rows.Editor;
using PocketGems.Parameters.DataGeneration.Util.Editor;

namespace PocketGems.Parameters.DataGeneration.LocalCSV.Editor
{
    internal static class CSVUtil
    {
        /// <summary>
        /// Sorts the rows in the same way that Unity Project window would sort the assets.
        /// </summary>
        /// <param name="rowDatas"></param>
        /// <returns></returns>
        public static IOrderedEnumerable<CSVRowData> IdentifierSort(IReadOnlyList<CSVRowData> rowDatas)
        {
            var comparator = new FileNameComparer();
            return rowDatas.OrderBy(x => x.Identifier, comparator);
        }

        public static IOrderedEnumerable<CSVRowData> GuidKeyPathSort(IReadOnlyList<CSVRowData> rowDatas)
        {
            var comparator = StructKeyPathBuilder.Comparer();
            return rowDatas.OrderBy(x => x.GUID, comparator);
        }

        public static void InvokeDefineSchema(string baseName, CSVFile csvFile, string assemblyName)
        {
            string methodName = EditorParameterConstants.CSVBridgeClass.DefineSchemaMethodName(baseName);
            InvokeStaticMethod(assemblyName, methodName, new object[] { csvFile });
        }

        public static IReadOnlyList<string> InvokeCheckSchema(
            IInfoCSVFileCache infoCSVFileCache, IStructCSVFileCache structCSVFileCache,
            string assemblyName)
        {
            string methodName = EditorParameterConstants.CSVBridgeClass.CheckSchemaMethodName;
            var args = new object[] { infoCSVFileCache, structCSVFileCache };
            return (IReadOnlyList<string>)InvokeStaticMethod(assemblyName, methodName, args);
        }

        public static IReadOnlyList<string> InvokeUpdateFromScriptableObjects(
            IInfoCSVFileCache infoCSVFileCache, IStructCSVFileCache structCSVFileCache,
            string assemblyName, Type interfaceType, List<IScriptableObjectMetadata> scriptableObjectsMetadatas)
        {
            string methodName = EditorParameterConstants.CSVBridgeClass.UpdateMethodName;
            var args = new object[] { infoCSVFileCache, structCSVFileCache, interfaceType, scriptableObjectsMetadatas };
            return (IReadOnlyList<string>)InvokeStaticMethod(assemblyName, methodName, args);
        }

        public static IReadOnlyList<string> InvokeReadToScriptableObjects(
            IInfoCSVFileCache infoCSVFileCache, IStructCSVFileCache structCSVFileCache,
            string assemblyName, Type interfaceType, List<IScriptableObjectMetadata> scriptableObjectsMetadatas)
        {
            string methodName = EditorParameterConstants.CSVBridgeClass.ReadMethodName;
            var args = new object[] { infoCSVFileCache, structCSVFileCache, interfaceType, scriptableObjectsMetadatas };
            return (IReadOnlyList<string>)InvokeStaticMethod(assemblyName, methodName, args);
        }

        private static object InvokeStaticMethod(string assemblyName, string methodName, object[] args)
        {
            // we must use reflection here because the generated class & assembly isn't guaranteed to exist
            // therefore compilation will fail
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
                throw new ArgumentException($"Couldn't find assembly {assemblyName}");
            string generatedNamespace = ParameterConstants.GeneratedNamespace;
            string className = EditorParameterConstants.CSVBridgeClass.ClassName;

            var type = assembly.GetType($"{generatedNamespace}.{className}");
            var methodInfo = type.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static);
            if (methodInfo == null)
                throw new ArgumentException($"Cannot find method {methodName} in type {type}");

            return (IReadOnlyList<string>)methodInfo.Invoke(null, args);
        }
    }
}
