#if UNITY_EDITOR
using System;
using PocketGems.Parameters.AssetLoader;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace PocketGems.Parameters
{
    public static class EditorParams
    {
        /// <summary>
        /// Public parameter manager to be queried in the editor.  If this returns null,
        /// call Init() to initialize a new one.
        /// </summary>

        public static IParameterManager ParameterManager
        {
            [ExcludeFromCoverage]
            get
            {
                if (s_parameterManager == null)
                    Init();
                return s_parameterManager;
            }
        }

        /// <summary>
        /// Internal getter for parameter manager.
        /// </summary>
        internal static IMutableParameterManager InternalParameterManager => s_parameterManager;

        /// <summary>
        /// The HotLoader to use to update the ParameterManager with changes for faster
        /// editor iteration.
        /// </summary>
        internal static IParameterHotLoader HotLoader => s_hotLoader;

        /// <summary>
        /// Creates a new parameter manager and populates the ParameterManager getter.
        /// </summary>
        [ExcludeFromCoverage]
        public static void Init()
        {
            // create parameter manager
            IMutableParameterManager parameterManager = new ParameterManager();

            // using this loader to be compatible with both addressable & resource asset locations
            IParameterHotLoader hotLoader = new EditorDirectFileParameterAssetLoader();
            IParameterDataLoader dataLoader = CreateParameterDataLoader();

            Init(parameterManager, hotLoader, dataLoader);

            ParameterLocalizationHandler.GlobalTranslateLocalizationKeyDelegate ??= localizationKey => localizationKey;
            ParameterLocalizationHandler.GlobalTranslateLocalizableScriptDelegate ??= localizableScript => localizableScript;
        }

        /// <summary>
        /// Destroy loaded instances.
        /// </summary>
        public static void Destroy()
        {
            s_parameterManager = null;
            s_hotLoader = null;
        }

        internal static void Init(IMutableParameterManager parameterManager,
            IParameterHotLoader hotLoader,
            IParameterDataLoader parameterDataLoader)
        {
            Destroy();
            hotLoader.LoadData(parameterManager, parameterDataLoader);
            if (hotLoader.Status != ParameterAssetLoaderStatus.Loaded)
            {
                Debug.LogError($"Unable to initialize {nameof(EditorParams)}, cannot find generated parameter data file.");
                return;
            }
            s_parameterManager = parameterManager;
            s_hotLoader = hotLoader;
        }

        private static IMutableParameterManager s_parameterManager;
        private static IParameterHotLoader s_hotLoader;
        private static Type s_cachedParameterDataLoader;

        [ExcludeFromCoverage]
        internal static IParameterDataLoader CreateParameterDataLoader()
        {
            // find and create data loader
            if (s_cachedParameterDataLoader == null)
                s_cachedParameterDataLoader = FindSingleInterfaceImplementation(typeof(IParameterDataLoader));
            return (IParameterDataLoader)Activator.CreateInstance(s_cachedParameterDataLoader);
        }

        /// <summary>
        /// Finds the single non-abstract implementation of <paramref name="searchInterfaceType"/> using
        /// Unity's prebuilt <see cref="TypeCache"/> - fast and does not scan every type in every assembly.
        /// Logs an error and returns the first match if more than one is found; returns null if none exist.
        /// </summary>
        internal static Type FindSingleInterfaceImplementation(Type searchInterfaceType)
        {
            Type implementationType = null;
            foreach (var type in TypeCache.GetTypesDerivedFrom(searchInterfaceType))
            {
                if (type.IsAbstract)
                    continue;

                if (implementationType == null)
                    implementationType = type;
                else
                    Debug.LogError($"Found more than one implementation of {searchInterfaceType}");
            }

            return implementationType;
        }
    }
}
#endif
