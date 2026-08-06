# String Localization <!-- omit in toc -->
There is support to tag strings for localization, collect all strings to translate, and return translated strings during runtime.

- [Tagging Properties](#tagging-properties)
  - [Example](#example)
- [\[Editor\] Collection of All Strings](#editor-collection-of-all-strings)
- [Runtime Translation](#runtime-translation)
  - [Example](#example-1)
  - [Setup](#setup)


## Tagging Properties
Tag a `string` or `IReadOnlyList<string>` property in your interface to mark it for localization support.

- `[ParameterLocalizationKey]`: Tags standard user-facing strings that require translation.
- `[ParameterLocalizableScript]`: Tags strings containing executable scripts or commands that require translation.

> [!WARNING]
> Attributes should only be applied to user-facing strings.

### Example
```C#
public interface ICharacterInfo : IBaseInfo
{
    [ParameterLocalizationKey]
    string DisplayName { get; }

    [ParameterLocalizationKey]
    IReadOnlyList<string> Taglines { get; }

    [ParameterLocalizableScript]
    string DialogScript { get; }

    [ParameterLocalizableScript]
    IReadOnlyList<string> VictoryDialogScripts { get; }

    // These are used internally and not user-facing, so they are not tagged
    string AnimalType { get; }
    int MaxHealth { get; }
}
```

## [Editor] Collection of All Strings
In the Unity Editor, you can make a static call to retrieve all unique strings tagged for localization.

```c#
var collectedLocalizedStrings = EditorParameterDataManager.CollectLocalizationStrings();
```

This returns an `ICollectedLocalizationStrings` object containing every unique string found with localization attributes. These strings can then be exported for translation using your preferred localization system.

```c#
namespace PocketGems.Parameters.Editor.Localization
{
    public interface ICollectedLocalizationStrings
    {
        enum State
        {
            Success,
            Error
        }

        /// <summary>
        /// The state of the results of attempting to collect strings.
        /// </summary>
        State Result { get; }

        /// <summary>
        /// The error message if the string collection resulted in an Error.
        /// </summary>
        string ErrorMessage { get; }

        /// <summary>
        /// If successful, all unique localization keys values of parameter interface properties that are tagged with the
        /// [ParameterLocalizationKey] attribute;
        ///
        /// Otherwise, null on error.
        /// </summary>
        IEnumerable<string> LocalizationKeys { get; }

        /// <summary>
        /// If successful, all unique localization script values of parameter interface properties that are tagged with the
        /// [ParameterLocalizableScriptAttribute] attribute;
        ///
        /// Otherwise, null on error.
        /// </summary>
        IEnumerable<string> LocalizableScripts { get; }
    }
}
```

> [!WARNING]
> The collection includes exact string matches, including whitespace. For example, both `"Tom"` and `"  Tom  "` would be collected as distinct entries. It is up to your implementation to handle trimming or unification if necessary.

## Runtime Translation
At runtime, accessing a tagged property on the interface will automatically attempt to return its localized version.

### Example
```c#
var characterInfo = Params.Get<ICharacterInfo>("Timmy");

// Returns the localized version of the DisplayName
string localizedDisplayName = characterInfo.DisplayName;

// Returns a translated script code.
string localizedScript = characterInfo.DialogScript;
myDialogManager.RunScript(localizedScript);
```
### Setup
To enable runtime translation, you must assign the translation delegates during application initialization. These delegates are invoked whenever a tagged property is accessed.

```c#
// Initialize your custom localization system
MyLocalizationManger myLocalizationManager = new ();

// Assign delegates to handle translation requests
ParameterLocalizationHandler.GlobalTranslateLocalizationKeyDelegate =
    localizationKey => myLocalizationManager.TranslateKey(localizationKey);

ParameterLocalizationHandler.GlobalTranslateLocalizableScriptDelegate =
    localizableScript => myLocalizationManager.TranslateScript(localizableScript);
```

If localization is not yet available or needed, you can simply return the original values.

```c#
// Localization disabled: return original untranslated strings
ParameterLocalizationHandler.GlobalTranslateLocalizationKeyDelegate =
    localizationKey => localizationKey;

ParameterLocalizationHandler.GlobalTranslateLocalizableScriptDelegate =
    localizableScript => localizableScript;
```
