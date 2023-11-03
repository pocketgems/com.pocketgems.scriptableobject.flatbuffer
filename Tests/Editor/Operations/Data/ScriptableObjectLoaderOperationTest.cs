using NUnit.Framework;
using PocketGems.Parameters.Editor.Operation;

namespace PocketGems.Parameters.Operations.Data
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
