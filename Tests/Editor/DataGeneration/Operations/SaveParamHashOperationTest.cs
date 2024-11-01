using NSubstitute;
using NUnit.Framework;
using PocketGems.Parameters.Common.Operation.Editor;
using PocketGems.Parameters.Common.Operations.Editor;
using PocketGems.Parameters.Common.Util.Editor;
using PocketGems.Parameters.DataGeneration.Operation.Editor;

namespace PocketGems.Parameters.DataGeneration.Operations.Editor
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
