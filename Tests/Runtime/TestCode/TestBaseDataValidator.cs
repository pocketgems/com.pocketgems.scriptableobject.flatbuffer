using PocketGems.Parameters.Interface;

namespace PocketGems.Parameters.Validation
{
    public class TestBaseDataValidator<T> : BaseDataValidator<T> where T : class, IBaseInfo
    {
        public const string PropertyName = "some property name";
        public const string ErrorMessage1 = "some error";
        public const string ErrorMessage2 = "some other error";
        public const string WarningMessage1 = "some warning";
        public const string WarningMessage2 = "some other warning";

        protected override void ValidateInfo(IParameterManager parameterManager, T info)
        {
            Error(PropertyName, ErrorMessage1);
            Warn(PropertyName, WarningMessage1);
        }

        protected override void ValidateParameters(IParameterManager parameterManager)
        {
            Error(ErrorMessage2);
            Warn(WarningMessage2);
        }
    }
}
