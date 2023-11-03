namespace PocketGems.Parameters.Editor.LocalCSV.Rows
{
    internal class CSVRowType : CSVRow
    {
        internal CSVRowType(CSVFile csvFile, string[] data, string metadata = null, string hash = null) : base(
            csvFile, data, metadata, hash)
        {
        }
    }
}
