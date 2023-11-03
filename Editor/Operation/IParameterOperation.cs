using System.Collections.Generic;

namespace PocketGems.Parameters.Editor.Operation
{
    public interface IParameterOperation<T>
    {
        /// <summary>
        /// Execute the operation.
        /// </summary>
        /// <param name="context">The context passed from one operation to another.</param>
        void Execute(T context);

        /// <summary>
        /// Current operation state.
        /// </summary>
        OperationState OperationState { get; }

        /// <summary>
        /// Errors messages if the operation ended in an error.
        /// </summary>
        List<OperationError> Errors { get; }

        /// <summary>
        /// Message if the operation resulted in a canceled state.
        /// </summary>
        string CancelMessage { get; }
    }
}
