using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using PocketGems.Parameters.Common.Util.Editor;
using PocketGems.Parameters.DataGeneration.LocalCSV.Rows.Editor;

namespace PocketGems.Parameters.DataGeneration.LocalCSV.Editor
{
    public class CSVFile
    {
        public string InterfaceHash
        {
            get
            {
                if (_columnTypes != null)
                    return _columnTypes.Metadata;
                return _interfaceHash ?? "";
            }
            set
            {
                _dirty |= value != _interfaceHash;
                if (_columnTypes != null)
                    _columnTypes.Metadata = value;
                _interfaceHash = value;
            }
        }

        public string Filename => Path.GetFileNameWithoutExtension(_filePath);
        public string FilePath => _filePath;
        public bool IsDirty => _dirty;
        public bool AttemptLoadExisting => _attemptLoadExisting;
        public bool RequiresIdentifier => _requiresIdentifier;
        public bool ColumnSchemaModified => _columnNames == null || !_columnNames.HashMatches ||
                                            _columnTypes == null || !_columnTypes.HashMatches;
        public IReadOnlyList<CSVRowData> RowData => _allRows;

        public CSVFile(string filePath, bool attemptLoadExisting, bool requiresIdentifier)
        {
            _allRows = new List<CSVRowData>();
            _guidToCSVRow = new Dictionary<string, List<CSVRowData>>();

            _filePath = filePath;
            _dirty = true;
            _attemptLoadExisting = attemptLoadExisting;
            _requiresIdentifier = requiresIdentifier;
            if (File.Exists(FilePath) && attemptLoadExisting)
            {
                _dirty = false;
                Read();
            }
        }

        public void DefineSchema(string[] columnNames, string[] columnTypes)
        {
            if (columnNames.Length != columnTypes.Length)
                throw new ArgumentException("names & types must be of the same length");
            _columnNames = new CSVRowName(this, columnNames);
            _columnTypes = new CSVRowType(this, columnTypes);
            _columnTypes.Metadata = _interfaceHash;
            _dirty = true;
        }

        public void CheckSchema(string[] columnNames, string[] columnTypes)
        {
            if (columnNames.Length != columnTypes.Length)
                throw new ArgumentException("names & types must be of the same length");
            if (_columnNames == null || _columnTypes == null)
                throw new Exception($"{nameof(_columnNames)} or {nameof(_columnTypes)} doesn't exist");

            if (columnNames.Length != _columnNames.Data.Count ||
                columnTypes.Length != _columnTypes.Data.Count)
                throw new Exception($"Columns do not match expected names: {string.Join(",", columnNames)}");

            for (int i = 0; i < columnNames.Length; i++)
            {
                if (columnNames[i] != _columnNames.Data[i])
                    throw new Exception($"Columns do not match expected names: {string.Join(",", columnNames)}");
                if (columnTypes[i] != _columnTypes.Data[i])
                    throw new Exception($"Columns Types do not match expected types:{string.Join(",", columnTypes)}");
            }
        }

        public bool HasRow(string guid)
        {
            if (_guidToCSVRow.TryGetValue(guid, out var rows))
                return rows.Count > 0;
            return false;
        }

        public CSVRowData GetOrCreateRow(string guid)
        {
            if (!_guidToCSVRow.TryGetValue(guid, out List<CSVRowData> rowDatas))
            {
                rowDatas = new List<CSVRowData>();
                _guidToCSVRow[guid] = rowDatas;
            }

            if (rowDatas.Count > 1)
                throw new Exception($"More than one row with the same guid {guid}");
            if (rowDatas.Count == 1)
                return rowDatas[0];

            var rowData = new CSVRowData(this, null, guid);
            rowDatas.Add(rowData);
            _allRows.Add(rowData);
            _dirty = true;
            return rowData;
        }

        /// <summary>
        /// Create a cache to look up the hex value for the output.
        ///
        /// This is faster than calling ToString("x2") for every char for every newly generated md5 output
        /// </summary>
        private MD5 _md5;
        private char[] _hexLookUp;
        internal string ComputeHash(string value)
        {
            // get md5
            var md5 = _md5;
            bool created = false;
            if (md5 == null)
            {
                md5 = MD5.Create();
                created = true;
            }

            byte[] data = md5.ComputeHash(Encoding.UTF8.GetBytes(value));

            if (created)
                md5.Dispose();

            // optimizing converting byte array to hash string
            if (_hexLookUp == null)
            {
                _hexLookUp = new char[256 * 2];
                for (int i = 0; i < 256; i++)
                {
                    string s = i.ToString("x2");
                    _hexLookUp[2 * i] = s[0];
                    _hexLookUp[2 * i + 1] = s[1];
                }
            }
            var result = new char[data.Length * 2];
            for (int i = 0; i < data.Length; i++)
            {
                result[2 * i] = _hexLookUp[2 * data[i]];
                result[2 * i + 1] = _hexLookUp[2 * data[i] + 1];
            }

            return new string(result);
        }

        /// <summary>
        /// Caching the md5 will save the perf cost of creating it for the many uses during Read/Write
        /// </summary>
        /// <param name="cache"></param>
        private void CacheMD5(bool cache)
        {
            if (cache && _md5 == null)
            {
                _md5 = MD5.Create();
            }
            if (!cache && _md5 != null)
            {
                _md5.Dispose();
                _md5 = null;
            }
        }

        internal void RowDataChanged(CSVRowData rowData, string oldGUID = null, string newGUID = null)
        {
            _dirty = true;

            if (oldGUID != null)
                _guidToCSVRow[oldGUID].Remove(rowData);

            if (newGUID != null)
            {
                if (!_guidToCSVRow.TryGetValue(newGUID, out List<CSVRowData> rowDatas))
                {
                    rowDatas = new List<CSVRowData>();
                    _guidToCSVRow[newGUID] = rowDatas;
                }
                rowDatas.Add(rowData);
            }
        }

        public bool Write()
        {
            if (!_dirty)
                return false;

            if (_columnNames == null || _columnTypes == null)
                throw new Exception($"{nameof(DefineSchema)} must be called prior to calling {nameof(Write)}");

            _dirty = false;
            CacheMD5(true);
            using (var writer = new StreamWriter(FilePath))
            {

                var config = new CsvConfiguration(CultureInfo.InvariantCulture);
                config.ShouldQuote = args => true;
                config.TrimOptions = TrimOptions.None;
                using (var csv = new CsvWriter(writer, config))
                {
                    void WriteRow(CSVRow csvRow)
                    {
                        for (int i = 0; i < csvRow.Data.Count; i++)
                            csv.WriteField(csvRow.Data[i]);

                        csv.WriteField(csvRow.Metadata);
                        csv.WriteField(csvRow.ActualHash);
                        csv.NextRecord();
                    }

                    WriteRow(_columnNames);
                    WriteRow(_columnTypes);
                    IOrderedEnumerable<CSVRowData> orderedRows;
                    if (_requiresIdentifier)
                        orderedRows = CSVUtil.IdentifierSort(_allRows);
                    else
                        orderedRows = CSVUtil.GuidKeyPathSort(_allRows);
                    foreach (var row in orderedRows)
                        WriteRow(row);
                }
            }

            CacheMD5(false);
            return true;
        }

        private bool _dirty;
        private readonly bool _attemptLoadExisting;
        private readonly string _filePath;
        private readonly bool _requiresIdentifier;
        private string _interfaceHash;
        private CSVRowName _columnNames;
        private CSVRowType _columnTypes;
        private readonly List<CSVRowData> _allRows;
        private readonly Dictionary<string, List<CSVRowData>> _guidToCSVRow;

        private void Read()
        {
            CacheMD5(true);
            using (var reader = new StreamReader(FilePath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                csv.Read();
                csv.ReadHeader();
                var columnNames = csv.HeaderRecord;
                int colCount = columnNames.Length;
                if (colCount < 2)
                    throw new Exception($"CSV doesn't have enough columns for meta data.");

                var columnNamesWithoutMetadataAndHash = new string[colCount - 2];
                string columnNameHash = null;
                string columnNameMetadata = null;
                int identifierColIndex = -1;
                for (int i = 0; i < colCount; i++)
                {
                    if (i == colCount - 1)
                        columnNameHash = columnNames[i]; // hash
                    else if (i == colCount - 2)
                        columnNameMetadata = columnNames[i]; // metadata
                    else
                        columnNamesWithoutMetadataAndHash[i] = columnNames[i];

                    if (columnNames[i].Trim() == NamingUtil.IdentifierPropertyName)
                        identifierColIndex = i;
                }
                _columnNames = new CSVRowName(this, columnNamesWithoutMetadataAndHash, columnNameMetadata, columnNameHash);
                _dirty |= !_columnNames.HashMatches;

                if (_requiresIdentifier && identifierColIndex < 0)
                    throw new Exception($"CSV {FilePath} doesn't have a {NamingUtil.IdentifierPropertyName} column.");

                bool isParsingTypes = true;
                while (csv.Read())
                {
                    if (isParsingTypes)
                    {
                        var columnTypes = new string[colCount - 2];
                        for (int i = 0; i < colCount - 2; i++)
                            columnTypes[i] = csv.GetField(i);
                        string columnTypesHash = csv.GetField(colCount - 1);
                        string columnTypesMetadata = csv.GetField(colCount - 2);
                        _columnTypes = new CSVRowType(this, columnTypes, columnTypesMetadata, columnTypesHash);
                        _dirty |= !_columnTypes.HashMatches;
                        isParsingTypes = false;
                    }
                    else
                    {
                        string[] data = new string[colCount - 2];
                        for (int i = 0; i < colCount - 2; i++)
                            data[i] = csv.GetField(i);
                        string rowDataGUID = csv.GetField(colCount - 2);
                        string rowDataHash = csv.GetField(colCount - 1);
                        var rowData = new CSVRowData(this, data, rowDataGUID, rowDataHash);
                        _dirty |= !rowData.HashMatches;

                        if (!_guidToCSVRow.TryGetValue(rowDataGUID, out List<CSVRowData> guidRowDatas))
                        {
                            guidRowDatas = new List<CSVRowData>();
                            _guidToCSVRow[rowDataGUID] = guidRowDatas;
                        }
                        guidRowDatas.Add(rowData);
                        _allRows.Add(rowData);
                    }
                }
            }
            CacheMD5(false);
        }
    }
}
