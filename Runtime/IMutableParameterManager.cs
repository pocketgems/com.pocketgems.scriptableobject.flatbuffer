using System.Collections.Generic;
using PocketGems.Parameters.Interface;

namespace PocketGems.Parameters
{
    /// <summary>
    /// An IParameterManager that also loads rows, and deletes, adds and edits them through overrides.
    /// </summary>
    public interface IMutableParameterManager : IParameterManager
    {
        /// <summary>
        /// Load the parameter file into the manager to be indexed by type T and by identifier.  This method compliments
        /// the Get and Get function in the IParameterManager interface..
        /// </summary>
        /// <param name="parameter">Parameter to load</param>
        /// <param name="identifier">the unique identifier for type T.</param>
        /// <param name="guid">the unique Unity guid for the type T.</param>
        /// <typeparam name="T">Type of the IBaseInfo class.</typeparam>
        /// <typeparam name="G">Type that implements both T and IMutableParameter.</typeparam>
        void Load<T, G>(G parameter, string identifier, string guid) where T : class, IBaseInfo
                                                                     where G : T, IMutableParameter;

        /// <summary>
        /// Load a struct into the manager to be indexed by type T and by guid.  This method compliments the
        /// GetStructWithGuid function in the IParameterManager interface.
        /// </summary>
        /// <param name="parameter">Parameter to load</param>
        /// <param name="guid">the unique guid for type T: the hash of the struct's key path.</param>
        /// <typeparam name="T">Type of the IBaseStruct class.</typeparam>
        /// <typeparam name="G">Type that implements both T and IMutableParameter.</typeparam>
        void Load<T, G>(G parameter, string guid) where T : class, IBaseStruct
                                                        where G : T, IMutableParameter;

        /// <summary>
        /// Removes every row loaded by Load<>(), along with the rows that ApplyOverrides added and deleted and the
        /// edits on the removed rows.  Load the data again, then apply the overrides again.
        /// </summary>
        void RemoveAll();

        /// <summary>
        /// Creates the rows that ApplyOverrides adds.  The generated ParameterDataLoader sets itself here when it
        /// loads data.  Without one, adding rows fails.
        /// </summary>
        IParameterFactory ParameterFactory { get; set; }

        /// <summary>
        /// Applies a json object of overrides that delete, add and edit rows of the loaded data.  Each action lists
        /// its rows by csv.  A key can't repeat within an object.
        ///
        /// Deletes apply first, then adds, then edits.  A row can't be both deleted and added; edit it instead.  Within
        /// each action, csvs, rows and columns apply in ordinal order, whatever order the json lists them in, so the
        /// order of added rows and of the errors doesn't depend on how the json was written.
        ///
        /// - delete: hides info or struct rows loaded from data.  Rows added or edited by overrides can't be deleted.
        ///   Rows that point to a deleted row don't change, so edit them too.
        /// - add: adds info rows (by identifier) or struct rows (by key path).  The values must set every column.  A
        ///   struct reference can point to a struct added in the same call; it isn't checked until it's read.  Added
        ///   rows are read-only.
        /// - edit: overrides columns of loaded rows with values given as text.  A single struct reference takes the
        ///   struct's key path or guid, which isn't checked until it's read.  Editing an added row is an error.
        /// </summary>
        /// <example>
        /// <code>
        /// // json schema
        /// {
        ///   "delete": { "csvName": ["identifier, guid or key path"] },
        ///   "add": { "csvName": { "identifier or key path": { "column": "value" } } },
        ///   "edit": { "csvName": { "identifier, guid or key path": { "column": "value" } } }
        /// }
        /// // example
        /// {
        ///   "delete": { "CurrencyInfo.csv": ["OldCoin", "OldRuby"] },
        ///   "add": {
        ///     "CurrencyInfo.csv": {
        ///       "Gem": { "Description": "I'm a gem!", "Reward": "CurrencyInfo[Gem].Reward" },
        ///       "Gem2": { "Description": "I'm a gem!", "Reward": "CurrencyInfo[Gem].Reward" }
        ///     },
        ///     "RewardStruct.csv": { "CurrencyInfo[Gem].Reward": { "Amount": "5" } }
        ///   },
        ///   "edit": {
        ///     "CurrencyInfo.csv": { "Coin": { "Description": "I'm a coin!" } },
        ///     "BuildingInfo.csv": { "House": { "BuildCost": "10" } }
        ///   }
        /// }
        /// </code>
        /// </example>
        /// <param name="json">the overrides as a json string.  Null or empty applies nothing.</param>
        /// <param name="errors">List of error strings for any errors, null otherwise.
        /// If there is an error, none of the overrides in this call are applied.</param>
        /// <returns>true if every override applied</returns>
        bool ApplyOverrides(string json, out IReadOnlyList<string> errors);

        /// <summary>
        /// Clears all overrides applied with ApplyOverrides, including added and deleted rows.
        /// </summary>
        void ClearAllOverrides();
    }
}
