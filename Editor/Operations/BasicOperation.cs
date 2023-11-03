using System.Collections.Generic;
using PocketGems.Parameters.Editor.Operation;
using PocketGems.Parameters.Validation;

namespace PocketGems.Parameters.Operations
{
    public abstract class BasicOperation<T> : IParameterOperation<T>
    {
        protected BasicOperation()
        {
            Errors = new List<OperationError>();
            OperationState = OperationState.Ready;
        }

        public virtual void Execute(T context) => Execute();

        protected void Execute()
        {
            OperationState = OperationState.Finished;
        }

        public OperationState OperationState { get; private set; }
        public List<OperationError> Errors { get; private set; }
        public string CancelMessage { get; private set; }

        protected void ShortCircuit()
        {
            OperationState = OperationState.ShortCircuit;
        }

        protected void Cancel(string message)
        {
            CancelMessage = message;
            OperationState = OperationState.Canceled;
        }

        protected void Error(string message)
        {
            Errors.Add(new OperationError(message));
            OperationState = OperationState.Error;
        }

        protected void Error(ValidationError validationError)
        {
            Errors.Add(new OperationError(validationError));
            OperationState = OperationState.Error;
        }
    }
}
