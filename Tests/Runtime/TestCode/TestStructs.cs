using System.Collections.Generic;
using PocketGems.Parameters;
using PocketGems.Parameters.Interface;
using PocketGems.Parameters.Validation;
using PocketGems.Parameters.Validation.Attributes;

public interface IMissingValidator1Struct : IBaseStruct
{
}

public interface IMissingValidator2Struct : IBaseStruct
{
}

public interface IKeyValueStruct : IBaseStruct
{
    [AssertStringNotEmpty]
    string Description { get; }

    [AssertGreater(0)]
    int Value { get; }

    ParameterStructReference<IInnerKeyValueStruct> InnerStruct { get; }
    IReadOnlyList<ParameterStructReference<IInnerKeyValueStruct>> InnerStructs { get; }
}

public interface IInnerKeyValueStruct : IBaseStruct
{
    [AssertStringNotEmpty]
    string Description { get; }

    [AssertGreater(0)]
    int Value { get; }
}

public interface IExceptionStruct : IBaseStruct
{
    [TestException(0)]
    int SomeValue1 { get; }

    [TestException(1)]
    int SomeValue2 { get; }

    [TestException(2)]
    int SomeValue3 { get; }
}
