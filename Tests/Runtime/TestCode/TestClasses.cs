
using System.Collections.Generic;
using PocketGems.Parameters;
using PocketGems.Parameters.Interface;

public class MockMutableParameter : IMutableParameter
{
    public int EditPropertyCalls = 0;
    public int RemoveAllEditCalls = 0;
    public string EditPropertyPropertyName;
    public string EditPropertyValue;
    public string ReturnEditPropertyError;

    public bool EditProperty(string propertyName, string value, out string error)
    {
        EditPropertyCalls++;
        EditPropertyPropertyName = propertyName;
        EditPropertyValue = value;
        error = ReturnEditPropertyError;
        return string.IsNullOrWhiteSpace(ReturnEditPropertyError);
    }

    public void RemoveAllEdits() => RemoveAllEditCalls++;
}

public class MockMutableBaseInfo : MockMutableParameter, IBaseInfo
{
    public string Identifier { get; set; }
}

public class MockMutableBaseStruct : MockMutableParameter, IBaseStruct { }
public class MockMySpecialInfo : MockMutableBaseInfo, IMySpecialInfo { }
public class MockMyVerySpecialInfo : MockMutableBaseInfo, IMyVerySpecialInfo
{
    public ParameterStructReference<IMissingValidator1Struct> Struct { get; set; }
    public IReadOnlyList<ParameterStructReference<IMissingValidator2Struct>> Structs { get; set; }
}

public class MockMyVerySpecial1Struct : MockMutableBaseStruct, IMissingValidator1Struct { }
public class MockMyVerySpecial2Struct : MockMutableBaseStruct, IMissingValidator2Struct { }

public class MockKeyValueStruct : MockMutableBaseStruct, IKeyValueStruct
{
    public string Description { get; set; }
    public int Value { get; set; }
    public ParameterStructReference<IInnerKeyValueStruct> InnerStruct { get; set; }
    public IReadOnlyList<ParameterStructReference<IInnerKeyValueStruct>> InnerStructs => _innerStructs;
    public ParameterStructReferenceRuntime<IInnerKeyValueStruct>[] _innerStructs;

    public MockKeyValueStruct(string description, int value, string innerStruct, string[] innerStructs)
    {
        Description = description;
        Value = value;
        InnerStruct = new ParameterStructReferenceRuntime<IInnerKeyValueStruct>(innerStruct);
        var innerStructsArray = new ParameterStructReferenceRuntime<IInnerKeyValueStruct>[innerStructs.Length];
        for (int i = 0; i < innerStructs.Length; i++)
            innerStructsArray[i] = new ParameterStructReferenceRuntime<IInnerKeyValueStruct>(innerStructs[i]);
        _innerStructs = innerStructsArray;
    }
}

public class MockInnerKeyValueStruct : MockMutableBaseStruct, IInnerKeyValueStruct
{
    public string Description { get; set; }
    public int Value { get; set; }

    public MockInnerKeyValueStruct(string description, int value)
    {
        Description = description;
        Value = value;
    }
}

public class MockTestValidationInfo : IMutableParameter, ITestValidationInfo
{
    public string Identifier { get; set; }
    public string DisplayName { get; set; }
    public string Description { get; set; }
    public ParameterReference<ITestValidationInfo> Ref { get; set; }
    public ParameterStructReference<IKeyValueStruct> StructRef { get; set; }
    public IReadOnlyList<ParameterStructReference<IKeyValueStruct>> StructRefs => _structRefs;
    public ParameterStructReference<IKeyValueStruct>[] _structRefs;

    public bool EditProperty(string propertyName, string value, out string error) => throw new System.NotImplementedException();
    public void RemoveAllEdits() => throw new System.NotImplementedException();
}

public class MockSubBadValidationInfo : IMutableParameter, ITestSubInterfaceInfo
{
    public string Identifier { get; set; }
    public int Value { get; set; }
    public int SubValue { get; set; }

    public bool EditProperty(string propertyName, string value, out string error) =>
        throw new System.NotImplementedException();

    public void RemoveAllEdits() => throw new System.NotImplementedException();
}
