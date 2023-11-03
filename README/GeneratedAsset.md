# Generated Asset <!-- omit in toc -->

- [Location](#location)
- [Loading](#loading)
  - [Addressable](#addressable)
  - [Resource](#resource)

## Location
The final asset that holds all Scriptable Object data is generated as a `Parameter.bytes` [TextAsset](https://docs.unity3d.com/ScriptReference/TextAsset.html).  It is located in a nested subdirectory under the directory `Assets/Parameters/GeneratedAssets`.   This folder is git ignored since it is a locally generated directory.

## Loading
There are two ways the asset can be loaded.  This loading is handled automatically by the initial setup in [One Time Setup](Setup.md).

### Addressable
If the [`com.unity.addressables`](https://docs.unity3d.com/Packages/com.unity.addressables@latest/index.html) package is included in the project, the asset will be generated as an Addressable Asset.

The asset is added under the addressable group named after the package name.  This file is source controlled and addressables schemas can be added/removed/modified for the game's addressable organization needs.

### Resource
If the [`com.unity.addressables`](https://docs.unity3d.com/Packages/com.unity.addressables@latest/index.html) package is **NOT** included in the project, the asset will live under a `Resources` folder and loaded as a [Unity Resource](https://docs.unity3d.com/Manual/SpecialFolders.html).

