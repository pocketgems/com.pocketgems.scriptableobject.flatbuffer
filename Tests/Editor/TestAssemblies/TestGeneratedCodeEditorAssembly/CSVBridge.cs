using System;
using System.Collections.Generic;
using PocketGems.Parameters.Editor.LocalCSV;
using PocketGems.Parameters.Models;

namespace Parameters
{
    public class CSVBridge
    {
        public static int CheckSchemaCalls;
        public static int DefineSchemaCalls;
        public static int UpdateFromScriptableObjectsCalls;
        public static int ReadToScriptableObjectsCalls;
        public static readonly List<string> NextErrors = new List<string>();

        public static void Reset()
        {
            CheckSchemaCalls = 0;
            DefineSchemaCalls = 0;
            UpdateFromScriptableObjectsCalls = 0;
            ReadToScriptableObjectsCalls = 0;
            NextErrors.Clear();
        }

        private static IReadOnlyList<string> GetNextErrors()
        {
            var nextErrors = new List<string>(NextErrors);
            NextErrors.Clear();
            return nextErrors;
        }

        // these are defined in BaseOperationTest<T>
        private static void DefineSchemaCurrencyInfo(CSVFile csvFile) => DefineSchemaCalls++;
        private static void DefineSchemaBuildingInfo(CSVFile csvFile) => DefineSchemaCalls++;
        private static void DefineSchemaDragonInfo(CSVFile csvFile) => DefineSchemaCalls++;
        private static void DefineSchemaRewardStruct(CSVFile csvFile) => DefineSchemaCalls++;
        private static void DefineSchemaKeyValueStruct(CSVFile csvFile) => DefineSchemaCalls++;

        // invoked via reflection
        private static IReadOnlyList<string> CheckSchema(
            IInfoCSVFileCache infoCSVFileCache, IStructCSVFileCache structCSVFileCache)
        {
            CheckSchemaCalls++;
            return GetNextErrors();
        }

        // invoked via reflection
        private static IReadOnlyList<string> UpdateFromScriptableObjects(
            IInfoCSVFileCache infoCSVFileCache, IStructCSVFileCache structCSVFileCache,
            Type type, List<IScriptableObjectMetadata> scriptableObjectMetadatas)
        {
            // csvFile.DefineSchema(new[] { "Identifier" }, new[] { "string" });
            UpdateFromScriptableObjectsCalls++;
            return GetNextErrors();
        }

        // invoked via reflection
        private static IReadOnlyList<string> ReadToScriptableObjects(
            IInfoCSVFileCache infoCSVFileCache, IStructCSVFileCache structCSVFileCache,
            Type type, List<IScriptableObjectMetadata> scriptableObjectMetadatas)
        {
            ReadToScriptableObjectsCalls++;
            return GetNextErrors();
        }
    }
}
