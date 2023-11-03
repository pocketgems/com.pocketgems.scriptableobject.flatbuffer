using NUnit.Framework;
using PocketGems.Parameters.Operations.TestCode;
using PocketGems.Parameters.Validation;

namespace PocketGems.Parameters.Operations
{
    public class BasicMultiOperationTest : BaseBasicOperationTest<int>
    {
        private BasicMultiOperationTestSubclass _testOperation;

        [SetUp]
        public void SetUp()
        {
            _testOperation = new BasicMultiOperationTestSubclass();
            _operation = _testOperation;
        }

        protected override void CallExecute() => _testOperation.Execute(10);
        protected override void CallErrorMessage(string message) => _testOperation.CallErrorMessage(message);
        protected override void CallValidationError(ValidationError error) => _testOperation.CallValidationError(error);
        protected override void CallCancel(string cancelMessage) => _testOperation.CallCancel(cancelMessage);
        protected override void CallShortCircuit() => _testOperation.CallShortCircuit();
    }
}
