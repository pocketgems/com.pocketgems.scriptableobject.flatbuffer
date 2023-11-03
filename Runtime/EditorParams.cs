#if UNITY_EDITOR
using System;
using System.Reflection;
using PocketGems.Parameters.AssetLoader;
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

            // find and create data loader
            if (s_cachedParameterDataLoader == null)
                s_cachedParameterDataLoader = FindSingleInterfaceImplementation(typeof(IParameterDataLoader));
            IParameterDataLoader dataLoader = (IParameterDataLoader)Activator.CreateInstance(s_cachedParameterDataLoader);

            Init(parameterManager, hotLoader, dataLoader);
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

        internal static Type FindSingleInterfaceImplementation(Type searchInterfaceType)
        {
            Type parameterDataLoaderType = null;

            AppDomain currentDomain = AppDomain.CurrentDomain;
            Assembly[] assemblies = currentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; i++)
            {
                var assembly = assemblies[i];
                // GetType() dies when iterating through BrotliSharpLib due to a struct being over 1MB
                // DynamicProxyGenAssembly2 can hold NSubstitute proxy interfaces which we don't want
                if (assembly.FullName.StartsWith("BrotliSharpLib") ||
                    assembly.FullName.StartsWith("DynamicProxyGenAssembly2")
                    )
                    continue;
                var types = assembly.GetTypes();
                for (int j = 0; j < types.Length; j++)
                {
                    var type = types[j];
                    if (type.IsAbstract)
                        continue;
                    var interfaces = type.GetInterfaces();
                    for (int k = 0; k < interfaces.Length; k++)
                    {
                        var interfaceType = interfaces[k];
                        if (interfaceType != searchInterfaceType)
                            continue;

                        if (parameterDataLoaderType == null)
                            parameterDataLoaderType = type;
                        else
                            Debug.LogError($"Found more than one implementation of {searchInterfaceType}");
                        break;
                    }
                }
            }

            return parameterDataLoaderType;
        }
    }
}
#endif
