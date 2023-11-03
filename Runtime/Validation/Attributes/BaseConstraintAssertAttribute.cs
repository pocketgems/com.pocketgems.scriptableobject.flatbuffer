using System;

namespace PocketGems.Parameters.Validation.Attributes
{
    public abstract class BaseConstraintAssertAttribute : BaseValidationAttribute
    {
        private bool _isIntConstraint;
        protected readonly long _intConstraint;
        protected readonly float _floatConstraint;

        protected BaseConstraintAssertAttribute(long constraint)
        {
            _isIntConstraint = true;
            _intConstraint = constraint;
        }

        protected BaseConstraintAssertAttribute(float constraint)
        {
            _isIntConstraint = false;
            _floatConstraint = constraint;
        }

        protected abstract string ValidateInt(long value);
        protected abstract string ValidateFloat(float value);

        protected override bool CompatibleWithReadOnlyLists => true;

        protected override bool CheckType(Type type)
        {
            if (_isIntConstraint)
                return type == typeof(short) || type == typeof(int) || type == typeof(long);
            return type == typeof(float);
        }

        protected override string ValidateEmptyList() => null;

        protected override string ValidateElement(object element)
        {
            if (_validationType == typeof(short))
                return ValidateInt((short)element);
            if (_validationType == typeof(int))
                return ValidateInt((int)element);
            if (_validationType == typeof(long))
                return ValidateInt((long)element);
            return ValidateFloat((float)element);
        }
    }
}
