using NSubstitute;
using NUnit.Framework;
using PocketGems.Parameters.Editor.Operation;
using PocketGems.Parameters.Util;

namespace PocketGems.Parameters.Operations.Code
{
    public class SaveAssemblyHashOperationTest : BaseOperationTest<ICodeOperationContext>
    {
        [Test]
        public void Execute()
        {
            var newHash = "some hash";
            var interfaceHashMock = Substitute.For<IInterfaceHash>();
            interfaceHashMock.AssemblyInfoHash = null;
            interfaceHashMock.AssemblyInfoEditorHash = null;
            interfaceHashMock.GeneratedDataHash = null;
            _contextMock.InterfaceAssemblyHash.ReturnsForAnyArgs(newHash);
            _contextMock.InterfaceHash.ReturnsForAnyArgs(interfaceHashMock);

            var operation = new SaveAssemblyHashOperation();

            AssertExecute(operation, OperationState.Finished);
            Assert.AreEqual(newHash, interfaceHashMock.AssemblyInfoHash);
            Assert.AreEqual(newHash, interfaceHashMock.AssemblyInfoEditorHash);
            Assert.IsNull(interfaceHashMock.GeneratedDataHash);
        }
    }
}
