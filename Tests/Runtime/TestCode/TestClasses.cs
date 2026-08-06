
using System.Collections.Generic;
using PocketGems.Parameters;
using PocketGems.Parameters.Interface;

public abstract class MockMutableParameter : IMutableParameter
{
    public int EditPropertyCalls;
    public int RevertEditPropertyCalls;
    public int RemoveAllEditCalls;

    public string EditPropertyPropertyName;
    public string EditPropertyValue;
    public string ReturnEditPropertyError;

    public string RevertEditPropertyPropertyName;
    public string ReturnRevertEditPropertyError;

    public bool EditProperty(IParameterManager parameterManager, string propertyName, string value, out string error)
    {
        EditPropertyCalls++;
        EditPropertyPropertyName = propertyName;
        EditPropertyValue = value;
        error = ReturnEditPropertyError;
        return string.IsNullOrWhiteSpace(ReturnEditPropertyError);
    }

    public bool RevertEditedProperty(string propertyName, out string error)
    {
        RevertEditPropertyCalls++;
        RevertEditPropertyPropertyName = propertyName;
        error = ReturnRevertEditPropertyError;
        return string.IsNullOrWhiteSpace(ReturnRevertEditPropertyError);
    }

    public void RemoveAllEdits() => RemoveAllEditCalls++;

    public abstract IMutableParameter CreateLinkedMutableParameter(IParameterManager parameterManager);
}

public class MockMutableBaseInfo : MockMutableParameter, IBaseInfo
{
    public string Identifier { get; private set; }

    public MockMutableBaseInfo(string identifier) => Identifier = identifier;

    public override IMutableParameter CreateLinkedMutableParameter(IParameterManager parameterManager) => new MockMutableBaseInfo(Identifier);
}

public abstract class MockMutableBaseStruct : MockMutableParameter, IBaseStruct
{
}
public class MockSubclassAInfo : MockMutableBaseInfo, ISubInterfaceAInfo
{
    public MockSubclassAInfo(string identifier) : base(identifier) { }
    public override IMutableParameter CreateLinkedMutableParameter(IParameterManager parameterManager) => new MockSubclassAInfo(Identifier);
}
public class MockErrorSubclassAInfo : MockMutableBaseInfo, ISubInterfaceAInfo
{
    public MockErrorSubclassAInfo(string identifier) : base(identifier)
    {
        ReturnEditPropertyError = "my error";
    }
    public override IMutableParameter CreateLinkedMutableParameter(IParameterManager parameterManager) => new MockErrorSubclassAInfo(Identifier);
}
public class MockSubclassBInfo : MockMutableBaseInfo, ISubInterfaceBInfo
{
    public MockSubclassBInfo(string identifier) : base(identifier) { }
    public ParameterStructReference<IMissingValidator1Struct> Struct { get; set; }
    public IReadOnlyList<ParameterStructReference<IMissingValidator2Struct>> Structs { get; set; }
    public override IMutableParameter CreateLinkedMutableParameter(IParameterManager parameterManager) => new MockSubclassBInfo(Identifier);
}

public class MockMyVerySpecial1Struct : MockMutableBaseStruct, IMissingValidator1Struct
{
    public override IMutableParameter CreateLinkedMutableParameter(IParameterManager parameterManager) => throw new System.NotImplementedException();
}
public class MockMyVerySpecial2Struct : MockMutableBaseStruct, IMissingValidator2Struct
{
    public override IMutableParameter CreateLinkedMutableParameter(IParameterManager parameterManager) => throw new System.NotImplementedException();
}

public class MockKeyValueStruct : MockMutableBaseStruct, IKeyValueStruct
{
    public string Description { get; set; }
    public int Value { get; set; }

    public ParameterStructReference<IInnerKeyValueStruct> InnerStruct => _innerStruct;
    public ParameterStructReference<IInnerKeyValueStruct> _innerStruct;

    public IReadOnlyList<ParameterStructReference<IInnerKeyValueStruct>> InnerStructs => _innerStructs;
    public ParameterStructReferenceRuntime<IInnerKeyValueStruct>[] _innerStructs;

    private readonly string _innerStructGuid;
    private readonly string[] _innerStructGuids;

    public MockKeyValueStruct(IParameterManager parameterManager, string description, int value, string innerStruct, string[] innerStructs)
    {
        Description = description;
        Value = value;
        _innerStructGuid = innerStruct;
        _innerStructGuids = innerStructs;

        _innerStruct = new ParameterStructReferenceRuntime<IInnerKeyValueStruct>(parameterManager, _innerStructGuid);
        _innerStructs = new ParameterStructReferenceRuntime<IInnerKeyValueStruct>[_innerStructGuids.Length];
        for (int i = 0; i < _innerStructGuids.Length; i++)
            _innerStructs[i] = new ParameterStructReferenceRuntime<IInnerKeyValueStruct>(parameterManager, _innerStructGuids[i]);
    }

    public override IMutableParameter CreateLinkedMutableParameter(IParameterManager parameterManager) => new MockKeyValueStruct(parameterManager, Description, Value, _innerStructGuid, _innerStructGuids);
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

    public override IMutableParameter CreateLinkedMutableParameter(IParameterManager parameterManager) => throw new System.NotImplementedException();
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

    public bool EditProperty(IParameterManager parameterManager, string propertyName, string value, out string error) =>
        throw new System.NotImplementedException();
    public bool RevertEditedProperty(string propertyName, out string error) =>
        throw new System.NotImplementedException();
    public void RemoveAllEdits() => throw new System.NotImplementedException();
    public IMutableParameter CreateLinkedMutableParameter(IParameterManager parameterManager) => throw new System.NotImplementedException();
}

public class MockSubBadValidationInfo : IMutableParameter, ITestSubInterfaceInfo
{
    public string Identifier { get; set; }
    public int Value { get; set; }
    public int SubValue { get; set; }

    public bool EditProperty(IParameterManager parameterManager, string propertyName, string value, out string error) =>
        throw new System.NotImplementedException();
    public bool RevertEditedProperty(string propertyName, out string error) => throw new System.NotImplementedException();
    public void RemoveAllEdits() => throw new System.NotImplementedException();
    public IMutableParameter CreateLinkedMutableParameter(IParameterManager parameterManager) => throw new System.NotImplementedException();
}
