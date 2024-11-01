using System.Collections;
using System.Collections.Generic;
using PocketGems.Parameters;
using PocketGems.Parameters.Interface;

// valid interface
public interface ISuperStruct : IBaseStruct
{
    string Identifier { get; } // should be treated like a regular string property and valid
    string MyString { get; }
    float MyFloat { get; }
}

// valid interface
public interface IPassingStruct : ISuperStruct
{
    int MyInt { get; }
}

// valid interface
public interface IReferenceStructStruct : ISuperStruct
{
    ParameterStructReference<IPassingStruct> PassingStruct { get; }
    IReadOnlyList<ParameterStructReference<IPassingStruct>> PassingStructs { get; }
}

// bad: non public interface
internal interface IInternalStruct : IBaseStruct
{
    int MyInt { get; }
}

// bad: badly named
public interface INamedBadly : IBaseStruct
{
    int MyInt { get; }
}

// bad: isn't sub interface of IBaseStruct
public interface INonBaseStruct
{
    int MyInt { get; }
}

// bad: cannot derive from non IBaseStruct interface
public interface IDeriveFromBadInterfaceStruct : IBaseStruct, IEnumerable
{
    int MyInt { get; }
}

// bad: cannot derive from non IBaseStruct interface
public interface IBadParameterNameStruct : IBaseStruct
{
    int My_Int { get; }
}

// bad: implements a duplicate property
public interface IDuplicatePropertyStruct : ISuperStruct
{
#pragma warning disable CS0108 // use the new keyword
    string Identifier { get; }
#pragma warning restore CS0108 // use the new keyword
}

// bad: implements a non supported property type
public interface INonSupportedTypeStruct : IBaseStruct
{
    IEnumerable MyEnumerable { get; }
}

// bad: has a setter
public interface ISetterStruct : ISuperStruct
{
    int MyInt { get; set; }
}

// bad: has a method
public interface IInternalPropertyStruct : ISuperStruct
{
    internal int MyInt { get; }
}

// bad: has a method
public interface IMethodStruct : ISuperStruct
{
    void MyInt();
}

// bad: has name space
namespace MyNameSpace
{
    public interface INameSpacedStruct : IBaseStruct
    {
        int MyInt { get; }
    }
}

// bad: referencing base struct
public interface IReferenceBaseStruct : IBaseStruct
{
    ParameterStructReference<IBaseStruct> MyStruct { get; }
}

// bad: referencing base struct
public interface IListReferenceBaseStruct : IBaseStruct
{
    IReadOnlyList<ParameterStructReference<IBaseStruct>> MyStructs { get; }
}

/*
 * Circular Reference Testing
 */

// basic circular reference
public interface ICircularAStruct : IBaseStruct
{
    ParameterStructReference<ICircularBStruct> Circular { get; }
}

// basic circular reference
public interface ICircularBStruct : IBaseStruct
{
    ParameterStructReference<ICircularAStruct> Circular { get; }
}

// circular reference in list
public interface ICircularCStruct : IBaseStruct
{
    IReadOnlyList<ParameterStructReference<ICircularAStruct>> Circular { get; }
}

// circular reference in super class references
public interface ICircularDStruct : ICircularBStruct
{
}

// circular reference between list
public interface ICircularEStruct : IBaseStruct
{
    IReadOnlyList<ParameterStructReference<ICircularFStruct>> Circular { get; }
}

// circular reference between list
public interface ICircularFStruct : IBaseStruct
{
    IReadOnlyList<ParameterStructReference<ICircularEStruct>> Circular { get; }
}

// circular reference between self (direct reference)
public interface ICircularGStruct : IBaseStruct
{
    ParameterStructReference<ICircularGStruct> Circular { get; }
}

// circular reference between self (list reference)
public interface ICircularHStruct : IBaseStruct
{
    IReadOnlyList<ParameterStructReference<ICircularHStruct>> Circular { get; }
}

// deeper circular reference
public interface ICircularIStruct : IBaseStruct
{
    ParameterStructReference<ICircularJStruct> Circular { get; }
}

// deeper circular reference
public interface ICircularJStruct : IBaseStruct
{
    IReadOnlyList<ParameterStructReference<ICircularKStruct>> Circular { get; }
}

// deeper circular reference
public interface ICircularKStruct : IBaseStruct
{
    ParameterStructReference<ICircularIStruct> Circular { get; }
}
