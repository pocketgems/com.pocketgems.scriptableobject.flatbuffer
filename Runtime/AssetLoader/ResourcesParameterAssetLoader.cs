#if UNITY_2021_3_OR_NEWER
using System.IO;
using UnityEngine;

namespace PocketGems.Parameters.AssetLoader
{
    /// <summary>
    /// Parameter loader that loads assets from the Resources folder.
    /// </summary>
    public class ResourcesParameterAssetLoader : IParameterAssetLoader
    {
        /// <summary>
        ///  the parameter manager that assets were loaded into.
        /// </summary>
        public IMutableParameterManager ParameterManager { get; private set; }
        /// <inheritdoc cref="IParameterAssetLoader.Status"/>
        public ParameterAssetLoaderStatus Status { get; protected set; }

        protected readonly string ResourceDirectoryPath;
        protected IParameterDataLoader ParameterDataLoader;

        /// <summary>
        /// Constructor for the asset loader.
        /// </summary>
        /// <param name="resourceDirectoryPath">relative resources directory path used by Resources.Load where all of the .byte files are
        /// located.</param>
        public ResourcesParameterAssetLoader(string resourceDirectoryPath = ParameterConstants.GeneratedResourceDirectory)
        {
            Status = ParameterAssetLoaderStatus.NotStarted;
            ResourceDirectoryPath = resourceDirectoryPath;
        }

        /// <inheritdoc cref="IParameterAssetLoader.LoadData"/>
        public virtual void LoadData(IMutableParameterManager parameterManager, IParameterDataLoader parameterDataLoader)
        {
            Status = ParameterAssetLoaderStatus.Loading;
            ParameterManager = parameterManager;
            ParameterDataLoader = parameterDataLoader;

            // load byte file from bundle
            var resourceName = ParameterConstants.GeneratedAssetName;
            var filePath = Path.Combine(ResourceDirectoryPath, resourceName);
            TextAsset asset = Resources.Load<TextAsset>(filePath);
            if (asset == null)
            {
                Debug.LogError($"Load data for IMutableParameterManager, cannot find parameter file {filePath}");
                Status = ParameterAssetLoaderStatus.Failed;
                return;
            }

            parameterDataLoader.LoadData(parameterManager, asset.bytes);
            Status = ParameterAssetLoaderStatus.Loaded;
        }
    }
}
#endif
