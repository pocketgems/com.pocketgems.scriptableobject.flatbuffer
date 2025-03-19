using PocketGems.Parameters.Interface;

namespace PocketGems.Parameters.Validation
{
    public class TestBaseDataValidator<T> : BaseDataValidator<T> where T : class, IBaseInfo
    {
        public const string InfoIdentifier = "some id";

        public const string PropertyName = "some property name";
        public const string ErrorMessage1 = "some error";
        public const string ErrorMessage2 = "some other error";
        public const string ErrorMessage3 = "some other other error";
        public const string WarningMessage1 = "some warning";
        public const string WarningMessage2 = "some other warning";
        public const string WarningMessage3 = "some other other warning";

        protected override void ValidateInfo(IParameterManager parameterManager, T info)
        {
            Error(PropertyName, ErrorMessage1);
            Warn(PropertyName, WarningMessage1);
        }

        protected override void ValidateParameters(IParameterManager parameterManager)
        {
            Error(ErrorMessage2);
            var info = parameterManager.Get<T>(InfoIdentifier);
            Error(info, PropertyName, ErrorMessage3);
            Error(info, ErrorMessage3);

            Warn(WarningMessage2);
            Warn(info, PropertyName, WarningMessage3);
            Warn(info, WarningMessage3);
        }
    }
}
