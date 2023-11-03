using System;
using System.Reflection;
using System.Text.RegularExpressions;

namespace PocketGems.Parameters.Validation.Attributes
{
    public class AssertRegexAttribute : BaseValidationAttribute
    {
        private readonly string _regexString;
        private Regex _regex;

        public AssertRegexAttribute(string regexString)
        {
            _regexString = regexString;
        }

        protected override bool CompatibleWithReadOnlyLists => true;

        protected override bool CheckType(Type type) => type == typeof(string);

        public override void WillValidateProperty(IParameterManager parameterManager, PropertyInfo propertyInfo)
        {
            base.WillValidateProperty(parameterManager, propertyInfo);
            if (!string.IsNullOrEmpty(_regexString))
                _regex = new Regex(_regexString, RegexOptions.None);
        }

        public override string Validate(IParameterManager parameterManager, PropertyInfo propertyInfo, object value)
        {
            if (string.IsNullOrEmpty(_regexString))
                return $"{GetType()} regex isn't defined";

            return base.Validate(parameterManager, propertyInfo, value);
        }

        protected override string ValidateEmptyList() => null;

        protected override string ValidateElement(object element)
        {
            string value = (string)element ?? "";
            if (_regex.Matches(value).Count > 0)
                return null;
            return $"string doesn't match regex pattern {_regexString}";
        }
    }
}
