using PocketGems.Parameters.Validation;

namespace PocketGems.Parameters.Common.Operations.Editor
{
    public class BasicOperationTestSubclass : BasicOperation<string>
    {
        public void CallErrorMessage(string message) => Error(message);
        public void CallValidationError(ValidationError error) => Error(error);
        public void CallCancel(string message) => Cancel(message);
        public void CallShortCircuit() => ShortCircuit();
    }
}
