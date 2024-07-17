using NUnit.Framework;
using PocketGems.Parameters.Common.Operation.Editor;
using PocketGems.Parameters.Common.Operations.Editor;
using PocketGems.Parameters.DataGeneration.Operation.Editor;

namespace PocketGems.Parameters.DataGeneration.Operations.Editor
{
    public class UpdateScriptableObjectsOperationTest : BaseOperationTest<IDataOperationContext>
    {
        private UpdateScriptableObjectsOperation _operation;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            _operation = new UpdateScriptableObjectsOperation();
        }

        [Test]
        public void NoOp()
        {
            _contextMock.GenerateDataType = GenerateDataType.All;
            AssertExecute(_operation, OperationState.Finished);
        }
    }
}
