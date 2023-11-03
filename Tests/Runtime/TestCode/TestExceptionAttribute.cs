using System;
using System.Data;
using System.Reflection;
using PocketGems.Parameters.Validation.Attributes;

namespace PocketGems.Parameters.Validation
{
    public class TestExceptionAttribute : Attribute, IValidationAttribute
    {
        private readonly int _errorStage;

        public TestExceptionAttribute(int errorStage)
        {
            _errorStage = errorStage;
        }

        public bool CanValidate(PropertyInfo propertyInfo)
        {
            if (_errorStage == 0)
                throw new EvaluateException("blah");
            return true;
        }

        public void WillValidateProperty(IParameterManager parameterManager, PropertyInfo propertyInfo)
        {
            if (_errorStage == 1)
                throw new ArithmeticException("boom");
        }

        public string Validate(IParameterManager parameterManager, PropertyInfo propertyInfo, object value)
        {
            if (_errorStage == 2)
                throw new ConstraintException("boo");
            return null;
        }
    }
}
