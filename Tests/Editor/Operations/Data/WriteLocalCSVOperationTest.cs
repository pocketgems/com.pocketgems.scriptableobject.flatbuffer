using System.Collections.Generic;
using System.IO;
using NSubstitute;
using NUnit.Framework;
using Parameters;
using PocketGems.Parameters.Editor.LocalCSV;
using PocketGems.Parameters.Editor.Operation;
using PocketGems.Parameters.Models;

namespace PocketGems.Parameters.Operations.Data
{
    public class WriteLocalCSVOperationTest : BaseDataOperationTest
    {
        private WriteLocalCSVOperation _operation;

        private Dictionary<IParameterInfo, List<IScriptableObjectMetadata>> _scriptableObjectMetadatas;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            TearDown();

            _operation = new WriteLocalCSVOperation();

            // setup mock
            _contextMock.GeneratedLocalCSVDirectory.ReturnsForAnyArgs(kTestCSVDir);

            _scriptableObjectMetadatas = new Dictionary<IParameterInfo, List<IScriptableObjectMetadata>>();
            foreach (var i in _mockParameterInfos)
                _scriptableObjectMetadatas[i] = new List<IScriptableObjectMetadata>();
            _contextMock.ScriptableObjectMetadatas.ReturnsForAnyArgs(_scriptableObjectMetadatas);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(kTestCSVDir))
                Directory.Delete(kTestCSVDir, true);
        }

        [Test]
        public void NoOp()
        {
            _contextMock.GenerateDataType = GenerateDataType.IfNeeded;
            AssertExecute(_operation, OperationState.Finished);
        }

        [Test]
        public void All()
        {
            // mock LoadedFile returns and mark dirty
            void CreateAndMarkDirty(Dictionary<string, CSVFile> filesDict, IParameterInterface parameterInterface)
            {
                var csvFile = CreateCSV(parameterInterface);
                filesDict[parameterInterface.BaseName] = csvFile;
                // create new row to make dirty
                csvFile.GetOrCreateRow("blah").UpdateData(new[] { "some id" });
            }

            CreateAndMarkDirty(_loadedInfoCSVFileCacheFiles, _mockParameterInfo1);
            CreateAndMarkDirty(_loadedInfoCSVFileCacheFiles, _mockParameterInfo2);
            CreateAndMarkDirty(_loadedInfoCSVFileCacheFiles, _mockParameterInfo3);
            CreateAndMarkDirty(_loadedStructCSVFileCacheFiles, _mockParameterStruct1);
            CreateAndMarkDirty(_loadedStructCSVFileCacheFiles, _mockParameterStruct2);

            _contextMock.GenerateDataType = GenerateDataType.All;
            AssertExecute(_operation, OperationState.Finished);
            Assert.IsTrue(Directory.Exists(kTestCSVDir));

            // clears all loaded CSVs to read them in again
            _infoCSVFileCacheMock.Received(1).ClearCache();
            _structCSVFileCacheMock.Received(1).ClearCache();

            // loading individual structs in case they weren't loaded through infos
            _structCSVFileCacheMock.Received(1).Load(_mockParameterStruct1);
            _structCSVFileCacheMock.Received(1).Load(_mockParameterStruct2);

            Assert.AreEqual(_mockParameterInfos.Count, CSVBridge.UpdateFromScriptableObjectsCalls);
            Assert.AreEqual(_mockParameterInfos.Count + _mockParameterStructs.Count, CSVBridge.DefineSchemaCalls);
            Assert.AreEqual(_mockParameterInfos.Count + _mockParameterStructs.Count, Directory.GetFiles(kTestCSVDir).Length);
        }

        [Test]
        public void Error()
        {
            CSVBridge.NextErrors.Add("some error");

            _contextMock.GenerateDataType = GenerateDataType.All;
            AssertExecute(_operation, OperationState.Error);

            Assert.IsTrue(Directory.Exists(kTestCSVDir));
            Assert.AreEqual(_mockParameterInfos.Count, CSVBridge.UpdateFromScriptableObjectsCalls);
        }

        [Test]
        public void ScriptableObjectDiff()
        {
            _scriptableObjectMetadatas.Clear();
            _scriptableObjectMetadatas[_mockParameterInfo1] = new List<IScriptableObjectMetadata>();

            // mock create csv that would've been called through UpdateFromScriptableObjectsCalls
            _loadedInfoCSVFileCacheFiles[_mockParameterInfo1.BaseName] = CreateCSV(_mockParameterInfo1);

            _contextMock.GenerateDataType = GenerateDataType.ScriptableObjectDiff;
            AssertExecute(_operation, OperationState.Finished);

            Assert.IsTrue(Directory.Exists(kTestCSVDir));
            Assert.AreEqual(1, CSVBridge.UpdateFromScriptableObjectsCalls);
            Assert.AreEqual(1, Directory.GetFiles(kTestCSVDir).Length);
        }

        [Test]
        public void CSVDiff()
        {
            // Call All() to populate the context CSVFiles
            All();

            // make csvs dirty
            _loadedInfoCSVFileCacheFiles[_mockParameterInfo1.BaseName].GetOrCreateRow("blam").
                UpdateData(new[] { "new id" });
            _loadedStructCSVFileCacheFiles[_mockParameterStruct1.BaseName].GetOrCreateRow("blam")
                .UpdateData(new[] { "new id" });

            // populate metadatas that would've been populated in a previous step
            _scriptableObjectMetadatas.Clear();
            _scriptableObjectMetadatas[_mockParameterInfo1] = new List<IScriptableObjectMetadata>();

            CSVBridge.Reset();
            _contextMock.GenerateDataType = GenerateDataType.CSVDiff;
            AssertExecute(_operation, OperationState.Finished);

            Assert.IsTrue(Directory.Exists(kTestCSVDir));
            Assert.AreEqual(1, CSVBridge.UpdateFromScriptableObjectsCalls);
            Assert.AreEqual(2, CSVBridge.DefineSchemaCalls);
            Assert.AreEqual(_mockParameterInfos.Count + _mockParameterStructs.Count,
                Directory.GetFiles(kTestCSVDir).Length);
        }
    }
}
