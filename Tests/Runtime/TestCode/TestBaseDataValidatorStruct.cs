using PocketGems.Parameters.Interface;

namespace PocketGems.Parameters.Validation
{
    public class TestBaseDataValidatorStruct<T> : BaseDataValidatorStruct<T> where T : class, IBaseStruct
    {
        public const string StructPropertyName = "some struct property name";
        public const string ErrorMessage1 = "some message";
        public const string ErrorMessage2 = "some other message";
        public const string WarningMessage1 = "some warning";
        public const string WarningMessage2 = "some other warning";

        protected override void ValidateStruct(IParameterManager parameterManager, T structObj)
        {
            Error(StructPropertyName, ErrorMessage1);
            Error(ErrorMessage2);

            Warn(StructPropertyName, WarningMessage1);
            Warn(WarningMessage2);
        }
    }
}
