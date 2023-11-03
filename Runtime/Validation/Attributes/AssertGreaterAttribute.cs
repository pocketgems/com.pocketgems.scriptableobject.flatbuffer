namespace PocketGems.Parameters.Validation.Attributes
{
    public class AssertGreaterAttribute : BaseConstraintAssertAttribute
    {
        public AssertGreaterAttribute(long constraint) : base(constraint)
        {
        }

        public AssertGreaterAttribute(float constraint) : base(constraint)
        {
        }

        protected override string ValidateInt(long value)
        {
            if (value > _intConstraint)
                return null;
            return $"must be greater than {_intConstraint}";
        }

        protected override string ValidateFloat(float value)
        {
            if (value > _floatConstraint)
                return null;
            return $"must be greater than {_floatConstraint}";
        }
    }
}
