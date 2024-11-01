using System;
using System.IO;
using NUnit.Framework;
using PocketGems.Parameters.DataGeneration.LocalCSV.Editor;

namespace PocketGems.Parameters.DataGeneration.LocalCSV.Editor
{
    public class CSVFileTest
    {
        private const string kTestBaseFileName = "csv_file_test";
        private string kTestFileName => $"{kTestBaseFileName}.csv";
        private const string kGuid = "myGuid";
        private string[] _testHeader;
        private string[] _testType;
        private string[] _testFirstRow;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _testHeader = new[] { "Identifier", "Name", "guid1", "hash1" };
            _testType = new[] { "String", "String", "guid2", "hash2" };
            _testFirstRow = new[] { "a", "b", kGuid, "hash3" };
        }

        [SetUp]
        public void SetUp()
        {
            if (File.Exists(kTestFileName))
                File.Delete(kTestFileName);
        }

        [TearDown]
        public void TearDown()
        {
            if (File.Exists(kTestFileName))
                File.Delete(kTestFileName);
        }

        private void WriteTest()
        {
            WriteFile(_testHeader, _testType, _testFirstRow);
        }

        private void WriteFile(string[] header, string[] type, string[] firstRow)
        {
            WriteFile(header, type, new[] { firstRow });
        }

        private void WriteFile(string[] header, string[] type, string[][] rows)
        {
            var lines = new string[2 + rows.Length];
            lines[0] = string.Join(",", header);
            lines[1] = string.Join(",", type);
            for (int i = 0; i < rows.Length; i++)
                lines[2 + i] = string.Join(",", rows[i]);
            File.WriteAllLines(kTestFileName, lines);
        }

        [Test]
        public void Loading()
        {
            var csvFile = new CSVFile(kTestFileName, true, true);
            Assert.IsTrue(csvFile.AttemptLoadExisting);
            Assert.IsTrue(csvFile.RequiresIdentifier);
            Assert.AreEqual(kTestBaseFileName, csvFile.Filename);
            // dirty because the file doesn't exist
            Assert.IsTrue(csvFile.IsDirty);
            Assert.IsEmpty(csvFile.RowData);

            WriteTest();

            csvFile = new CSVFile(kTestFileName, true, true);
            // dirty because file's hashes are outdated
            Assert.IsTrue(csvFile.IsDirty);

            Assert.IsTrue(csvFile.Write());
            // not dirty after write
            Assert.IsFalse(csvFile.IsDirty);
            // attempting to write when not dirty doesn't write
            Assert.IsFalse(csvFile.Write());

            // reloading csv which has correct hashes results in non-dirty
            csvFile = new CSVFile(kTestFileName, true, true);
            Assert.IsFalse(csvFile.IsDirty);

            // instructed to create a new file so it's dirty
            csvFile = new CSVFile(kTestFileName, false, true);
            Assert.IsTrue(csvFile.IsDirty);
            Assert.IsFalse(csvFile.AttemptLoadExisting);
            Assert.IsTrue(csvFile.RequiresIdentifier);
        }

        [Test]
        public void LoadingErrors()
        {
            // not enough columns
            WriteFile(new[] { "Identifier" }, new[] { "string" }, new[] { "" });
            Assert.Throws<Exception>(() =>
            {
                var _ = new CSVFile(kTestFileName, true, true);
            });
        }

        [Test]
        public void MissingIdentifierColumn()
        {
            // not enough columns
            WriteFile(new[] { "Identifier" }, new[] { "string" }, new[] { "" });
            Assert.Throws<Exception>(() =>
            {
                var _ = new CSVFile(kTestFileName, true, true);
            });

            // missing Identifier column
            WriteFile(
                new[] { "Blah", "Meta1", "Meta2" },
                new[] { "string", "string", "string" },
                new[] { "", "", "" });
            Assert.Throws<Exception>(() =>
            {
                var _ = new CSVFile(kTestFileName, true, true);
            });

            var csvFile = new CSVFile(kTestFileName, true, false);
            Assert.IsTrue(csvFile.AttemptLoadExisting);
            Assert.IsFalse(csvFile.RequiresIdentifier);
        }

        [Test]
        public void ComputeHash()
        {
            var csvFile = new CSVFile(kTestFileName, false, true);
            Assert.AreEqual("900150983cd24fb0d6963f7d28e17f72", csvFile.ComputeHash("abc"));
        }

        [Test]
        public void InterfaceHash()
        {
            // initially interface hash is null
            var csvFile = new CSVFile(kTestFileName, false, true);
            Assert.AreEqual("", csvFile.InterfaceHash);
            Assert.IsTrue(csvFile.IsDirty);
            csvFile.DefineSchema(new[] { "Identifier" }, new[] { "string" });
            csvFile.Write();
            // reload file and check interface hash
            csvFile = new CSVFile(kTestFileName, true, true);
            Assert.AreEqual("", csvFile.InterfaceHash);
            Assert.IsFalse(csvFile.IsDirty);

            var interfaceHash = "myhash";

            // set hash
            WriteTest();
            csvFile = new CSVFile(kTestFileName, true, true);
            Assert.IsTrue(csvFile.IsDirty);
            Assert.AreEqual("guid2", csvFile.InterfaceHash);
            csvFile.InterfaceHash = interfaceHash;
            Assert.AreEqual(interfaceHash, csvFile.InterfaceHash);
            Assert.IsTrue(csvFile.Write());
            Assert.IsFalse(csvFile.IsDirty);

            // reload file and check interface hash
            csvFile = new CSVFile(kTestFileName, true, true);
            Assert.IsFalse(csvFile.IsDirty);
            Assert.AreEqual(interfaceHash, csvFile.InterfaceHash);
        }

        [Test]
        [TestCase(false)]
        [TestCase(true)]
        public void CreateNew(bool requireIdentifier)
        {
            string firstColumnName = requireIdentifier ? "Identifier" : "Name";

            var csvFile = new CSVFile(kTestFileName, true, requireIdentifier);
            Assert.IsTrue(csvFile.IsDirty);
            Assert.Throws<ArgumentException>(() =>
            {
                // mismatched arguments
                csvFile.DefineSchema(
                    new[] { firstColumnName, "Amount" },
                    new[] { "string" });
            });
            csvFile.DefineSchema(
                new[] { firstColumnName, "Amount" },
                new[] { "string", "int" });
            var guid = "blah";
            Assert.IsFalse(csvFile.HasRow(guid));
            var row = csvFile.GetOrCreateRow(guid);
            Assert.IsTrue(csvFile.HasRow(guid));
            row.UpdateData(new[] { "my_id", "5" });
            Assert.True(csvFile.Write());

            var lines = File.ReadAllLines(kTestFileName);
            Assert.True(lines[0].StartsWith($"\"{firstColumnName}\",\"Amount\""));
            Assert.True(lines[1].StartsWith("\"string\",\"int\""));
            Assert.True(lines[2].StartsWith("\"my_id\",\"5\",\"blah\""));
        }

        [Test]
        public void WriteFail()
        {
            var csvFile = new CSVFile(kTestFileName, false, true);
            Assert.IsTrue(csvFile.IsDirty);
            // forgetting to call DefineSchema
            Assert.Throws<Exception>(() =>
            {
                csvFile.Write();
            });
        }

        [Test]
        public void CheckSchema()
        {
            var csvFile = new CSVFile(kTestFileName, true, true);
            Assert.IsTrue(csvFile.ColumnSchemaModified);
            // no pre-existing columns
            Assert.Throws<Exception>(() =>
            {
                csvFile.CheckSchema(
                    new[] { "Identifier", "Name" },
                    new[] { "String", "String" });
            });

            WriteTest();
            csvFile = new CSVFile(kTestFileName, true, true);
            // WriteTest() didn't write hashes correctly
            Assert.IsTrue(csvFile.ColumnSchemaModified);
            // valid
            csvFile.CheckSchema(
                new[] { "Identifier", "Name" },
                new[] { "String", "String" });
            Assert.Throws<ArgumentException>(() =>
            {
                // number of elements do not match
                csvFile.CheckSchema(
                    new[] { "Identifier", "Name" },
                    new[] { "String" });
            });
            Assert.Throws<Exception>(() =>
            {
                // schema column count doesn't match
                csvFile.CheckSchema(
                    new[] { "Identifier", "Name", "Name2" },
                    new[] { "String", "String", "String" });
            });
            Assert.Throws<Exception>(() =>
            {
                // column names do not match
                csvFile.CheckSchema(
                    new[] { "Identifier2", "Name2" },
                    new[] { "String", "String" });
            });
            Assert.Throws<Exception>(() =>
            {
                // column types do not match
                csvFile.CheckSchema(
                    new[] { "Identifier", "Name" },
                    new[] { "String2", "String2" });
            });

            // after write & reload, schema hashes were updated
            csvFile.Write();
            csvFile = new CSVFile(kTestFileName, true, true);
            Assert.IsFalse(csvFile.ColumnSchemaModified);
        }

        [Test]
        public void CreatingRows()
        {
            WriteTest();
            var csvFile = new CSVFile(kTestFileName, true, true);
            Assert.IsTrue(csvFile.IsDirty);
            csvFile.Write();
            Assert.IsFalse(csvFile.IsDirty);
            Assert.AreEqual(1, csvFile.RowData.Count);

            const string id = "test_id";
            const string guid = "test_guid";
            var data = new[] { id, "blah" };

            // create new row
            Assert.IsFalse(csvFile.HasRow(guid));
            var newRow = csvFile.GetOrCreateRow(guid);
            Assert.IsTrue(csvFile.HasRow(guid));
            Assert.IsNull(newRow.Identifier);
            Assert.IsNull(newRow.Data);
            Assert.AreEqual(guid, newRow.GUID);
            Assert.IsTrue(csvFile.IsDirty);
            Assert.AreEqual(2, csvFile.RowData.Count);

            // querying row
            var queryRow = csvFile.GetOrCreateRow(guid);
            Assert.IsTrue(csvFile.HasRow(guid));
            Assert.AreEqual(newRow, queryRow);
            queryRow.UpdateData(data);
            Assert.AreEqual(id, queryRow.Identifier);
            Assert.AreEqual(data, queryRow.Data);
            Assert.AreEqual(2, csvFile.RowData.Count);

            // write to disk
            csvFile.Write();

            // reload csv file
            csvFile = new CSVFile(kTestFileName, true, true);
            var loadedRow = csvFile.GetOrCreateRow(guid);
            Assert.IsTrue(csvFile.HasRow(guid));
            Assert.AreEqual(guid, loadedRow.GUID);
            Assert.AreEqual(id, loadedRow.Identifier);
            Assert.IsFalse(csvFile.IsDirty);
            Assert.AreEqual(2, csvFile.RowData.Count);
        }

        [Test]
        public void CreatingRowError()
        {
            var guid = "abc";
            WriteFile(
                new[] { "Identifier", "Guid", "Meta" },
                new[] { "string", "string", "string" },
                new[] {
                    new[]{"id1", guid, "" },
                    new[] { "id2", guid, "" }
                });
            var csvFile = new CSVFile(kTestFileName, true, true);
            Assert.Throws<Exception>(() =>
            {
                var _ = csvFile.GetOrCreateRow(guid);
            });
        }

        [Test]
        public void UpdatingRowGuid()
        {
            WriteTest();
            var csvFile = new CSVFile(kTestFileName, true, true);
            Assert.IsTrue(csvFile.IsDirty);
            csvFile.Write();
            Assert.IsFalse(csvFile.IsDirty);

            // get existing row
            Assert.IsTrue(csvFile.HasRow(kGuid));
            var newRow = csvFile.GetOrCreateRow(kGuid);
            Assert.IsTrue(csvFile.HasRow(kGuid));
            Assert.IsFalse(csvFile.IsDirty);
            newRow.GUID = kGuid;
            Assert.IsFalse(csvFile.IsDirty);

            var newGuid = "blah";
            newRow.GUID = newGuid;
            Assert.IsTrue(csvFile.IsDirty);

            Assert.IsFalse(csvFile.HasRow(kGuid));
            Assert.IsTrue(csvFile.HasRow(newGuid));
            var anotherRow = csvFile.GetOrCreateRow(newGuid);
            Assert.IsFalse(csvFile.HasRow(kGuid));
            Assert.IsTrue(csvFile.HasRow(newGuid));
            Assert.AreEqual(newRow, anotherRow);
        }
    }
}
