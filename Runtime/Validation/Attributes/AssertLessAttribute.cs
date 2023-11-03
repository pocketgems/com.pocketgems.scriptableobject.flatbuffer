namespace PocketGems.Parameters.Validation.Attributes
{
    public class AssertLessAttribute : BaseConstraintAssertAttribute
    {
        public AssertLessAttribute(long constraint) : base(constraint)
        {
        }

        public AssertLessAttribute(float constraint) : base(constraint)
        {
        }

        protected override string ValidateInt(long value)
        {
            if (value < _intConstraint)
                return null;
            return $"must be less than {_intConstraint}";
        }

        protected override string ValidateFloat(float value)
        {
            if (value < _floatConstraint)
                return null;
            return $"must be less than {_floatConstraint}";
        }
    }
}
