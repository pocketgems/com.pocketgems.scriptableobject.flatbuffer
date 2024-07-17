using System.Collections.Generic;
using System.IO;
using NSubstitute;
using NUnit.Framework;
using PocketGems.Parameters.CodeGeneration.Operation.Editor;
using PocketGems.Parameters.Common.Operation.Editor;
using PocketGems.Parameters.Common.Operations.Editor;

namespace PocketGems.Parameters.Operations
{
    public class ParseInterfaceAssemblyOperationTest : BaseOperationTest<ICodeOperationContext>
    {
        private ParseInterfaceAssemblyOperation<ICodeOperationContext> _operation;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _operation = new();
        }

        [Test]
        public void NoInterfaceFolder_CodeOperationPath()
        {
            var contextMock = Substitute.For<ICodeOperationContext>();
            string badFolderPath = "blah";
            contextMock.InterfaceDirectoryRootPath.ReturnsForAnyArgs(badFolderPath);
            _operation.Execute(contextMock);
            Assert.AreEqual(OperationState.ShortCircuit, _operation.OperationState);
        }
    }
}
