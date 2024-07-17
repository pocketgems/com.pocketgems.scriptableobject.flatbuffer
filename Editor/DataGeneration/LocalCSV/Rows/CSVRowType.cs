using PocketGems.Parameters.DataGeneration.LocalCSV.Editor;

namespace PocketGems.Parameters.DataGeneration.LocalCSV.Rows.Editor
{
    internal class CSVRowType : CSVRow
    {
        internal CSVRowType(CSVFile csvFile, string[] data, string metadata = null, string hash = null) : base(
            csvFile, data, metadata, hash)
        {
        }
    }
}
