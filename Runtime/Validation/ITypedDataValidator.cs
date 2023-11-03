using PocketGems.Parameters.Interface;

namespace PocketGems.Parameters.Validation
{
    internal interface ITypedDataValidator<T> : IDataValidator where T : class, IBaseInfo
    {

    }
}
