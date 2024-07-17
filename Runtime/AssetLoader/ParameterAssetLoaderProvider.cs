using UnityEngine.TestTools;

namespace PocketGems.Parameters.AssetLoader
{
    /// <summary>
    /// Provider class that create the appropriate parameter asset loader to use.
    /// </summary>
    [ExcludeFromCoverage]
    public static class ParameterAssetLoaderProvider
    {
#if UNITY_EDITOR
        /// <summary>
        /// Static reference to the current loader so parameter hot loading can be done through the active loader.
        /// </summary>
        internal static IParameterHotLoader RunningHotLoader;
#endif

        /// <summary>
        /// Creates creates and returns the appropriate IParameterAssetLoader to use.
        /// </summary>
        /// <returns>The loader to use.</returns>
        public static IParameterAssetLoader CreateParameterAssetLoader()
        {
#if UNITY_EDITOR && UNITY_2021_3_OR_NEWER
#if ADDRESSABLE_PARAMS
            RunningHotLoader = new EditorAddressablesParameterAssetLoader();
#else
            RunningHotLoader = new EditorResourcesParameterAssetLoader();
#endif
            return RunningHotLoader;
#elif UNITY_2021_3_OR_NEWER
#if ADDRESSABLE_PARAMS
            return new AddressablesParameterAssetLoader();
#else
            return new ResourcesParameterAssetLoader();
#endif
#else
            return new AssemblyManifestResourceAssetLoader();
#endif
        }
    }
}
