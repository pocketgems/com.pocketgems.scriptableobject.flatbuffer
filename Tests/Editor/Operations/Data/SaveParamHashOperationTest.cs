using NSubstitute;
using NUnit.Framework;
using PocketGems.Parameters.Editor.Operation;
using PocketGems.Parameters.Util;

namespace PocketGems.Parameters.Operations.Data
{
    public class SaveParamHashOperationTest : BaseOperationTest<IDataOperationContext>
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

            var operation = new SaveParamHashOperation();

            AssertExecute(operation, OperationState.Finished);
            Assert.IsNull(interfaceHashMock.AssemblyInfoHash);
            Assert.IsNull(interfaceHashMock.AssemblyInfoEditorHash);
            Assert.AreEqual(newHash, interfaceHashMock.GeneratedDataHash);
        }
    }
}
