using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using NSubstitute;
using NUnit.Framework;
using Parameters;
using PocketGems.Parameters.Common.Operation.Editor;
using PocketGems.Parameters.DataGeneration.Operation.Editor;
using PocketGems.Parameters.Editor;
using UnityEngine;
using UnityEngine.TestTools;

namespace PocketGems.Parameters.DataGeneration.Operations.Editor
{
    public class ReadLocalCSVOperationTest : BaseDataOperationTest
    {
        private ReadLocalCSVOperation _operation;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            _contextMock.GeneratedLocalCSVDirectory.ReturnsForAnyArgs(kTestCSVDir);
            _contextMock.ModifiedCSVPaths = new List<string>();
            _contextMock.ModifiedCSVPaths.Add(Path.Combine(kTestCSVDir, $"{_mockParameterInfo1.BaseName}.csv"));
            _contextMock.ModifiedCSVPaths.Add(Path.Combine(kTestCSVDir, $"{_mockParameterStruct1.BaseName}.csv"));
            foreach (var i in _mockParameterInfos)
                CreateCSV(i);
            foreach (var i in _mockParameterStructs)
                CreateCSV(i);
            Assert.AreEqual(_mockParameterInfos.Count + _mockParameterStructs.Count, Directory.GetFiles(kTestCSVDir).Length);

            _operation = new ReadLocalCSVOperation();
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(kTestCSVDir))
                Directory.Delete(kTestCSVDir, true);
        }

        [Test]
        public void NoReadAll()
        {
            _contextMock.GenerateDataType = GenerateDataType.All;
            AssertExecute(_operation, OperationState.Finished);
            Assert.AreEqual(0, CSVBridge.CheckSchemaCalls);
            Assert.AreEqual(GenerateDataType.All, _contextMock.GenerateDataType);
        }

        [Test]
        public void NoReadNotCSVDiff()
        {
            _contextMock.GenerateDataType = GenerateDataType.None;
            AssertExecute(_operation, OperationState.Finished);
            Assert.AreEqual(0, CSVBridge.CheckSchemaCalls);
            Assert.AreEqual(GenerateDataType.None, _contextMock.GenerateDataType);
        }

        [Test]
        public void NoReadDirectory()
        {
            Directory.Delete(kTestCSVDir, true);

            _contextMock.GenerateDataType = GenerateDataType.CSVDiff;
            AssertExecute(_operation, OperationState.Finished);
            Assert.AreEqual(0, CSVBridge.CheckSchemaCalls);
            Assert.AreEqual(GenerateDataType.All, _contextMock.GenerateDataType);
        }

        [Test]
        public void SuccessfulRead()
        {
            _contextMock.GenerateDataType = GenerateDataType.CSVDiff;
            AssertExecute(_operation, OperationState.Finished);

            _infoCSVFileCacheMock.Received(1).Load(_mockParameterInfo1);
            _structCSVFileCacheMock.Received(1).Load(_mockParameterStruct1);

            _infoCSVFileCacheMock.ReceivedWithAnyArgs(1).LoadedFiles();
            _structCSVFileCacheMock.ReceivedWithAnyArgs(1).LoadedFiles();

            Assert.AreEqual(1, CSVBridge.CheckSchemaCalls);
            Assert.AreEqual(GenerateDataType.CSVDiff, _contextMock.GenerateDataType);
        }

        [Test]
        public void ModificationOfNonExistingInterface()
        {
            _contextMock.ModifiedCSVPaths.Add(Path.Combine(kTestCSVDir, "BlanInfo.csv"));
            _contextMock.GenerateDataType = GenerateDataType.CSVDiff;

            LogAssert.Expect(LogType.Error, new Regex(".*"));
            AssertExecute(_operation, OperationState.Finished);
            Assert.AreEqual(0, CSVBridge.CheckSchemaCalls);
            Assert.AreEqual(GenerateDataType.All, _contextMock.GenerateDataType);
        }

        [Test]
        public void MissingInfoCSV()
        {
            File.Delete(Path.Combine(kTestCSVDir, $"{_mockParameterInfo1.BaseName}.csv"));

            _contextMock.GenerateDataType = GenerateDataType.CSVDiff;
            AssertExecute(_operation, OperationState.Finished);
            Assert.AreEqual(0, CSVBridge.CheckSchemaCalls);
            Assert.AreEqual(GenerateDataType.All, _contextMock.GenerateDataType);
        }

        [Test]
        public void MissingStructCSV()
        {
            File.Delete(Path.Combine(kTestCSVDir, $"{_mockParameterStruct1.BaseName}.csv"));

            _contextMock.GenerateDataType = GenerateDataType.CSVDiff;
            AssertExecute(_operation, OperationState.Finished);
            Assert.AreEqual(0, CSVBridge.CheckSchemaCalls);
            Assert.AreEqual(GenerateDataType.All, _contextMock.GenerateDataType);
        }

        [Test]
        public void SchemaChanged()
        {
            CSVBridge.NextErrors.Add("error");

            _contextMock.GenerateDataType = GenerateDataType.CSVDiff;
            LogAssert.Expect(LogType.Error, new Regex(".*"));
            AssertExecute(_operation, OperationState.Finished);

            // verify load attempted
            _infoCSVFileCacheMock.ReceivedWithAnyArgs(1).Load(default);
            _structCSVFileCacheMock.ReceivedWithAnyArgs(1).Load(default);

            Assert.AreEqual(1, CSVBridge.CheckSchemaCalls);
            Assert.AreEqual(GenerateDataType.All, _contextMock.GenerateDataType);
        }

        [Test]
        public void DeletingExtraFiles()
        {
            int expectedFileCount = _mockParameterInfos.Count + _mockParameterStructs.Count;

            // create extra random csv
            Assert.AreEqual(expectedFileCount, Directory.GetFiles(kTestCSVDir).Length);
            CreateCSV(MockedInterfaces.ParameterInfo("BlahInfo"));
            expectedFileCount++;
            Assert.AreEqual(expectedFileCount, Directory.GetFiles(kTestCSVDir).Length);

            _contextMock.GenerateDataType = GenerateDataType.CSVDiff;
            AssertExecute(_operation, OperationState.Finished);
            expectedFileCount--;

            Assert.AreEqual(1, CSVBridge.CheckSchemaCalls);
            Assert.AreEqual(GenerateDataType.CSVDiff, _contextMock.GenerateDataType);
            Assert.AreEqual(expectedFileCount, Directory.GetFiles(kTestCSVDir).Length);
        }

        [Test]
        public void ColumnSchemaModified()
        {
            // make hash mismatch in csvs
            var infoCSVFile = CreateCSV(_mockParameterInfo1);
            var path = infoCSVFile.FilePath;
            var text = File.ReadAllText(path);
            text = text.Replace("Metadata", "abc");
            File.WriteAllText(path, text);

            var structCSVFile = CreateCSV(_mockParameterStruct1);
            path = structCSVFile.FilePath;
            text = File.ReadAllText(path);
            text = text.Replace("Metadata", "abc");
            File.WriteAllText(path, text);

            // mock LoadedFile returns
            _loadedInfoCSVFileCacheFiles[_mockParameterInfo1.BaseName] = infoCSVFile;
            _loadedStructCSVFileCacheFiles[_mockParameterStruct1.BaseName] = structCSVFile;

            _contextMock.GenerateDataType = GenerateDataType.CSVDiff;
            LogAssert.Expect(LogType.Error, new Regex(".*"));
            LogAssert.Expect(LogType.Error, new Regex(".*"));
            AssertExecute(_operation, OperationState.Finished);

            // verify load attempted
            _infoCSVFileCacheMock.ReceivedWithAnyArgs(1).Load(default);
            _structCSVFileCacheMock.ReceivedWithAnyArgs(1).Load(default);

            _infoCSVFileCacheMock.ReceivedWithAnyArgs(1).LoadedFiles();
            _structCSVFileCacheMock.ReceivedWithAnyArgs(1).LoadedFiles();

            Assert.AreEqual(2, _contextMock.ModifiedCSVPaths.Count);
            Assert.AreEqual(1, CSVBridge.CheckSchemaCalls);
            Assert.AreEqual(GenerateDataType.All, _contextMock.GenerateDataType);
        }
    }
}
