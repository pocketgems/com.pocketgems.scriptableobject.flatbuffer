using System;
using PocketGems.Parameters.Types;

namespace PocketGems.Parameters.Validation.Attributes
{
    public class AssertStringNotEmptyAttribute : BaseValidationAttribute
    {
        private const string ErrorString = "requires a value";

        protected override bool CompatibleWithReadOnlyLists => true;

        protected override bool CheckType(Type type)
        {
            return type == typeof(string) || type == typeof(LocalizedString);
        }

        protected override string ValidateEmptyList() => null;

        protected override string ValidateElement(object element)
        {
            // null string is considered empty
            if (element == null)
                return ErrorString;

            if (_validationType == typeof(LocalizedString))
            {
                if (string.IsNullOrEmpty(((LocalizedString)element).Key))
                    return ErrorString;
                return null;
            }

            if (string.IsNullOrEmpty((string)element))
                return ErrorString;
            return null;
        }
    }
}
