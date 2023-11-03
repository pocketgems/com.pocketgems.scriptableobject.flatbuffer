using PocketGems.Parameters.Validation;

namespace PocketGems.Parameters.Editor.Operation
{
    /// <summary>
    /// Error object for operation errors.
    /// </summary>
    public class OperationError
    {
        public enum ErrorType
        {
            General,
            Validation
        }

        /// <summary>
        /// The type of error
        /// </summary>
        public ErrorType Type { get; private set; }

        /// <summary>
        /// Message about the error if the error type is General
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// The validation error if the error type is Validation
        /// </summary>
        public ValidationError ValidationError { get; private set; }

        /// <summary>
        /// Construct a General error
        /// </summary>
        /// <param name="message">error message</param>
        public OperationError(string message)
        {
            Type = ErrorType.General;
            Message = message;
        }

        /// <summary>
        /// Construct a Validation error
        /// </summary>
        /// <param name="validationError">the validation error</param>
        public OperationError(ValidationError validationError)
        {
            Type = ErrorType.Validation;
            ValidationError = validationError;
        }
    }
}
