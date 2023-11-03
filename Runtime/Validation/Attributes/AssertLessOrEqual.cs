namespace PocketGems.Parameters.Validation.Attributes
{
    public class AssertLessOrEqual : BaseConstraintAssertAttribute
    {
        public AssertLessOrEqual(long constraint) : base(constraint)
        {
        }

        public AssertLessOrEqual(float constraint) : base(constraint)
        {
        }

        protected override string ValidateInt(long value)
        {
            if (value <= _intConstraint)
                return null;
            return $"must be less than or equal to {_intConstraint}";
        }

        protected override string ValidateFloat(float value)
        {
            if (value <= _floatConstraint)
                return null;
            return $"must be less than or equal to {_floatConstraint}";
        }
    }
}
