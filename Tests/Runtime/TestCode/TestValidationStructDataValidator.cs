using System;
using System.Collections.Generic;

namespace PocketGems.Parameters.Validation
{
    public class KeyValueStructDataValidator : StructCounterDataValidator<IKeyValueStruct>
    {
        public static List<ValidationError> NextInstanceErrors { get; set; }

        public KeyValueStructDataValidator() : base()
        {
            Errors = NextInstanceErrors;
            NextInstanceErrors = null;
        }
    }

    public class InnerKeyValueStructDataValidator : StructCounterDataValidator<IInnerKeyValueStruct>
    {
        public static List<ValidationError> NextInstanceErrors { get; set; }

        public InnerKeyValueStructDataValidator() : base()
        {
            Errors = NextInstanceErrors;
            NextInstanceErrors = null;
        }
    }

    public class TestExceptionStructDataValidator : StructCounterDataValidator<IExceptionStruct>
    {
        public override void ValidateStruct(IParameterManager parameterManager,
            ValidationObjectData validationObjectData)
        {
            base.ValidateStruct(parameterManager, validationObjectData);
            throw new ArgumentException("some struct exception");
        }
    }
}
