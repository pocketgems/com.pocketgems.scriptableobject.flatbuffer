namespace PocketGems.Parameters.Validation.Attributes
{
    public class AssertGreaterOrEqualAttribute : BaseConstraintAssertAttribute
    {
        public AssertGreaterOrEqualAttribute(long constraint) : base(constraint)
        {
        }

        public AssertGreaterOrEqualAttribute(float constraint) : base(constraint)
        {
        }

        protected override string ValidateInt(long value)
        {
            if (value >= _intConstraint)
                return null;
            return $"must be greater than or equal to {_intConstraint}";
        }

        protected override string ValidateFloat(float value)
        {
            if (value >= _floatConstraint)
                return null;
            return $"must be greater than or equal to {_floatConstraint}";
        }
    }
}
