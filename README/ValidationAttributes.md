# Validation Attributes <!-- omit in toc -->

- [Overview](#overview)
- [Scalar Comparisons](#scalar-comparisons)
  - [AssertGreater](#assertgreater)
  - [AssertGreaterOrEqual](#assertgreaterorequal)
  - [AssertLess](#assertless)
  - [AssertLessOrEqual](#assertlessorequal)
  - [Example](#example)
- [List](#list)
  - [AssertListNotEmpty](#assertlistnotempty)
- [References](#references)
  - [AssertAssignedReference](#assertassignedreference)
- [Strings](#strings)
  - [AssertRegex](#assertregex)
  - [AssertStringNotEmpty](#assertstringnotempty)
- [Writing Custom Attributes](#writing-custom-attributes)
  - [Create Assembly](#create-assembly)
  - [Define Custom Attribute](#define-custom-attribute)
  - [Utilize Attribute](#utilize-attribute)

## Overview
One or more of these Attributes can be added to an interface's properties.  Adding an attribute to a non-compatible property will result in error(s) within the validaiton output.

## Scalar Comparisons
The following operations can be used for `short`, `int`, `long`, and `float` property types.

They are also valid on `IReadOnlyList` of these types.  It will verify that every element in the list passes the assertion.  If the list is null or of size 0, the asset is considered a pass.

### AssertGreater
Assert value is greater than the specified value (see [Example](#example)).

### AssertGreaterOrEqual
Assert value is greater than or equal to the specified value (see [Example](#example)).

### AssertLess
Assert value is less than the specified value (see [Example](#example)).

### AssertLessOrEqual
Assert value is less than or equal to the specified value (see [Example](#example)).

### Example

```C#
public interface ICurrencyInfo : IBaseInfo
{
  [AssertGreater(0)]
  int InitialAmount { get; }

  [AssertGreaterOrEqual(0f)]
  float DropRate { get; }

  [AssertLess(0f)]
  float SomeMultiplier { get; }

  [AssertLessOrEqual(0f)]
  IReadOnlyList<float> SomeMultipliers { get; }
}
```

## List

### AssertListNotEmpty
Assert that any `IReadOnlyList<T>` is not null or empty.

```C#
public interface ICurrencyInfo : IBaseInfo
{
  [AssertListNotEmpty]
  IReadOnlyList<float> DropRates { get; }
}
```

## References

### AssertAssignedReference
Assert that `AssetReference`, `AssetReferenceSprite`, or `ParameterReference<T>` has an assigned object.

They are also valid on `IReadOnlyList` of these types.  It will verify that every element in the list passes the assertion.  If the list is null or of size 0, the asset is considered a pass.

```C#
public interface ICurrencyInfo : IBaseInfo
{
  [AssertAssignedReference]
  ParameterReference<IDropTable> DropTable { get; }

  [AssertAssignedReference]
  IReadOnlyList<ParameterReference<IDropTable>> BonusDropTables { get; }

  [AssertAssignedReference]
  AssetReference MainPrefab { get; }

  [AssertAssignedReference]
  AssetReferenceSprite Icon { get; }

  [AssertAssignedReference]
  IReadOnlyList<AssetReference> UpgradePrefabs { get; }

  [AssertAssignedReference]
  IReadOnlyList<AssetReferenceSprite> DooberIcons { get; }
}
```

## Strings

### AssertRegex
Asserts that a `string` or every string in `IReadOnlyList<string>` matches the regex.  If the list is null or of size 0, the asset is considered a pass.

```C#
public interface ICurrencyInfo : IBaseInfo
{
  [AssertRegex(".*\\.mp3")]
  string Sound { get; }

  [AssertRegex(".*\\.mp3")]
  IReadOnlyList<string> Sounds { get; }
}
```

### AssertStringNotEmpty
Asserts that a `string`, `LocalizedString` or every string in `IReadOnlyList<string>` or `IReadOnlyList<LocalizedString>` is not null or empty.  If the list is null or of size 0, the asset is considered a pass.

```C#
public interface ICurrencyInfo : IBaseInfo
{
  [AssertStringNotEmpty]
  string IconResource { get; }

  [AssertStringNotEmpty]
  LocalizedString DisplayName { get; }

  [AssertStringNotEmpty]
  IReadOnlyList<string> Resources { get; }

  [AssertStringNotEmpty]
  IReadOnlyList<LocalizedString> ToolTips { get; }
}
```

## Writing Custom Attributes

### Create Assembly
To write custom app specific attributes, follow the following steps:
1. Create a new assembly (e.g. `MyAssertAttributes`)
2. Reference the assembly `PocketGems.Parameters.Runtime` in the new assembly.
3. Reference the new assembly (e.g. `MyAssertAttributes`) from the parameter interface assembly where all of the interfaces are written `Assets/Parameters/Interfaces/ParameterInterface.asmdef`.  This is required because only interface & enums can be added directly to the `ParameterInterface.asmdef` assembly.


### Define Custom Attribute
1. Write a subclass of `System.Attribute` that implements the parameter's `IValidationAttribute` interface.
2. At validation time, the following occurs:
   1. `CanValidate` is first called to validate if the attribute is compatible with a property it's assigned to.  This is called once.
   2. if `CanValidate` above is true, `WillValidateProperty` is called for any preparation for the `propertyInfo`. (e.g. some attributes are compatible with many types, this might be a good oportunity to cache what the current type is).  This is called once.
   3.  `Validate(PropertyInfo propertyInfo, object value)` is called for every property of every parameter instance.  Therefore, this can be called multiple times.

This is a basic example that works on `int` properties and checks that the value isn't set to a particualr value.
```C#
using System.Reflection;
using PocketGems.Parameters.Validation.Attributes;

[System.AttributeUsage(System.AttributeTargets.Property)]
public class MyCustomCheckAttribute : System.Attribute, IValidationAttribute
{
    private int _constraint;

    public MyCustomCheckAttribute(int constraint)
    {
        _constraint = constraint;
    }

    // return true if this attribute can validate the property
    public bool CanValidate(PropertyInfo propertyInfo) => propertyInfo.PropertyType == typeof(int);

    // any preparation work or caching needed
    public void WillValidateProperty(PropertyInfo propertyInfo)
    {

    }

    // called on every instance that has this property
    public string Validate(PropertyInfo propertyInfo, object value)
    {
        if ((int)value == _constraint)
            return $"the value cannot be set to {_constraint}";
        return null;
    }
}

```

### Utilize Attribute
Utilize the new attribute on a property.

```C#
public interface ICurrencyInfo : IBaseInfo
{
  [MyCustomCheck(10)]
  int NotTenInt { get; }
}
```

