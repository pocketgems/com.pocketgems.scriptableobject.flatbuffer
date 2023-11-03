namespace PocketGems.Parameters.Editor.LocalCSV.Rows
{
    internal class CSVRowName : CSVRow
    {
        public CSVRowName(CSVFile csvFile, string[] data, string metadata = "Metadata", string hash = null) : base(
            csvFile, data, metadata, hash)
        {
        }
    }
}
