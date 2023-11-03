using System.Collections.Generic;

namespace PocketGems.Parameters.Editor.LocalCSV.Rows
{
    public abstract class CSVRow
    {
        public bool HashMatches => _hash != null && _hash == ActualHash;
        public IReadOnlyList<string> Data => _data;

        protected CSVRow(CSVFile csvFile, string[] data, string metadata, string hash = null)
        {
            _csvFile = csvFile;
            _data = data;
            Metadata = metadata;
            _hash = hash;
        }

        internal string Metadata
        {
            get => _metadata;
            set
            {
                _actualHash = null;
                _metadata = value;
            }
        }

        internal string ActualHash
        {
            get
            {
                if (_actualHash == null)
                {
                    // identifier is already part of _data
                    var joinedData = string.Join(",", _data);
                    joinedData += $",{_metadata}";
                    _actualHash = _csvFile.ComputeHash(joinedData);
                }

                return _actualHash;
            }
        }

        protected CSVFile _csvFile;
        protected string[] _data;
        private string _metadata;
        private string _actualHash;
        private readonly string _hash;
    }
}
