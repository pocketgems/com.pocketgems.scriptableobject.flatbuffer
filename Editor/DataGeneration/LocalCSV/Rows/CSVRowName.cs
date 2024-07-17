using PocketGems.Parameters.DataGeneration.LocalCSV.Editor;

namespace PocketGems.Parameters.DataGeneration.LocalCSV.Rows.Editor
{
    internal class CSVRowName : CSVRow
    {
        public CSVRowName(CSVFile csvFile, string[] data, string metadata = "Metadata", string hash = null) : base(
            csvFile, data, metadata, hash)
        {
        }
    }
}
