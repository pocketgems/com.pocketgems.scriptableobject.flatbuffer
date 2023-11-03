using System.Collections.Generic;
using PocketGems.Parameters.Interface;

namespace PocketGems.Parameters.Validation
{
    public abstract class BaseCounterDataValidator<T>
    {
        public static BaseCounterDataValidator<T> Instance { get; set; }

        public IReadOnlyList<ValidationError> Errors { get; protected set; }

        public virtual void ValidateInfo(IParameterManager parameterManager, IBaseInfo info)
        {
            ValidateInfoCalls++;
        }

        public virtual void ValidateParameters(IParameterManager parameterManager)
        {
            ValidateParametersCalls++;
        }

        public virtual void ValidateStruct(IParameterManager parameterManager,
            ValidationObjectData validationObjectData)
        {
            ValidateStructCalls++;
        }

        public int ValidateInfoCalls { get; private set; }
        public int ValidateParametersCalls { get; private set; }
        public int ValidateStructCalls { get; private set; }

        protected BaseCounterDataValidator()
        {
            Instance = this;
        }
    }

    public abstract class InfoCounterDataValidator<T> : BaseCounterDataValidator<T>, ITypedDataValidator<T> where T : class, IBaseInfo
    {
    }

    public abstract class StructCounterDataValidator<T> : BaseCounterDataValidator<T>, ITypedDataValidatorStruct<T> where T : class, IBaseStruct
    {
    }
}
