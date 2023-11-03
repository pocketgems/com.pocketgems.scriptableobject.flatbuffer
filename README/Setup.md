# Setup <!-- omit in toc -->

- [Adding Package](#adding-package)
  - [Git URL](#git-url)
  - [Embedded](#embedded)
- [Editor Setup](#editor-setup)
  - [Created Folders \& Files](#created-folders--files)
  - [Git Ignore](#git-ignore)
- [Runtime Initalization](#runtime-initalization)


## Adding Package
This package can be added to Unity in two ways.

### Git URL
Add the package through [Package Manager](https://docs.unity3d.com/Manual/upm-ui.html) with it's git url.  Follow Unity's instructions [here](https://docs.unity3d.com/Manual/upm-ui-giturl.html) and the URL of this repository.

### Embedded
This repository can be embeded under the `Packages` folder in the Unity project (as raw files or a git submodule).  Read [here](https://docs.unity3d.com/Manual/CustomPackages.html#EmbedMe) for more information.

## Editor Setup
Upon code compilation, any files & folders needed for the package are auto created.  These files should be committed to source control.

### Created Folders & Files
The following folder structure will be created on initial setup.

```bash
Assets/
├─ Parameters/
│  ├─ GeneratedCode/ (*contains generated files - DO NOT MODIFY manually*)
│  │  ├─ AssemblyInfo.cs
│  │  ├─ GeneratedParameters.asmdef
│  │  ├─ Editor/
│  │  │  ├─ AssemblyInfo.cs
│  │  │  ├─ GeneratedParameters.Editor.asmdef
│  ├─ Interfaces/ (*contains interface & enums defined by developers*)
│  │  ├─ AssemblyInfo.cs
│  │  ├─ ParameterInterface.asmdef
```

### Git Ignore
Required lines will be added to the `.gitignore`.

## Runtime Initalization
The `Setup()` method needs to be called on app startup (or at the latest, before using any `Params` APIs.).  This will return a loader.
```c#
var loader = ParamsSetup.Setup();
```

The params are loaded asynchronously.  The `Status` property in the loader will let be `Success` upon successful loading.  Once it's completed, calls to `Params.Get<>()` can be utilized.
```c#
bool isDone = parameterDataLoader.Status == Success;
```
