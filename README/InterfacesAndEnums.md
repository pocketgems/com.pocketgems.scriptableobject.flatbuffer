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
  - [Date \& Time](#date--time)
  - [Unity Types](#unity-types)
  - [Enums](#enums)
  - [Parameter References](#parameter-references)
  - [Parameter Struct References](#parameter-struct-references)
  - [Addressable References](#addressable-references)
- [Attributes](#attributes)
  - [ParameterHeader](#parameterheader)
  - [ParameterTooltip](#parametertooltip)
  - [ParameterFoldOut](#parameterfoldout)
  - [ParameterAttachFieldAttribute](#parameterattachfieldattribute)
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
- Must inherit from `IBaseInfo` or from an interface that inherits `IBaseInfo`.
- May inherit from multiple other `Info` interfaces as long as property names do not collide.
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

### Date & Time
```C#
TimeSpan Duration { get; }
IReadOnlyList<TimeSpan> Durations { get; }

DateTime Birthday { get; }
IReadOnlyList<DateTime> Birthdays { get; }
```

### Unity Types
```C#
Color TextColor { get; }
IReadOnlyList<Color> TextColors { get; }

Vector2 Boundary { get; }
IReadOnlyList<Vector2> Boundaries { get; }

Vector2Int GridFootprint { get; }
IReadOnlyList<Vector2Int> GridFootprints { get; }

Vector3 StartingLocation { get; }
IReadOnlyList<Vector3> StartingLocations { get; }

Vector3Int Dimension { get; }
IReadOnlyList<Vector3Int> Dimensions { get; }
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

## Attributes
[Unity attributes](https://docs.unity3d.com/2020.3/Documentation/ScriptReference/RangeAttribute.html) are useful for facilitating content creation tooling.

Since the outputed generated Scriptable Object code cannot have attributes added directly from the developer, attributes for the Scriptable Object field can be defined from the interface.

### ParameterHeader
To attach a [HeaderAttribute](https://docs.unity3d.com/ScriptReference/HeaderAttribute.html) to the generated Scriptable Object, add a `ParameterHeader` to the interface property getter.

```
[ParameterHeader("Health Settings")]
int Health { get; }
int MaxHealth { get; }
```

### ParameterTooltip
To attach a [TooltipAttribute](https://docs.unity3d.com/ScriptReference/TooltipAttribute.html) to the generated Scriptable Object, add a `ParameterTooltip` to the interface property getter.

```
[ParameterTooltip("This is the starting spawn health of the hero")]
int StartingHealth { get; }
```

### ParameterFoldOut
To wrap a [Foldout](https://docs.unity3d.com/ScriptReference/EditorGUILayout.Foldout.html) around a group of fields, use `ParameterFoldOut` on the first field to start.

The foldout will wrap all fields below it until one of the two conditions are met:
1. The foldout reaches another `ParameterFoldOut`
2. The next field was declared in another interface.

```
[ParameterFoldOut("Health")]
int Health { get; }
int MaxHealth { get; }

[ParameterFoldOut("Spells")]
int Mana { get; }
int MaxMana { get; }
```

### ParameterAttachFieldAttribute
There may be less common attributes that a developer may want to attach to a generated Scriptable Object.  In order to add a other attributes, use the `ParameterAttachFieldAttribute`.

For example, if a [Range()](https://docs.unity3d.com/ScriptReference/RangeAttribute.html) and [TextArea](https://docs.unity3d.com/ScriptReference/TextAreaAttribute.html) attribute is desired, utilize the `ParameterAttachFieldAttribute` in the interface.

```
[ParameterAttachFieldAttribute("[Range(0,10)]")]
int Scale { get; }

[ParameterAttachFieldAttribute("[TextArea(3)]")]
string Description { get; }
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
