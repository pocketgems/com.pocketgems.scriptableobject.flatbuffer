using System.Collections.Generic;
using PocketGems.Parameters.Common.Models.Editor;
using PocketGems.Parameters.Common.Util.Editor;
using PocketGems.Parameters.DataGeneration.LocalCSV.Rows.Editor;
using PocketGems.Parameters.Interface;

namespace PocketGems.Parameters.DataGeneration.LocalCSV.Editor
{
    internal class StructCSVFileCache : CSVFileCache<IBaseStruct, IParameterStruct>, IStructCSVFileCache
    {
        private readonly Dictionary<string, HashSet<string>> _baseNameToGuids;

        public StructCSVFileCache(string csvDir, bool attemptLoadExistingOnLoad) : base(csvDir, attemptLoadExistingOnLoad)
        {
            _baseNameToGuids = new Dictionary<string, HashSet<string>>();
        }

        protected override string BaseName<T>() => NamingUtil.BaseNameFromStructInterfaceName(typeof(T).Name);
        protected override bool RequiresIdentifier => false;

        public CSVRow LoadRow<T>(string guid) where T : IBaseStruct
        {
            var baseName = BaseName<T>();
            var csvFile = Load<T>();
            if (!_baseNameToGuids.TryGetValue(baseName, out var guids))
            {
                guids = new HashSet<string>();
                _baseNameToGuids[baseName] = guids;
            }
            guids.Add(guid);
            return csvFile.GetOrCreateRow(guid);
        }

        public IReadOnlyDictionary<string, HashSet<string>> LoadRowHistory => _baseNameToGuids;

        public void ClearLoadRowHistory() => _baseNameToGuids.Clear();
    }
}
