using System.IO;
using NSubstitute;
using NUnit.Framework;
using PocketGems.Parameters.Common.Operation.Editor;
using PocketGems.Parameters.Common.Operations.Editor;
using PocketGems.Parameters.Common.Util.Editor;
using PocketGems.Parameters.DataGeneration.Operation.Editor;

namespace PocketGems.Parameters.DataGeneration.Operations.Editor
{
    public class CheckGenerateDataTypeOperationTest : BaseOperationTest<IDataOperationContext>
    {
        private CheckGenerateDataTypeOperation _operation;
        private IInterfaceHash _interfaceHashMock;
        private string kAssemblyHash = "some hash";
        private string kDirectoryName = "TempUnitTestDir";
        private string kAssetFileName = "TempAssetName";

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            TearDown();

            // create files
            Directory.CreateDirectory(kDirectoryName);
            File.WriteAllText(Path.Combine(kDirectoryName, kAssetFileName), "test");

            // mock interface hash
            _interfaceHashMock = Substitute.For<IInterfaceHash>();
            _interfaceHashMock.AssemblyInfoHash = kAssemblyHash;
            _interfaceHashMock.AssemblyInfoEditorHash = kAssemblyHash;
            _interfaceHashMock.GeneratedDataHash = kAssemblyHash;
            _interfaceHashMock.GeneratedDataLoaderHash.Returns(kAssemblyHash);

            // mock context
            _contextMock.InterfaceAssemblyHash = kAssemblyHash;
            _contextMock.InterfaceHash.ReturnsForAnyArgs(_interfaceHashMock);
            _contextMock.GeneratedAssetDirectory.ReturnsForAnyArgs(kDirectoryName);
            _contextMock.GeneratedAssetFileName.ReturnsForAnyArgs(kAssetFileName);

            _operation = new CheckGenerateDataTypeOperation();
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(kDirectoryName))
                Directory.Delete(kDirectoryName, true);
        }

        [Test]
        public void All()
        {
            _contextMock.GenerateDataType = GenerateDataType.All;
            AssertExecute(_operation, OperationState.Finished);
            Assert.AreEqual(GenerateDataType.All, _contextMock.GenerateDataType);
        }

        [Test]
        public void IfNeeded()
        {
            _contextMock.GenerateDataType = GenerateDataType.IfNeeded;
            AssertExecute(_operation, OperationState.ShortCircuit);
            Assert.AreEqual(GenerateDataType.IfNeeded, _contextMock.GenerateDataType);
        }


        [Test]
        public void MissingFolder()
        {
            Directory.Delete(kDirectoryName, true);

            _contextMock.GenerateDataType = GenerateDataType.IfNeeded;
            _interfaceHashMock.AssemblyInfoHash = kAssemblyHash;
            _interfaceHashMock.AssemblyInfoEditorHash = kAssemblyHash;
            AssertExecute(_operation, OperationState.Finished);
            Assert.AreEqual(GenerateDataType.All, _contextMock.GenerateDataType);
        }

        [Test]
        public void MissingFile()
        {
            File.Delete(Path.Combine(kDirectoryName, kAssetFileName));

            _contextMock.GenerateDataType = GenerateDataType.IfNeeded;
            _interfaceHashMock.AssemblyInfoHash = kAssemblyHash;
            _interfaceHashMock.AssemblyInfoEditorHash = kAssemblyHash;
            AssertExecute(_operation, OperationState.Finished);
            Assert.AreEqual(GenerateDataType.All, _contextMock.GenerateDataType);
        }

        [Test]
        public void DataHashMismatch()
        {
            _contextMock.GenerateDataType = GenerateDataType.IfNeeded;
            _interfaceHashMock.GeneratedDataHash = "blah";
            AssertExecute(_operation, OperationState.Finished);
            Assert.AreEqual(GenerateDataType.All, _contextMock.GenerateDataType);
        }

        [Test]
        public void GeneratedCodeMismatch_AssemblyInfoHash()
        {
            _contextMock.GenerateDataType = GenerateDataType.IfNeeded;
            _interfaceHashMock.AssemblyInfoHash = "blah";
            AssertExecute(_operation, OperationState.Error);
        }

        [Test]
        public void GeneratedCodeMismatch_AssemblyInfoEditorHash()
        {
            _contextMock.GenerateDataType = GenerateDataType.IfNeeded;
            _interfaceHashMock.AssemblyInfoEditorHash = "blah";
            AssertExecute(_operation, OperationState.Error);
        }

        [Test]
        public void GeneratedCodeMismatch_GeneratedDataLoaderHash()
        {
            _contextMock.GenerateDataType = GenerateDataType.IfNeeded;
            _interfaceHashMock.GeneratedDataLoaderHash.Returns("blah");
            AssertExecute(_operation, OperationState.Error);
        }
    }
}
