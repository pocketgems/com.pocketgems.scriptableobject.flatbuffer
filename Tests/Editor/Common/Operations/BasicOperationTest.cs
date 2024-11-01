using NUnit.Framework;
using PocketGems.Parameters.Validation;

namespace PocketGems.Parameters.Common.Operations.Editor
{
    public class BasicOperationTest : BaseBasicOperationTest<string>
    {
        private BasicOperationTestSubclass _testOperation;

        [SetUp]
        public void SetUp()
        {
            _testOperation = new BasicOperationTestSubclass();
            _operation = _testOperation;
        }

        protected override void CallExecute() => _testOperation.Execute("execute");
        protected override void CallErrorMessage(string message) => _testOperation.CallErrorMessage(message);
        protected override void CallValidationError(ValidationError error) => _testOperation.CallValidationError(error);
        protected override void CallCancel(string cancelMessage) => _testOperation.CallCancel(cancelMessage);
        protected override void CallShortCircuit() => _testOperation.CallShortCircuit();
    }
}
