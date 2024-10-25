# Changelog
All package updates & migration steps will be listed in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [4.1.0] - 2024-10-28
### Added
- `ParameterReference<T>` implements `IComparable`
- New interface attributes
  - `ParameterAttachFieldAttributeAttribute`
  - `ParameterFoldOutAttribute`
  - `ParameterTooltipAttribute`
  - `ParameterHeaderAttribute`
### Changed
- deprecated `AttachFieldAttributeAttribute`

## [4.0.1] - 2024-10-07
### Fixed
- Issue where validation is ran on the Scriptable Object inspector while the editor is playing and parameter manager isn't set up.

## [4.0.0] - 2024-07-17
### Changed
- Editor code organization to support external code generation.
  - Break editor assembly into smaller modular assemblies.
  - Updated all editor namespaces.
  - Generalized various pathing code to make it more robust.
### Fixed
- Code gen overwrites git conflicts in AssemblyInfo
- Console logged parameter struct directory path after code generation
- Disabling code gen on editor loading should still check to generate data

## [3.9.0] - 2024-07-01
### Added
- Additional checks to prevent declaring references with `IBaseInfo` and `IBaseStruct`
### Changed
- Printing info at runtime will print interface name & identifier
- Updated README about inheritance

## [3.8.6] - 2024-06-06
### Fixed
- Missing `using` for `DateTime` & `TimeSpan` usage in structs
- `DateTime` & `TimeSpan` drawer rendering when nested in structs

## [3.8.5] - 2024-05-28
### Fixed
- `ParameterStructReference<>.Struct` to be thread safe.

## [3.8.4] - 2024-05-22
### Fixed
- `ParameterReference<>.Info` to be thread safe.

## [3.8.3] - 2024-05-17
### Fixed
- `ParameterReferenceDrawer` edge case where it shows "CANNOT DISPLAY" when it should be able to.

## [3.8.2] - 2024-05-10
### Fixed
- missing meta file

## [3.8.1] - 2024-05-10
### Fixed
- `ParameterScriptableObject` drawer depth limitation removed to blocking drawers from opening.

## [3.8.0] - 2024-05-07
### Changed
- Replaced FileAssetLoader with EmbeddedResourceAssetLoader for Non-Unity environment

## [3.7.1] - 2024-05-03
### Fixed
- Fix assembly name used in parameter reference drawer

## [3.7.0] - 2024-05-03
### Added
- Ordering in the Scriptable Object create asset menu.

## [3.6.0] - 2024-04-30
### Added
- Support code for building the parameters runtime outside of Unity
- `FilePathAssetLoader` for non Unity loading
- Method `IBaseInfo Get(string identifier, Type type)` to the `ParameterManager` and `Params`

## [3.5.0] - 2024-02-21
### Added
- Support for displaying only the correct sub class of ParameterScriptableObjects in Inspector

## [3.4.1] - 2023-12-18
### Added
- More details in `README` about renaming fields.

## [3.4.0] - 2023-11-08
### Added
- `IReadOnlyList` support for:
  - `Color`
  - `Vector2`
  - `Vector2Int`
  - `Vector3`
  - `Vector3Int`
- Support for `DateTime` and `TimeSpan` properties in interfaces
- Check to prevent invalid keyword property name usage in interface
### Changed
- Rename internal class names.
### Fixed
- Corner case in Color CSV syncing

## [3.3.2] - 2023-11-03
### Changed
- Update Author in `package.json`

## [3.3.1] - 2023-11-03
### Changed
- Updates usages of the loose term "parameter" in the README to be more explicit
### Fixed
- Typo in the README

## [3.3.0] - 2023-10-19
### Added
- More details for adding & renaming info interfaces
### Changed
- Moved package name to constant
- Structure of generated Scriptable Object so Unity isn't confused when renaming is done
### Fixed
- Spelling mistakes in READMEs

## [3.2.0] - 2023-10-11
### Added
- Support for ushort, uint and ulong property types

## [3.1.1] - 2023-08-23
### Fixed
- Conflicting key name for application launching.

## [3.1.0] - 2023-08-12
### Added
- More high level details in README
- Open source license
- Github pull request:
 - Template
 - Workflow
### Fixed
- File cleanup

## [3.0.1] - 2023-08-11
### Added
- More details to `README.md`
### Changed
- Broke out `README.md` into smaller more manageable pages.
- Cleaned up license organization.

## [3.0.0] - 2023-08-10
### Changed
- Minimum Unity version to `2021.3`.
- Updated `csvhelper` to use .net standard 2.1 for better LTS2022 compatibility.
- Rename `ParameterManager.GetWitGUID` to `GetWithGUID`
### Fixed
- Errors thrown during code compilation due to source generators in Unity 2022
- Various compilation warnings
### Removed
- Dependency internal Pocket Gems packages
- `[Obsolete]` APIs
  - ParameterReference<T>.GUID
  - ParameterManager.Remove<T>()
  - ParameterManager.RemoveStruct<T>()
  - CSVValueConverter.Enum

## [2.8.0] - 2023-08-08
### Added
- Struct workflow support
- `IReadOnlyList<>` support for Enums
- Flag to disable auto parameter validation
- Convenience save & hot-load button in Parameter Scriptable Object inspector
- Safeguards
 - Catching accidental/partial disabled interface implementations
 - Catch missing data during inspector validation
### Changed
- Disabling interface implementations doesn't delete files (less file thrash)
- Only dispatch code generation when the Unity Editor has focus

## [2.7.0] - 2023-07-06
### Added
- More robust data compatability checking and error message.

## [2.6.1] - 2023-06-13
### Added
- Minimum validation window size so error messages are not missed.
### Changed
- Always re-generate the whole flatbuffer when in packed addressables mode for uploading
### Fixed
- Non re-generating code when base interface changes

## [2.6.0] - 2023-03-09
### Add
- Support for `AssetReferenceAtlasedSprite` and `IReadOnlyList<AssetReferenceAtlasedSprite>` property type in interfaces.
### Changed
- Improved error message when validation attribute is not compatible with property assigned.

## [2.5.0] - 2023-02-24
### Add
- Data validation support.
- Multiple Scriptable Object inspector editing support.
- Additional unit tests to increase coverage over critical code.
- `EditorParams` to access a ParameterManager from the editor.
### Fixed
- Prevent unneeded code & data checks upon editor play.
- Improved performance with CSV writing.
- Improved performance with Scriptable Object loading.

## [2.4.1] - 2023-01-30
### Add
- Option to disable Scriptable Object Auto Save in the Parameters Config window.
### Changed
- Scriptable Object auto save delay is now 5 seconds.
### Fixed
- Updating Scriptable Object from CSV performance.
- Autosaving didn't work for Scriptable Objects outside the original Scriptable Objects folder.

## [2.4.0] - 2023-01-19
### Added
- Addressables support for asset file.
### Fixed
- Various code warnings.
- `README.md` links.

## [2.3.1] - 2023-01-05
### Fixed
- Deleting Scriptable Objects didn't regenerate the FlatBuffer data or CSV.

## [2.3.0] - 2023-01-04
### Add
- `ParameterScriptableObject` inspector provides a button to open the CSV.
- Expose a method in the `ParameterReferenceDrawer` to allow overrides.
- Caching of the commonly accessed `Identifier` property from FlatBuffers.
### Changed
- Getters for string properties default to empty string for predictable results.  This prevents Unity default behavior where new fields default to null until the object is re-serialized and saves the string as `""`.
- Allow Scriptable Objects to live anywhere in the project.

## [2.2.1] - 2022-11-30
### Fixed
- Improved `ScriptableObjectUtil` functions to be more robust.

## [2.2.0] - 2022-11-30
### Added
- API to support A/B testing service
  - Additional APIs to apply json overrides to the `IMutableParameterManager`
  - Added `HasGetBeenCalled` getter to the `IParameterManager`
  - Added `IsGettingSafe()` API to `Params` to safe guard parameter changes
- Check for valid addressable when updating from CSVs.
- `LocalizedString.ToString()` returns the localized string.  This helps with accidental usage of the `LocalizedString` directly in string construction.
### Changed
- Hashing for detecting coding changes also takes into account defined Enum values.
- Enum values are stored as `long` instead of `int` in the FlatBuffer.
- Update package dependency for unit test fix.

## [2.1.0] - 2022-10-26
### Added
- Support to expand `ParameterReference<>` drawers in the inspector to view & make modifications.
- Added a `New` button to the `ParameterReference<>` drawers to quickly create a Scriptable Object from the object field.
### Fixed
- Auto saving of parameter scriptable objects on windows.
- Reduce logging too much to the console which caused Unity to error.

## [2.0.1] - 2022-09-26
### Changed
- Make it easier to copy and paste csvs that are still compatible from older versions.
### Fixed
- Poorly formatted generated code whitespace & newlines.
- Bug with csv syncing: mismatch trying to match new row re-using an existing identifier (renamed for another row).
- Issue with accurately reporting correct error fields for csv syncing errors.

## [2.0.0] - 2022-09-22
### Added
- Parameters are authored and persisted as Scriptable Objects
- Workflow to modify Scriptable Objects via CSVs
### Removed
- old Param V1 CSV workflow for authoring parameters

## [1.0.0] - 2022-08-08
### Added
- Setup Parameters as a standalone package from an internal package.
