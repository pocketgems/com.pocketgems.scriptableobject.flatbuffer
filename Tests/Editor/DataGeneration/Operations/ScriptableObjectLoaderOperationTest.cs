using NUnit.Framework;
using PocketGems.Parameters.Common.Operation.Editor;
using PocketGems.Parameters.Common.Operations.Editor;
using PocketGems.Parameters.DataGeneration.Operation.Editor;

namespace PocketGems.Parameters.DataGeneration.Operations.Editor
{
    public class ScriptableObjectLoaderOperationTest : BaseOperationTest<IDataOperationContext>
    {
        private ScriptableObjectLoaderOperation _operation;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            _operation = new ScriptableObjectLoaderOperation();
        }

        [Test]
        public void NoOp()
        {
            _contextMock.GenerateDataType = GenerateDataType.IfNeeded;
            AssertExecute(_operation, OperationState.Finished);
        }
    }
}
