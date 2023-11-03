# Interfaces & Enums <!-- omit in toc -->

- [Info Interfaces](#info-interfaces)
  - [Overview](#overview)
  - [Requirements](#requirements)
- [Struct Interfaces](#struct-interfaces)
  - [Overview](#overview-1)
  - [Requirements](#requirements-1)
- [Interface Property Types](#interface-property-types)
  - [Strings](#strings)
  - [Localized Strings](#localized-strings)
  - [Scalars](#scalars)
  - [Unity Types](#unity-types)
  - [Enums](#enums)
  - [Parameter References](#parameter-references)
  - [Parameter Struct References](#parameter-struct-references)
  - [Addressable References](#addressable-references)
- [Adding Attributes](#adding-attributes)
- [Enums](#enums-1)
  - [Overview](#overview-2)
  - [Requirements](#requirements-2)

## Info Interfaces

### Overview
If there are any changes to an interface, all source code will be re-generated to reflect usage of the new API.

This is an example interface under `Assets/Parameters/Interfaces/ICurrencyInfo.cs`.  Comments are allowed in these files.
```C#
public interface ICurrencyInfo : IBaseInfo
{
  // user facing display name for currency
  LocalizedString DisplayName { get; }
  AssetReferenceSprite Icon { get; }
  CurrencyType CurrencyType { get; }
  // amount a new player starts with
  int InitialAmount { get; }
  float DropRate { get; }
}
```

This is a basic of example of what the auto-generated Scriptable Object that implements the interface would look like.
```C#
namespace Parameters
{
  [CreateAssetMenu(fileName = "CurrencyInfo", menuName = "Parameters/CurrencyInfo")]
  internal class CurrencyInfoScriptableObject : ParameterScriptableObject, ICurrencyInfo
  {
    public string Identifier => name;

    public string _displayName;
    public LocalizedString DisplayName => new LocalizedString(_displayName);

    public AssetReferenceSprite _icon;
    public AssetReferenceSprite Icon => _icon;

    public CurrencyType _currencyType;
    public CurrencyType CurrencyType => _currencyType;

    public int _initialAmount;
    public int InitialAmount => _initialAmount;

    public float _dropRate;
    public float DropRate => _dropRate;
  }
}
```

Other files are also created behind the scene to handle optimal data compression, loading, and runtime access.

### Requirements
Requirements for interfaces are as follows:
- Interface name must follow the pattern `I[A-Z]+[A-Za-z0-9]*Info`.
- Visbility must be `public`.
- Must not have a namespace.
- Must inherit from or from an interface that inherits `IBaseInfo`.
- Must NOT inherit any other external interfaces (e.g. `IEnumerable`).
- Can only define public property getters. Methods & setters are not allowed.
- Propery names must follow the pattern `[A-Z]+[A-Za-z0-9]*`.
- Duplicate property names are not allowed.

## Struct Interfaces

### Overview

Structs are supported and follow the same workflow as defined infos above.

```C#
public interface ITransactionStruct : IBaseStruct
{
  string Description { get; }
  ParameterRefereince<ICurrencyInfo> Currency { get; }
  int Amount { get; }
}
```

Infos or other Structs can reference the struct via [`ParameterStructReference<>`](#parameter-struct-references).
```C#
public interface IRewardBoxStruct : IBaseStruct
{
  LocalizedString DisplayName { get; }
  IReadOnlyList<ParameterStructReference<ITransactionStruct>> Transactions { get; }
}

public interface IEventInfo : IBaseInfo
{
  ParameterStructReference<IRewardBoxStruct> Rewards {get;}
}
```

### Requirements
Requirements for interfaces are as follows:
- Interface name must follow the pattern `I[A-Z]+[A-Za-z0-9]*Struct`.
- Visbility must be `public`.
- Must not have a namespace.
- Must inherit from or from an interface that inherits `IBaseStruct`.
- Must NOT inherit any other external interfaces (e.g. `IEnumerable`).
- Can only define public property getters. Methods & setters are not allowed.
- Propery names must follow the pattern `[A-Z]+[A-Za-z0-9]*`.
- Duplicate property names are not allowed.
- Similar to traditional Structs, circular references using `ParameterStructReference` are not allowed directly between structs.  However a circular reference with an info as part of the dependency chain is allowed.
  - Invalid: `IOneStruct` -> `ITwoStruct` -> `IOneStruct`
  - Valid: `IOneStruct` -> `ICurrencyInfo` -> `IOneStruct`

## Interface Property Types
These are the following types that can be defined in Interface property getters.

### Strings
```C#
string Identifier { get; }
IReadOnlyList<string> Strings { get; }
```

### Localized Strings
Localized strings are used to translate user displayed text.  These values should never be utilized and stored in model data.

```C#
LocalizedString Description { get; }
IReadOnlyList<LocalizedString> Names { get; }
```

**Utilizing LocalizedStrings**

To fetch the translated string, access the `Text` property of the `LocalizedString`.
```C#
string displayString = currencyInfo.Description.Text;
```

### Scalars
```C#
bool Enabled { get; }
short BonusAmount { get; }
int StartingAmount { get; }
long RewardedAmount { get; }
float BonusChance { get; }
ushort DayOfWeek { get; }
uint CurrencyAmount { get; }
ulong VeryLargeAmount { get; }
```

List of scalars.
```C#
IReadOnlyList<bool> Toggles { get; }
IReadOnlyList<short> Levels { get; }
IReadOnlyList<int> RewardAmounts { get; }
IReadOnlyList<long> BonusRewardAmounts { get; }
IReadOnlyList<float> BonusChances { get; }
IReadOnlyList<ushort> DayOfWeeks { get; }
IReadOnlyList<uint> CurrencyAmounts { get; }
IReadOnlyList<ulong> VeryLargeAmounts { get; }
```

A `ParameterLocalizationHandler` is provided to provide a delegate to return translated strings.
```C#
# setting the translation delegate
string TheTranslation(string inputString)
{
  // return translated string of inputString
}

ParameterLocalizationHandler.GlobalTranslateStringDelegate = TheTranslation;
```

### Unity Types
```C#
Color TextColor { get; }

Vector2 Boundry { get; }
Vector2Int GridFootprint { get; }

Vector3 StartingLocation { get; }
Vector3Int Dimensions { get; }
```

### Enums
Enums must be declared in the `ParameterInterface` assembly.  They can be utilized as a property.

```C#
// example
// CurrencyType is a developer defined enum
CurrencyType CurrencyType { get; }
IReadOnlyList<CurrencyType> CurrencyTypes { get; }
```

### Parameter References
ParameterReferecnes are utilized to reference other info parameters.

```C#
ParameterReference<IBuildingInfo> Building { get; }
IReadOnlyList<ParameterReference<ICurrencyInfo>> Rewards { get; }
```

### Parameter Struct References
ParameterReferecnes are utilized to reference structs.

```C#
ParameterStructReference<ITransactionStruct> Cost { get; }
IReadOnlyList<ParameterStructReference<ITransactionStruct>> Rewards { get; }
```

### Addressable References
Addressable asset & sprite references are supported if the [`com.unity.addressables`](https://docs.unity3d.com/Packages/com.unity.addressables@latest/index.html) package is added to the Unity project.
```C#
AssetReference WorldItemPrefab { get; }
AssetReferenceSprite Icon { get; }
AssetReferenceAtlasedSprite Image { get; }
IReadOnlyList<AssetReference> Doobers { get; }
IReadOnlyList<AssetReferenceSprite> LoadingImages { get; }
IReadOnlyList<AssetReferenceAtlasedSprite> Images { get; }
```

## Adding Attributes
[Unity attributes](https://docs.unity3d.com/2020.3/Documentation/ScriptReference/RangeAttribute.html) are useful for facilitating content creation tooling.

Since the outputed generated Scriptable Object code cannot have attributes directly from the developer, attributes for the Scriptable Object field can be defined from the interface.

For example, if a `Range()` and `ToolTip()` attribute is desired for the Scriptable Object field representing the `Scale` property below, utilize the `AttachFieldAttribute` in the interfacere.

```
[AttachFieldAttribute("[Range(0,10)]")]
[AttachFieldAttribute("[Tooltip(\"My tool tip\")]")]
int Scale { get; }
```

During Scriptable Object code generation, the attribute will be applied to the field.

**Example Auto Generated Code**
```
[Range(0,10)]
[Tooltip("My tool tip")
int _scale;
```

## Enums

### Overview
Custom enums can also be referenced from Interfaces.  Referencing enums external to the `ParameterInterface.asmdef` are not currently supported.

This is an example enum under `Assets/Parameters/Interfaces/CurrencyType.cs`. Comments are allowed in these files.
```C#
public enum CurrencyType
{
    Regular, // general currency
    Premium, // currency bought with IAP
    Event // event specific currency
}
```


```C#
// flags are supported
[Flags]
public enum UnitAttributes
{
  None = 0,
  Attacker = 1,
  Flyer = 1 << 1
  Support = 1 << 2
  Healer = 1 << 3
}
```

```C#
// specifying the data type is supported
public enum UnitTypeTag : short
{
    Enemy,
    Ally,
    Boss
}
```

### Requirements
Requirements for enums are as follows:
- Visbility must be `public`.
- Must not have a namespace.
