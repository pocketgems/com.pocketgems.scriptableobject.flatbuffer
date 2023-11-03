using PocketGems.Parameters.Interface;

namespace PocketGems.Parameters.Validation
{
    public class TestBaseDataValidator<T> : BaseDataValidator<T> where T : class, IBaseInfo
    {
        public const string PropertyName = "some property name";
        public const string ErrorMessage1 = "some message";
        public const string ErrorMessage2 = "some other message";

        protected override void ValidateInfo(IParameterManager parameterManager, T info)
        {
            Error(PropertyName, ErrorMessage1);
        }

        protected override void ValidateParameters(IParameterManager parameterManager)
        {
            Error(ErrorMessage2);
        }
    }
}
