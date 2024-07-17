using NSubstitute;
using NUnit.Framework;
using PocketGems.Parameters.CodeGeneration.Operation.Editor;
using PocketGems.Parameters.Common.Operation.Editor;
using PocketGems.Parameters.Common.Operations.Editor;
using PocketGems.Parameters.Common.Util.Editor;

namespace PocketGems.Parameters.CodeGeneration.Operations.Editor
{
    public class CheckGenerateCodeTypeOperationTest : BaseOperationTest<ICodeOperationContext>
    {
        private CheckGenerateCodeTypeOperation _operation;
        private IInterfaceHash _interfaceHashMock;
        private string kAssemblyHash = "some hash";

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _interfaceHashMock = Substitute.For<IInterfaceHash>();
            _interfaceHashMock.AssemblyInfoHash = kAssemblyHash;
            _interfaceHashMock.AssemblyInfoEditorHash = kAssemblyHash;
            _interfaceHashMock.GeneratedDataHash = kAssemblyHash;
            _contextMock.InterfaceAssemblyHash = kAssemblyHash;
            _contextMock.InterfaceHash.ReturnsForAnyArgs(_interfaceHashMock);

            _operation = new CheckGenerateCodeTypeOperation();
        }

        [Test]
        public void Generate()
        {
            _contextMock.GenerateCodeType = GenerateCodeType.Generate;
            AssertExecute(_operation, OperationState.Finished);
            Assert.AreEqual(GenerateCodeType.Generate, _contextMock.GenerateCodeType);
        }

        [Test]
        public void HashMismatch()
        {
            _contextMock.GenerateCodeType = GenerateCodeType.IfNeeded;
            _interfaceHashMock.AssemblyInfoHash = "blah";
            AssertExecute(_operation, OperationState.Finished);
            Assert.AreEqual(GenerateCodeType.Generate, _contextMock.GenerateCodeType);
        }

        [Test]
        public void EditorHashMismatch()
        {
            _contextMock.GenerateCodeType = GenerateCodeType.IfNeeded;
            _interfaceHashMock.AssemblyInfoEditorHash = "blah";
            AssertExecute(_operation, OperationState.Finished);
            Assert.AreEqual(GenerateCodeType.Generate, _contextMock.GenerateCodeType);
        }

        [Test]
        public void IfNeeded()
        {
            _contextMock.GenerateCodeType = GenerateCodeType.IfNeeded;
            AssertExecute(_operation, OperationState.ShortCircuit);
            Assert.AreEqual(GenerateCodeType.IfNeeded, _contextMock.GenerateCodeType);
        }
    }
}
