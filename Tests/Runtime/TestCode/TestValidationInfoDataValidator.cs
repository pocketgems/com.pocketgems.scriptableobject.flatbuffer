using System;
using System.Collections.Generic;
using PocketGems.Parameters.Interface;

namespace PocketGems.Parameters.Validation
{
    public class MySpecialInfoDataValidator : InfoCounterDataValidator<IMySpecialInfo>
    {
        public static List<ValidationError> NextInstanceErrors { get; set; }
        public MySpecialInfoDataValidator()
        {
            Errors = NextInstanceErrors;
            NextInstanceErrors = null;
        }
    }

    public class TestValidationInfoDataValidator : InfoCounterDataValidator<ITestValidationInfo>
    {
        public static List<ValidationError> NextInstanceErrors { get; set; }
        public TestValidationInfoDataValidator()
        {
            Errors = NextInstanceErrors;
            NextInstanceErrors = null;
        }
    }

    public class TestBaseValidationInfoDataValidator : InfoCounterDataValidator<ITestBaseValidationInfo>
    {
        public static List<ValidationError> NextInstanceErrors { get; set; }

        public TestBaseValidationInfoDataValidator()
        {
            Errors = NextInstanceErrors;
            NextInstanceErrors = null;
        }
    }

    public class BadValidationInfoDataValidator : InfoCounterDataValidator<IBadValidationInfo>
    {
        public static List<ValidationError> NextInstanceErrors { get; set; }

        public BadValidationInfoDataValidator() : base()
        {
            Errors = NextInstanceErrors;
            NextInstanceErrors = null;
        }
    }

    public class TestSuperInterfaceInfoDataValidator : InfoCounterDataValidator<ITestSuperInterfaceInfo>
    {
        public static List<ValidationError> NextInstanceErrors { get; set; }

        public TestSuperInterfaceInfoDataValidator()
        {
            Errors = NextInstanceErrors;
            NextInstanceErrors = null;
        }
    }

    public class TestSubInterfaceInfoDataValidator : InfoCounterDataValidator<ITestSubInterfaceInfo>
    {
        public static List<ValidationError> NextInstanceErrors { get; set; }

        public TestSubInterfaceInfoDataValidator()
        {
            Errors = NextInstanceErrors;
            NextInstanceErrors = null;
        }
    }

    public class TestExceptionInfoDataValidator : InfoCounterDataValidator<ITestExceptionInfo>
    {
        public override void ValidateInfo(IParameterManager parameterManager, IBaseInfo info)
        {
            base.ValidateInfo(parameterManager, info);
            throw new Exception("some exception");
        }

        public override void ValidateParameters(IParameterManager parameterManager)
        {
            base.ValidateParameters(parameterManager);
            throw new ArgumentException("some other exception");
        }
    }
}
