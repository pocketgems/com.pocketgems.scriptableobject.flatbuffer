namespace PocketGems.Parameters.Editor.LocalCSV.Rows
{
    public class CSVRowData : CSVRow
    {
        public string Identifier => _data?[0];

        public void UpdateData(string[] data)
        {
            _data = data;
            _csvFile.RowDataChanged(this);
        }

        public string GUID
        {
            get => Metadata;
            set
            {
                if (Metadata == value)
                    return;
                var oldValue = Metadata;
                Metadata = value;
                _csvFile.RowDataChanged(this, oldValue, value);
            }
        }

        internal CSVRowData(CSVFile csvFile, string[] data, string guid, string hash = null) :
            base(csvFile, data, guid, hash)
        {

        }
    }
}
