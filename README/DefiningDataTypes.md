# Defining Data Types <!-- omit in toc -->
To define new or modify existing Scriptable Objects, modify Interfaces and/or Enums located in the folder `Assets/Parameters/Interfaces/`.  Code for parameter Scriptable Objects and relevant files are automatically created upon compilation of new changes to files in this folder.

These files live under the `Interfaces` folder and can be organized in subfolders.

Before reading about the workflows below, read about the anatomy of interfaces & enum at [InterfacesAndEnums.md](InterfacesAndEnums.md).

```bash
Assets/
├─ Parameters/
│  ├─ Interfaces/
│  │  ├─ Enums
│  │  │  ├─ CurrencyType.cs (enum)
│  │  ├─ Structs
│  │  │  ├─ RewardStruct.cs (struct interface)
│  │  ├─ AssemblyInfo.cs
│  │  ├─ ParameterInterface.asmdef
│  │  ├─ ICurrencyInfo.cs (info interface)
│  │  ├─ IBuildingInfo.cs (info interface)
│  ├─ GeneratedCode/ (contains generated files that implement the defined interfaces)
```

- [Interfaces](#interfaces)
  - [Adding an Interface](#adding-an-interface)
  - [Deleting an Interface](#deleting-an-interface)
  - [Renaming An Interface](#renaming-an-interface)
- [Interface Properties](#interface-properties)
  - [Adding a Property to Existing Interface](#adding-a-property-to-existing-interface)
  - [Deleting a Property from Existing Interface](#deleting-a-property-from-existing-interface)
  - [Renaming An Interface Property](#renaming-an-interface-property)
- [Other](#other)
  - [Resolving Merge Conflicts](#resolving-merge-conflicts)
  - [One Time Mac Setup](#one-time-mac-setup)
  - [Interface Extensions](#interface-extensions)
  - [Game Specific Tooling](#game-specific-tooling)

# Interfaces

## Adding an Interface
To add an interface, simply crete a new interface file under the `Assets/Parameters/Interfaces/` folder or subfolder.  Upon Unity compilation, code will automatically be generated author the interface as Scriptable Objects.

**Example**
- Create `IDragonInfo.cs` in `Assets/Parameters/Interfaces`.
- Upon compilation, new files will be modified & added in `Assets/Parameters/GeneratedCode/`.
- Commit these changes.

## Deleting an Interface

To delete an interface, follow the following steps:
- Delete the interface file.
- If there are any references to that interface in game code, those will need to be resolved.
- Delete the associated valiation file under `Assets/Parameters/Validation/`
- Compilation
  - There will also be compilation errors in auto generated files in `Assets/Parameters/GeneratedCode/`.
  - Upon this compilation error, the package will automatically disable any classes that implement these interfaces and allow compilation again.
  - Upon compilation, the correct code will be auto-regenerated in the `Assets/Parameters/GeneratedCode` folder.
- Commit these changes.

**Example**
- If `ICurrencyInfo.cs` is no longer needed.
- Delete it from `Assets/Parameters/Interfaces/ICurrencyInfo.cs`.
- Resolve game code that have compilation errors from this removal.
- This will result in errors in the auto generated code:
  - `The type or namespace name 'ICurrencyInfo' could not be found`
- The package will automatically disable code paths causing errors in auto generated code and allow compilation.
- New code in `Assets/Parameters/GeneratedCode` will be regenerated to reflect the correct changes.
- Delete old `CurrencyInfo` Scriptable Objects.

## Renaming An Interface
Renaming an interface creates a new Scriptable Object of a different name therefore any previous data created with the original Scriptable Object class (with a unique guid) may be lost if the instructions below are not followed.

**Example**
1. Lets say we'd like to rename `IAnimalInfo` to `ICritterInfo`.
2. Find the Scriptable Object class for the object to rename `AnimalInfoScriptableObject`.  It should be under the `Assets/Parameters/GeneratedCode/Editor/ScriptableObjects` folder.
3. Use your IDE and use the refactor functionality & rename the class to the desired name `CritterInfoScriptableObject`.  Your IDE should make all of the code changes across the board to change this.
4. Wait for Unity to recompile and click on a few of your Scriptable Objects to ensure they're displayed correctly in the inspector.
5. Open the `IAnimalInfo` interface file and do the same IDE refactor & rename to `ICritterInfo`.
6. After a Unity recompliation, it will trigger another code generation to rename all generated files & variables correctly.
7. Wait for Unity to recompile and click on a few of your Scriptable Objects to ensure they're displayed correctly in the inspector.
8. Delete the newly created `CritterInfoValidator` in the `Assets/Parameters/Validation` Folder.
9. Use your IDE to refactor & rename the `AnimalInfoValidator` class to `CritterInfoValidator`

# Interface Properties

## Adding a Property to Existing Interface

To add a new property to an existing interface:
- Add the new property to the interface.
- Compilation
  - This change will result in compilation errors in the `Assets/Parameters/GeneratedCode` files that implement the changed interface.
  - Upon this compilation error, the parameter system will automatically disable any classes that implement these interfaces and allow compilation again.
  - Upon compilation the correct code will be auto-regenerated in the `Assets/Parameters/GeneratedCode` folder.
- Commit these changes.
- The property can now be utilized.

**Example**
- Add a new property `int RewardCount { get; }` to `IEventInfo`.
- This will result in two errors in the auto generated code:
  - `EventInfoScriptableObject' does not implement interface member 'IEventInfo.RewardCount`
  - `EventInfoFlatBuffer' does not implement interface member 'IEventInfo.RewardCount`
- The parameter system will automatically disable these code paths and allow compilation.
- Upon compilation, `Assets/Parameters/GeneratedCode` will be re-generated with the correct implementation code.

## Deleting a Property from Existing Interface
- Delete the property from the interface.
- Any game specific compilation errors due to deleting of property needs to be resolved.
- Upon compilation, code in `Assets/Parameters/GeneratedCode` will be regenerated to reflect the updates.
- Commit these changes.

## Renaming An Interface Property
> ⚠️ Note: This flow will be improved & simplified in a future release.

To rename a property, we must ensure that the serialized Scriptable Object saves the data under the correct new name.  There is tooling provided to support this.

**Example**
- Lets say we'd like to rename this property `int InitialAmount { get; }` to `int StartingAmount { get; }` in `ICurrencyInfo`.
- Find the Scriptable Object file for the interface `Assets/Parameters/GeneratedCode/Editor/ScriptableObjects/CurrencyInfoScriptableObject.cs`.
- Find the field that's mapped to that interface `int _initialAmount`.
- Rename it utilizing `FormerlySerializedAs`.  It should look like this: `[FormerlySerializedAs("_initialAmount")] public int _startingAmount;`.
- Go to the Parameters config window under `Pocket Gems` -> `Config Panel`.  Press the `Re-save All Scriptable Objects`.  This will re-save all Scriptable Objects under the new field.
- Go to the `ICurrencyInfo.cs` and utilize your IDE's "refactor" feature to rename the property from `InitialAmount` to `StartingAmount`.  This should rename all references to this interface across the code base.
- Upon compilation, the correct code in `Assets/Parameters/GeneratedCode` will be generated.
- Commit `.asset` and code changes.

# Other

## Resolving Merge Conflicts
> ⚠️ Note: This flow will be improved & simplified in a future release.

If two developers are making modifications to the parameters interfaces, this can result in conflict the auto-generated code.
- Resolve any game code merge conflicts.
- Resolve the conflict in any files under `Assets/Parameters/Interfaces/` to intended outcome.
- Code files in `Assets/Parameters/GeneratedCode` will be regenerated again once the game code is compiled.  Therefore comment out any code within files in that folder to get the game to compile without concern for correctness.
- Upon the game compiling, the correct code in `Assets/Parameters/GeneratedCode` will be auto generated.

## One Time Mac Setup
On MacOS, the first time code generation occurs, a permissions error will occur.

You must go to your mac OS's `System Preferences` --> `Security & Privacy` --> `Allow "flatc"`.  After this, try generating with the menu item `Pocket Gems` --> `Parameters` --> `Regenerate All Data & CSVs`.

## Interface Extensions
Interface extensions can be authored in the application in order to add helper methods to interface objects.

**Example**
```
public static class IEventCurrencyInfoExt
{
    // Example Extension
    public static int CalculateBonus(this IEventCurrencyInfo info)
    {
        return (int)(info.StartingAmount * info.EventBonusChance);
    }
}
```

## Game Specific Tooling

The auto generated scripts for Scriptable Objects located under `Assets/Parameters/GeneratedCode/Editor/ScriptableObjects` are `internal` & Editor only so that developers do not accidentally directly reference the class at runtime (the `ParameterReference<>` must be used instead).

In cases where game specific tooling is required to access, modify, or create Scriptable Objects directly, these `internal` classes can be accessed by exposing the assembly internals to a game specific editor assembly.  Add `[assembly: InternalsVisibleTo("ASSEMBLY.NAME.HERE")]` attribute to `Assets/Parameters/GeneratedCode/Editor/AssemblyInfo.cs`.  These changes will not be overridden and persist across code generation.

**Example**
```C#
// Assembly Info to expose internals of generated parameter code to game specific editor tools
// DO NOT EXPOSE INTERNALS TO RUNTIME CODE!

using System.Runtime.CompilerServices;
// Made up example:
// glam specific Editor tool that auto generates Scriptable Object content based on designer inputs
[assembly: InternalsVisibleTo("Glam.ContentCreation.Editor")]

// hash:[d6fe471fb74d0f566ac79bcb9c682415]
```
