using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using PocketGems.Parameters.Interface;

namespace PocketGems.Parameters
{
    /// <summary>
    /// A mutable IParameterManager can load and fetch parameter data in the form of a IFlatbufferObjects
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
        /// Load the parameter file into the manager to be indexed by type T and by identifier.  This method compliments
        /// the Get and Get function in the IParameterManager interface..
        /// </summary>
        /// <param name="parameter">Parameter to load</param>
        /// <param name="identifier">the unique identifier for type T.</param>
        /// <typeparam name="T">Type of the IBaseInfo class.</typeparam>
        /// <typeparam name="G">Type that implements both T and IMutableParameter.</typeparam>
        void Load<T, G>(G parameter, string guid) where T : class, IBaseStruct
                                                        where G : T, IMutableParameter;

        /// <summary>
        /// Removes all parameters loaded by Load<>()
        /// </summary>
        void RemoveAll();

        /// <summary>
        /// Applies a json dict of diffs to apply modifications to override loaded data.
        ///
        /// API is mainly used for diffs retrieved from an AB testing service.
        ///
        /// Currently only edits are supported.  In the future, "add" will be supported as well.
        /// </summary>
        /// <example>
        /// <code>
        /// // json schema
        /// {
        ///   "edit": [
        ///     [
        ///       "csvName",
        ///       "identifier/guid",
        ///       "column",
        ///       "value"
        ///     ]
        ///   ]
        /// }
        /// // example
        /// {
        ///   "edit": [
        ///     [
        ///       "CurrencyInfo.csv",
        ///       "Coin",
        ///       "Description",
        ///       "I'm a coin!"
        ///     ],
        ///     [
        ///       "BuildingInfo.csv",
        ///       "House",
        ///       "BuildCost",
        ///       "10"
        ///     ]
        ///   ]
        /// }
        /// </code>
        /// </example>
        /// <param name="json">json for the diff</param>
        /// <param name="errors">List of error strings for any errors, null otherwise.
        /// If there is an error, the override will only be partially applied.
        /// Call ClearAllOverrides() to remove all overrides.</param>
        /// <returns></returns>
        bool ApplyOverrides(JObject json, out IReadOnlyList<string> errors);

        /// <summary>
        /// Clears all overrides called with ApplyOverrides.
        /// </summary>
        void ClearAllOverrides();
    }
}
