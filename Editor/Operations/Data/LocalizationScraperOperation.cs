using System.Collections.Generic;
using PocketGems.Parameters.Editor.Operation;
using UnityEngine.TestTools;

namespace PocketGems.Parameters.Operations.Data
{
    /// <summary>
    /// Scrapes for all localization keys in the parameters.
    /// </summary>
    [ExcludeFromCoverage]
    internal class LocalizationScraperOperation : BasicOperation<IDataOperationContext>
    {
        private readonly List<string> _localizationKeys;
        private readonly HashSet<string> _viewedLocalizationKeys;

        /// <summary>
        /// Constructor for the operation.
        /// </summary>
        /// <param name="localizationKeysCollection">The collection to populate with found unique localization keys.</param>
        public LocalizationScraperOperation(List<string> localizationKeysCollection)
        {
            _localizationKeys = localizationKeysCollection;
            _viewedLocalizationKeys = new HashSet<string>();
        }

        /// <summary>
        /// Iterates through all columns of localization data types and extracts the localization key strings from them.
        /// </summary>
        /// <param name="parameterFileContainer">The parameter file containers to iterate over for data.</param>
        public override void Execute(IDataOperationContext context)
        {
            base.Execute(context);

            // REMEMBER TO CHECK ExcludeFromCoverage after this is re-implemented




            // var localizedStringDataType = new LocalizedStringDataType();
            // var arrayLocalizedStringDataType = new ArrayLocalizedStringDataType();

            // for (int i = 0; i < context.ParameterFiles.Count; i++)
            // {
            //     var parameterFile = context.ParameterFiles[i];
            //     for (int columnIndex = 0; columnIndex < parameterFile.ColumnTypes.Length; columnIndex++)
            //     {
            //         var columnType = parameterFile.ColumnTypes[columnIndex];
            //         // parsing LocalizedString column types
            //         if (columnType == localizedStringDataType.ColumnType)
            //         {
            //             for (int rowIndex = 0; rowIndex < parameterFile.RowDatas.Count; rowIndex++)
            //             {
            //                 var rowData = parameterFile.RowDatas[rowIndex];
            //                 SaveLocalizationKey(rowData[columnIndex]);
            //             }
            //         }
            //         // parsing Array<LocalizedString> column types
            //         else if (columnType == arrayLocalizedStringDataType.ColumnType)
            //         {
            //             for (int rowIndex = 0; rowIndex < parameterFile.RowDatas.Count; rowIndex++)
            //             {
            //                 var rowData = parameterFile.RowDatas[rowIndex];
            //                 var stringArray = arrayLocalizedStringDataType.ConstructStringArray(rowData[columnIndex]);
            //                 if (stringArray == null) continue;
            //                 for (int stringIndex = 0; stringIndex < stringArray.Length; stringIndex++)
            //                 {
            //                     SaveLocalizationKey(stringArray[stringIndex]);
            //                 }
            //             }
            //         }
            //     }
            // }
        }

        /// <summary>
        /// Checks and saves the localization key if it hasn't been seen already.
        /// </summary>
        /// <param name="localizationKey">The localization key</param>
        private void SaveLocalizationKey(string localizationKey)
        {
            if (localizationKey == null) return;
            var trimmedKey = localizationKey.Trim();
            if (trimmedKey.Length == 0) return;
            if (_viewedLocalizationKeys.Contains(trimmedKey)) return;

            _localizationKeys.Add(trimmedKey);
            _viewedLocalizationKeys.Add(trimmedKey);
        }
    }
}
