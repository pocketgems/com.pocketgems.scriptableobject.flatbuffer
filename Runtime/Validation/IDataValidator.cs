using System.Collections.Generic;
using PocketGems.Parameters.Interface;

namespace PocketGems.Parameters.Validation
{
    internal interface IDataValidator
    {
        IReadOnlyList<ValidationError> Errors { get; }

        void ValidateInfo(IParameterManager parameterManager, IBaseInfo info);

        void ValidateParameters(IParameterManager parameterManager);
    }
}
