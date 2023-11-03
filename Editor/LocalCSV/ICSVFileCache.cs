using System.Collections.Generic;
using PocketGems.Parameters.Models;

namespace PocketGems.Parameters.Editor.LocalCSV
{
    public interface ICSVFileCache<T, U> where U : IParameterInterface
    {
        /// <summary>
        ///  Getter/Setter for future Load calls to determine if the file should be loaded from disk.
        /// </summary>
        bool AttemptLoadExistingOnLoad { get; set; }

        /// <summary>
        /// Checks if a current file has been loaded.
        /// </summary>
        /// <typeparam name="V">interface V to check.</typeparam>
        /// <returns>true if Load or Load<> has been called on a file</returns>
        bool IsLoaded<V>() where V : T;

        /// <summary>
        /// Load a CSV file by interface T
        /// </summary>
        /// <typeparam name="V"></typeparam>
        /// <returns>loaded or new CSV file</returns>
        CSVFile Load<V>() where V : T;

        /// <summary>
        /// Load a CSV by parameter interface
        /// </summary>
        /// <param name="parameterInterface">parameter interface to load</param>
        /// <returns>loaded CSV file</returns>
        CSVFile Load(U parameterInterface);

        /// <summary>
        /// Returns all loaded CSVFiles.
        /// </summary>
        /// <returns>dictionary of baseName to CSVFile</returns>
        IReadOnlyDictionary<string, CSVFile> LoadedFiles();

        /// <summary>
        /// Clears the cache of any loaded files.  Any unsaved modifications to CSVFiles are discarded.
        /// </summary>
        void ClearCache();
    }
}
