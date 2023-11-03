# Data Validation <!-- omit in toc -->

- [Running Data Validation](#running-data-validation)
  - [Inspector](#inspector)
  - [All Data](#all-data)
  - [Debug Code](#debug-code)
- [Building Data Validation](#building-data-validation)
  - [Pre-Defined Validation](#pre-defined-validation)
    - [Scriptable Object](#scriptable-object)
    - [Broken References](#broken-references)
  - [Adding Validation: Interface Atrributes](#adding-validation-interface-atrributes)
  - [Adding Validation: Scripting](#adding-validation-scripting)
    - [Template](#template)
    - [ValidateInfo](#validateinfo)
    - [ValidateParameters](#validateparameters)
    - [ValidateStruct](#validatestruct)

# Running Data Validation
There are different instances when the validation code is executed.

## Inspector
When editing in the inspector, only the validaiton code for the currently opened Scriptable Object will be ran.

## All Data
Valdation for **all data** will be ran in the following situations:
- On Unity startup.
- On device build.
- When there changes in Scriptable Objects (git pull, saved modified scriptable object)
- Manually triggered from the validation window `Pocket Gems` --> `Parameters` --> `Validation Window`.
- At runtime (both edtior & device) if configured for it (see below)

## Debug Code
Validation code is not automatically ran at app runtime due to the performance overhead.

Debug code must be added at strategic times in order to run the validation code on debug/test builds.  Examples could be:
- After `ParameterManager` is initalized for the first time on launch.
- After AB Testing has been applied

```C#
if (areTestMenusEnabled)
{
    var errors = ParamsValidation.Validate();
    for (int i = 0; i < errors.Count; i++)
        Debug.LogError($"Parameter Error: {errors[i]}");
}
```

# Building Data Validation

## Pre-Defined Validation
Pre-defined validations are ran on specific types.

### Scriptable Object
The scriptable objects are always checked to ensure that they're not marked as an addressable asset.

### Broken References
For the following reference types, assigned references they will be checked to ensure there are no broken references.
- `AssetReference`
- `IReadOnlyList<AssetReference>`
- `AssetReferenceSprite`
- `IReadOnlyList<AssetReferenceSprite>`
- `ParameterReference<>`
- `IReadOnlyList<ParameterReference<>>`

## Adding Validation: Interface Atrributes
Assert attributes can be added to the `IBaseInfo` or `IBaseStruct` sub-interface properties.  One or many of these attributes can be added to each property.

To see the extensive list of attributes, see [ValidationAttributes.md](ValidationAttributes.md).

```C#
// example sub interface
public interface IEventCurrencyInfo : ICurrencyInfo
{
  [AssertGreater(0f)]
  float EventDropRate { get; }
}
```

## Adding Validation: Scripting
Upon code generation, validation files for each interface will be created in the `Assets/Parameters/Validation` folder.  More complex validation checks can be added to the respective classes.

### Template
Example for `ICurrencyInfo`.
```C#
namespace Parameters.Validation
{
    public class CurrencyInfoValidator : BaseDataValidator<ICurrencyInfo>
    {
        protected override void ValidateInfo(IParameterManager parameterManager, ICurrencyInfo info)
        {
            // Add validation code here.
        }

        protected override void ValidateParameters(IParameterManager parameterManager)
        {
            // Add validation code here.
        }
    }
}
```

Example for `IRewardStruct`.
```C#
namespace Parameters.Validation
{
    public class RewardStructValidator : BaseDataValidatorStruct<IRewardStruct>
    {
        protected override void ValidateStruct(IParameterManager parameterManager, IRewardStruct structObj)
        {
            // Add validation code here.
        }
    }
}
```

### ValidateInfo
`ValidateInfo` will be called for every row of data that conforms to `ICurrencyInfo`.  More complex checks that cannot be enforced via attributes can be written here.

Upon finding an error `Error` should be called with the `nameof()` the property and the error message.  The system already knows the `Identifier` of the `info` so there is no need to incooporate that as part of the error message.
```C#
protected override void ValidateInfo(IParameterManager parameterManager, ICurrencyInfo info)
{
  if (info.IsPremiumCurrency && string.IsNullOrEmpty(info.PremiumCurrencyIconSprite.AssetGUID))
    Error(nameof(ICurrencyInfo.PremiumCurrencyIconSprite), "required for premium currency");

  // add other checks here
}
```

### ValidateParameters
ValidateParameters will be called once.  This can be used for more holistic checks.

Upon error, `Error` can be called with the error message.
```C#
protected override void ValidateParameters(IParameterManager parameterManager)
{
    HashSet<Color> seenColors = new HashSet<Color>();
    foreach (var info in parameterManager.Get<ICurrencyInfo>())
    {
        if (seenColors.Contains(info.CurrencyColor))
            Error($"Colors must be unique, the color {info.CurrencyColor} is used more than once.");
        else
            seenColors.Add(info.CurrencyColor);
    }
    // add other checks here
}
```

### ValidateStruct
`ValidateStruct` will be called for every struct that conforms to `IRewardStruct`.  More complex checks that cannot be enforced via attributes can be written here.

Upon finding an error `Error` should be called with the `nameof()` the property and the error message.  The system already knows the `Identifier` of the `info` and KeyPath where the struct is located so there is no need to incooporate that as part of the error message.

In this validation, the code doesn't know about the context of where the data is used.  For top-down validation utilize the appropriate parent Struct or Info that references the struct.
```C#
protected override void ValidateStruct(IParameterManager parameterManager, IRewardStruct structObj)
{
  if (structObj.IsPremiumCurrency && string.IsNullOrEmpty(structObj.PremiumCurrencyIconSprite.AssetGUID))
    Error(nameof(IRewardStruct.PremiumCurrencyIconSprite), "required for premium currency");

  // add other checks here
}
```