using PocketGems.Parameters.Interface;

namespace PocketGems.Parameters.Validation
{
    internal interface ITypedDataValidatorStruct<T> : IDataValidatorStruct where T : class, IBaseStruct
    {

    }
}
