using NUnit.Framework;
using PocketGems.Parameters.Editor.Operation;
using PocketGems.Parameters.Validation;

namespace PocketGems.Parameters.Operations
{
    public abstract class BaseBasicOperationTest<T>
    {
        protected IParameterOperation<T> _operation;

        protected abstract void CallExecute();
        protected abstract void CallErrorMessage(string message);
        protected abstract void CallValidationError(ValidationError error);
        protected abstract void CallCancel(string cancelMessage);
        protected abstract void CallShortCircuit();

        [Test]
        public void Init()
        {
            Assert.AreEqual(OperationState.Ready, _operation.OperationState);
            Assert.AreEqual(0, _operation.Errors.Count);
            Assert.IsNull(_operation.CancelMessage);
        }

        [Test]
        public void Execute()
        {
            CallExecute();
            Assert.AreEqual(OperationState.Finished, _operation.OperationState);
            Assert.AreEqual(0, _operation.Errors.Count);
            Assert.IsNull(_operation.CancelMessage);
        }

        [Test]
        public void Error()
        {
            var errorMessage = "blah";
            var validationError = new ValidationError(null, null, null, null);

            CallExecute();
            CallErrorMessage(errorMessage);
            CallValidationError(validationError);
            Assert.AreEqual(OperationState.Error, _operation.OperationState);
            Assert.AreEqual(2, _operation.Errors.Count);
            Assert.AreEqual(OperationError.ErrorType.General, _operation.Errors[0].Type);
            Assert.AreEqual(errorMessage, _operation.Errors[0].Message);
            Assert.AreEqual(OperationError.ErrorType.Validation, _operation.Errors[1].Type);
            Assert.AreEqual(validationError, _operation.Errors[1].ValidationError);
            Assert.IsNull(_operation.CancelMessage);
        }

        [Test]
        public void ShortCircuit()
        {
            CallExecute();
            CallShortCircuit();
            Assert.AreEqual(OperationState.ShortCircuit, _operation.OperationState);
            Assert.IsNull(_operation.CancelMessage);
            Assert.AreEqual(0, _operation.Errors.Count);
        }

        [Test]
        public void Cancel()
        {
            var cancelMessage = "canceled";

            CallExecute();
            CallCancel(cancelMessage);
            Assert.AreEqual(OperationState.Canceled, _operation.OperationState);
            Assert.AreEqual(cancelMessage, _operation.CancelMessage);
            Assert.AreEqual(0, _operation.Errors.Count);
        }
    }
}
