using System.Collections.Generic;
using System.IO;
using PocketGems.Parameters.Common.Models.Editor;
using PocketGems.Parameters.Common.Util.Editor;

namespace PocketGems.Parameters.DataGeneration.LocalCSV.Editor
{
    public abstract class CSVFileCache<T, U> : ICSVFileCache<T, U> where U : IParameterInterface
    {
        private readonly string _csvDir;
        protected readonly Dictionary<string, CSVFile> _baseNameToFile;

        protected CSVFileCache(string csvDir, bool attemptLoadExistingOnLoad)
        {
            _csvDir = csvDir;
            AttemptLoadExistingOnLoad = attemptLoadExistingOnLoad;
            _baseNameToFile = new Dictionary<string, CSVFile>();
        }

        public bool AttemptLoadExistingOnLoad { get; set; }

        protected abstract string BaseName<TV>();
        protected abstract bool RequiresIdentifier { get; }

        public bool IsLoaded<V>() where V : T
        {
            string baseName = BaseName<V>();
            return _baseNameToFile.ContainsKey(baseName);
        }

        public CSVFile Load<V>() where V : T
        {
            return Load(BaseName<V>());
        }

        public CSVFile Load(U parameterInterface)
        {
            var csvFile = Load(parameterInterface.BaseName);
            return csvFile;
        }

        /// <summary>
        /// Returns all loaded files
        /// </summary>
        /// <returns>dictionary with key as baseName and values as CSVFile</returns>
        public IReadOnlyDictionary<string, CSVFile> LoadedFiles() => _baseNameToFile;

        public void ClearCache() => _baseNameToFile.Clear();

        private CSVFile Load(string baseName)
        {
            if (!_baseNameToFile.TryGetValue(baseName, out CSVFile csvFile))
            {
                var csvFileName = NamingUtil.CSVFileNameFromBaseName(baseName, true);
                var csvFilePath = Path.Combine(_csvDir, csvFileName);
                csvFile = new CSVFile(csvFilePath, AttemptLoadExistingOnLoad, RequiresIdentifier);
                _baseNameToFile[baseName] = csvFile;
            }
            return csvFile;
        }
    }
}
