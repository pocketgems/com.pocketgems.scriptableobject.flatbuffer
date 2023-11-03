using System;
using System.Collections;
using System.Linq;
using System.Reflection;

namespace PocketGems.Parameters.Validation.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class AssertListNotEmptyAttribute : Attribute, IValidationAttribute
    {
        private string ErrorString => "collection must have size greater than 0";

        public bool CanValidate(PropertyInfo propertyInfo) =>
            typeof(IEnumerable).IsAssignableFrom(propertyInfo.PropertyType);

        public void WillValidateProperty(IParameterManager parameterManager, PropertyInfo propertyInfo) { }

        public string Validate(IParameterManager parameterManager, PropertyInfo propertyInfo, object value)
        {
            // null array is considered empty
            if (value == null)
                return ErrorString;

            // iterate and check
            var enumerable = (IEnumerable)value;
            if (!enumerable.Cast<object>().Any())
                return ErrorString;

            return null;
        }
    }
}
