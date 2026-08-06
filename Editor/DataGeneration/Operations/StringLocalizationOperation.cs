using System.Collections.Generic;
using PocketGems.Parameters.Common.Operations.Editor;
using PocketGems.Parameters.DataGeneration.Operation.Editor;

namespace PocketGems.Parameters.DataGeneration.Operations.Editor
{
    /// <summary>
    /// Scrapes for all localization keys in the parameters.
    /// </summary>
    internal class StringLocalizationOperation : BasicOperation<IDataOperationContext>
    {
        private readonly HashSet<string> _localizationKeys;
        private readonly HashSet<string> _localizedScripts;

        /// <summary>
        /// Constructor for the operation.
        /// </summary>
        /// <param name="localizationKeysCollection">The collection to populate with found unique localization keys.</param>
        public StringLocalizationOperation(out HashSet<string> localizationKeys, out HashSet<string> localizedScript)
        {
            _localizationKeys = new HashSet<string>();
            _localizedScripts = new HashSet<string>();
            localizationKeys = _localizationKeys;
            localizedScript = _localizedScripts;
        }

        /// <summary>
        /// Iterates through all columns of localization data types and extracts the localization key strings from them.
        /// </summary>
        /// <param name="parameterFileContainer">The parameter file containers to iterate over for data.</param>
        public override void Execute(IDataOperationContext context)
        {
            base.Execute(context);

            foreach (var metaDatas in context.ScriptableObjectMetadatas.Values)
            {
                foreach (var metaData in metaDatas)
                {
                    metaData.ScriptableObject.CollectLocalizationStrings(_localizationKeys, _localizedScripts);
                }
            }
        }
    }
}
