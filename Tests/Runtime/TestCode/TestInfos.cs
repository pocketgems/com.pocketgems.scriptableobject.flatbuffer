using System.Collections.Generic;
using PocketGems.Parameters;
using PocketGems.Parameters.Interface;
using PocketGems.Parameters.Validation;
using PocketGems.Parameters.Validation.Attributes;

public interface IMySpecialInfo : IBaseInfo
{
}

public interface IMyVerySpecialInfo : IMySpecialInfo
{
    ParameterStructReference<IMissingValidator1Struct> Struct { get; }
    IReadOnlyList<ParameterStructReference<IMissingValidator2Struct>> Structs { get; }
}

public interface ITestBaseValidationInfo : IMySpecialInfo
{
    [AssertStringNotEmpty]
    string DisplayName { get; }
}

public interface ITestValidationInfo : ITestBaseValidationInfo
{
    [AssertStringNotEmpty]
    string Description { get; }

    ParameterReference<ITestValidationInfo> Ref { get; }

    ParameterStructReference<IKeyValueStruct> StructRef { get; }
    IReadOnlyList<ParameterStructReference<IKeyValueStruct>> StructRefs { get; }
}

public interface IBadValidationInfo : IBaseInfo
{
    // not compatible
    [AssertStringNotEmpty]
    int Description { get; }
}

public interface ITestSuperInterfaceInfo : IBaseInfo
{
    [AssertGreater(0)]
    int Value { get; }
}

public interface ITestSubInterfaceInfo : ITestSuperInterfaceInfo
{
    [AssertGreater(0)]
    int SubValue { get; }
}

public interface ITestExceptionInfo : IBaseInfo
{
    [TestException(0)]
    int SomeValue1 { get; }

    [TestException(1)]
    int SomeValue2 { get; }

    [TestException(2)]
    int SomeValue3 { get; }

    ParameterStructReference<IExceptionStruct> ExceptionStruct { get; }
}
