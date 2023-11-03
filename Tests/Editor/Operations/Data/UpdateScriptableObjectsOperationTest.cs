using NUnit.Framework;
using PocketGems.Parameters.Editor.Operation;

namespace PocketGems.Parameters.Operations.Data
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
