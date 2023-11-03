using System.Collections.Generic;

namespace PocketGems.Parameters.Validation
{
    internal interface IDataValidatorStruct
    {
        IReadOnlyList<ValidationError> Errors { get; }
        void ValidateStruct(IParameterManager parameterManager, ValidationObjectData validationObjectData);
    }
}
