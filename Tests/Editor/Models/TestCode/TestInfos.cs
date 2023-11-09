using System.Collections;
using PocketGems.Parameters.Interface;

// valid interface
public interface ISuperInfo : IBaseInfo
{
    string MyString { get; }
    float MyFloat { get; }
}

// valid interface
public interface IPassingInfo : ISuperInfo
{
    int MyInt { get; }
}

// bad: non public interface
internal interface IInternalInfo : IBaseInfo
{
    int MyInt { get; }
}

// bad: badly named
public interface IBadlyNamed : IBaseInfo
{
    int MyInt { get; }
}

// bad: isn't sub interface of IBaseInfo
public interface INonBaseInfo
{
    int MyInt { get; }
}

// bad: cannot derive from non IBaseInfo interface
public interface IDeriveFromBadInterfaceInfo : IBaseInfo, IEnumerable
{
    int MyInt { get; }
}

// bad: cannot derive from non IBaseInfo interface
public interface IBadParameterNameInfo : IBaseInfo
{
    int My_Int { get; }
}

// bad: implements a duplicate property
public interface IDuplicatePropertyInfo : IBaseInfo
{
#pragma warning disable CS0108 // use the new keyword
    string Identifier { get; }
#pragma warning restore CS0108 // use the new keyword
}

// bad: uses reserved keyword in property name
public interface IKeywordPropertyInfo : IBaseInfo
{
    string Int { get; }
}

// bad: implements a non supported property type
public interface INonSupportedTypeInfo : IBaseInfo
{
    IEnumerable MyEnumerable { get; }
}

// bad: has a setter
public interface ISetterInfo : ISuperInfo
{
    int MyInt { get; set; }
}

// bad: has a method
public interface IInternalPropertyInfo : ISuperInfo
{
    internal int MyInt { get; }
}

// bad: has a method
public interface IMethodInfo : ISuperInfo
{
    void MyInt();
}

// bad: has name space
namespace MyNameSpace
{
    public interface INameSpacedInfo : IBaseInfo
    {
        int MyInt { get; }
    }
}
