using PocketGems.Parameters.Interface;

namespace PocketGems.Parameters.Validation
{
    public class TestBaseDataValidatorStruct<T> : BaseDataValidatorStruct<T> where T : class, IBaseStruct
    {
        public const string StructPropertyName = "some struct property name";
        public const string ErrorMessage1 = "some message";
        public const string ErrorMessage2 = "some other message";

        protected override void ValidateStruct(IParameterManager parameterManager, T structObj)
        {
            Error(StructPropertyName, ErrorMessage1);
            Error(ErrorMessage2);
        }
    }
}
