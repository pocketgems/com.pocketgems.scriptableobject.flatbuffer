
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
    public Dictionary<string, string> EditedValues = new();

    public string RevertEditPropertyPropertyName;
    public string ReturnRevertEditPropertyError;

    public virtual bool EditProperty(IParameterManager parameterManager, string propertyName, string value, out string error)
    {
        EditPropertyCalls++;
        EditPropertyPropertyName = propertyName;
        EditPropertyValue = value;
        error = ReturnEditPropertyError;
        if (!string.IsNullOrWhiteSpace(ReturnEditPropertyError))
            return false;

        // like a generated class, which fails on a property that already has an edit
        if (EditedValues.ContainsKey(propertyName))
        {
            error = "Overrides is already set.";
            return false;
        }
        EditedValues[propertyName] = value;
        return true;
    }

    public bool RevertEditedProperty(string propertyName, out string error)
    {
        RevertEditPropertyCalls++;
        RevertEditPropertyPropertyName = propertyName;
        error = ReturnRevertEditPropertyError;
        if (!string.IsNullOrWhiteSpace(ReturnRevertEditPropertyError))
            return false;

        // like a generated class, which fails on a property that has no edit
        if (!EditedValues.Remove(propertyName))
        {
            error = "No overrides to remove.";
            return false;
        }
        return true;
    }

    public void RemoveAllEdits()
    {
        RemoveAllEditCalls++;
        EditedValues.Clear();
    }

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

    public override IMutableParameter CreateLinkedMutableParameter(IParameterManager parameterManager) => new MockInnerKeyValueStruct(Description, Value);
}

/// <summary>
/// Stands in for the generated factory: creates the mock rows below and hands them an edit error to return.
/// </summary>
public class FakeParameterFactory : IParameterFactory
{
    public int CreateCalls;
    public string EditError;

    public ICreatedParameter Create(IParameterManager parameterManager, string interfaceName, string identifierOrKeyPath)
    {
        switch (interfaceName)
        {
            case nameof(ISubInterfaceAInfo):
                CreateCalls++;
                return new FakeCreatedParameter(new MockAddedSubclassAInfo(identifierOrKeyPath, EditError), true, identifierOrKeyPath,
                    new[] { nameof(ISubInterfaceAInfo) }, new[] { "Name" });
            case nameof(ISubInterfaceBInfo):
                CreateCalls++;
                return new FakeCreatedParameter(new MockAddedSubclassBInfo(identifierOrKeyPath, EditError), true, identifierOrKeyPath,
                    new[] { nameof(ISubInterfaceAInfo), nameof(ISubInterfaceBInfo) }, new[] { "Name" });
            case nameof(IKeyValueStruct):
                CreateCalls++;
                return new FakeCreatedParameter(new MockAddedKeyValueStruct(parameterManager), false, identifierOrKeyPath,
                    new[] { nameof(IKeyValueStruct) }, new[] { "Description", "Value", "InnerStruct" });
            case nameof(IInnerKeyValueStruct):
                CreateCalls++;
                return new FakeCreatedParameter(new MockAddedInnerKeyValueStruct(), false, identifierOrKeyPath,
                    new[] { nameof(IInnerKeyValueStruct) }, new[] { "Description", "Value" });
            default:
                return null;
        }
    }
}

/// <summary>
/// Stands in for the CreatedParameter class generated next to the ParameterDataLoader.
/// </summary>
public class FakeCreatedParameter : ICreatedParameter
{
    public FakeCreatedParameter(IMutableParameter parameter, bool isInfo, string identifierOrKeyPath,
        IReadOnlyList<string> interfaceNames, IReadOnlyList<string> columns)
    {
        Parameter = parameter;
        IsInfo = isInfo;
        IdentifierOrKeyPath = identifierOrKeyPath;
        InterfaceNames = interfaceNames;
        Columns = columns;
    }

    public IMutableParameter Parameter { get; }
    public bool IsInfo { get; }
    public string IdentifierOrKeyPath { get; }
    public IReadOnlyList<string> InterfaceNames { get; }
    public IReadOnlyList<string> Columns { get; }
}

public class MockAddedSubclassAInfo : MockSubclassAInfo
{
    public MockAddedSubclassAInfo(string identifier, string editError) : base(identifier) => ReturnEditPropertyError = editError;

    // like a generated class, which fails on a name that isn't one of its columns
    public override bool EditProperty(IParameterManager parameterManager, string propertyName, string value, out string error)
    {
        if (propertyName != "Name")
        {
            error = "property doesn't exist";
            return false;
        }
        return base.EditProperty(parameterManager, propertyName, value, out error);
    }
}

public class MockAddedSubclassBInfo : MockSubclassBInfo
{
    public MockAddedSubclassBInfo(string identifier, string editError) : base(identifier) => ReturnEditPropertyError = editError;
}

public class MockAddedKeyValueStruct : MockKeyValueStruct
{
    private readonly IParameterManager _parameterManager;

    public MockAddedKeyValueStruct(IParameterManager parameterManager) :
        base(parameterManager, null, 0, null, new string[0]) => _parameterManager = parameterManager;

    public override bool EditProperty(IParameterManager parameterManager, string propertyName, string value, out string error)
    {
        if (!base.EditProperty(parameterManager, propertyName, value, out error))
            return false;
        if (propertyName == "Description")
            Description = value;
        else if (propertyName == "Value")
            Value = int.Parse(value);
        else if (propertyName == "InnerStruct")
            _innerStruct = new ParameterStructReferenceRuntime<IInnerKeyValueStruct>(_parameterManager, value);
        return true;
    }
}

public class MockAddedInnerKeyValueStruct : MockInnerKeyValueStruct
{
    public MockAddedInnerKeyValueStruct() : base(null, 0) { }

    public override bool EditProperty(IParameterManager parameterManager, string propertyName, string value, out string error)
    {
        if (!base.EditProperty(parameterManager, propertyName, value, out error))
            return false;
        if (propertyName == "Description")
            Description = value;
        else if (propertyName == "Value")
            Value = int.Parse(value);
        return true;
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
