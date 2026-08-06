using System.Collections.Generic;
using PocketGems.Parameters.Interface;

namespace PocketGems.Parameters.Validation
{
    /// <summary>
    /// Abstract class that developers can implement to validate parameter interface of type T
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class BaseDataValidatorStruct<T> : ITypedDataValidatorStruct<T> where T : class, IBaseStruct
    {
        /// <summary>
        /// Errors that can be populated after running the various Validate methods.
        ///
        /// Can be null or empty if there are no errors.
        /// </summary>
        public IReadOnlyList<ValidationError> Errors => _errors;

        /// <summary>
        /// Abstract function for developers to validate the T struct.  Called once for each T struct.
        /// </summary>
        /// <param name="parameterManager">parameter manager to query if needed during validation</param>
        /// <param name="structObj">the struct to validate</param>
        protected abstract void ValidateStruct(IParameterManager parameterManager, T structObj);

        /// <summary>
        /// Log an error during validation.
        /// </summary>
        /// <param name="propertyName">property name with the error</param>
        /// <param name="message">user facing message about the error</param>
        protected void Error(string propertyName, string message)
        {
            AddError(propertyName, message, ValidationError.Severity.Error);
        }

        /// <summary>
        /// General error during validation.
        /// </summary>
        /// <param name="message">user facing message about the error</param>
        protected void Error(string message)
        {
            AddError(message, ValidationError.Severity.Error);
        }

        /// <summary>
        /// Log a warning during validation.
        /// </summary>
        /// <param name="propertyName">property name with the warning</param>
        /// <param name="message">user facing message about the warning</param>
        protected void Warn(string propertyName, string message)
        {
            AddError(propertyName, message, ValidationError.Severity.Warning);
        }

        /// <summary>
        /// General warning during validation.
        /// </summary>
        /// <param name="message">user facing message about the warning</param>
        protected void Warn(string message)
        {
            AddError(message, ValidationError.Severity.Warning);
        }

        public void ValidateStruct(IParameterManager parameterManager, ValidationObjectData validationObjectData)
        {
            _currentValidationObjectData = validationObjectData;
            ValidateStruct(parameterManager, (T)validationObjectData.Struct);
        }

        private void AddError(string message, ValidationError.Severity severity)
        {
            AddError(null, message, severity);
        }

        private void AddError(string propertyName, string message, ValidationError.Severity severity)
        {
            var error = new ValidationError(
                _currentValidationObjectData.InfoType,
                _currentValidationObjectData.Info.Identifier,
                _currentValidationObjectData.StructParentInfoReferenceProperty,
                message,
                _currentValidationObjectData.StructKeyPath,
                propertyName,
                severity:severity);
            _errors.Add(error);
        }

        private ValidationObjectData _currentValidationObjectData;
        private readonly List<ValidationError> _errors = new List<ValidationError>();
    }
}
