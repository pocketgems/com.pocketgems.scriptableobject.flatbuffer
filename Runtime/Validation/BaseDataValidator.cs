using System.Collections.Generic;
using PocketGems.Parameters.Interface;

namespace PocketGems.Parameters.Validation
{
    /// <summary>
    /// Abstract class that developers can implement to validate parameter interface of type T
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class BaseDataValidator<T> : ITypedDataValidator<T> where T : class, IBaseInfo
    {
        /// <summary>
        /// Errors that can be populated after running the various Validate methods.
        ///
        /// Can be null or empty if there are no errors.
        /// </summary>
        public IReadOnlyList<ValidationError> Errors => _errors;

        /// <summary>
        /// Abstract function for developers to validate the T info.  Called once for each T info.
        /// </summary>
        /// <param name="parameterManager">parameter manager to query if needed during validation</param>
        /// <param name="info">the info to validate</param>
        protected abstract void ValidateInfo(IParameterManager parameterManager, T info);

        /// <summary>
        /// Abstract function for developers to write holistic checks in the parameterManager in relation to the T type.
        /// </summary>
        /// <param name="parameterManager">parameter manager to query if needed during validation</param>
        protected abstract void ValidateParameters(IParameterManager parameterManager);

        /// <summary>
        /// Log an error during validation.
        /// </summary>
        /// <param name="propertyName">property name with the error</param>
        /// <param name="message">user facing message about the error</param>
        protected void Error(string propertyName, string message)
        {
            var error = new ValidationError(typeof(T), _currentIdentifier, propertyName, message);
            _errors.Add(error);
        }

        /// <summary>
        /// General error during validation.
        /// </summary>
        /// <param name="message">user facing message about the error</param>
        protected void Error(string message)
        {
            var error = new ValidationError(typeof(T), _currentIdentifier, null, message);
            _errors.Add(error);
        }

        /// <summary>
        /// Log a warning during validation.
        /// </summary>
        /// <param name="propertyName">property name with the warning</param>
        /// <param name="message">user facing message about the warning</param>
        protected void Warn(string propertyName, string message)
        {
            var error = new ValidationError(typeof(T), _currentIdentifier, propertyName, message,
                severity: ValidationError.Severity.Warning);
            _errors.Add(error);
        }

        /// <summary>
        /// General warning during validation.
        /// </summary>
        /// <param name="message">user facing message about the warning</param>
        protected void Warn(string message)
        {
            var error = new ValidationError(typeof(T), _currentIdentifier, null, message,
                severity: ValidationError.Severity.Warning);
            _errors.Add(error);
        }

        void IDataValidator.ValidateInfo(IParameterManager parameterManager, IBaseInfo info)
        {
            _currentIdentifier = info.Identifier;
            ValidateInfo(parameterManager, (T)info);
            _currentIdentifier = null;
        }

        void IDataValidator.ValidateParameters(IParameterManager parameterManager)
        {
            ValidateParameters(parameterManager);
        }

        private string _currentIdentifier;
        private readonly List<ValidationError> _errors = new List<ValidationError>();
    }
}
