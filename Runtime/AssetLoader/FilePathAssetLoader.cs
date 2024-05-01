using System.IO;
using UnityEngine;

namespace PocketGems.Parameters.AssetLoader
{
    /// <summary>
    /// Parameter loader that loads assets directly by path.
    /// </summary>
    public class FilePathAssetLoader : IParameterAssetLoader
    {
        /// <inheritdoc cref="IParameterAssetLoader.Status"/>
        public ParameterAssetLoaderStatus Status { get; protected set; }

        public static string DirectoryPath;

        /// <summary>
        /// Constructor for the asset loader.
        /// </summary>
        public FilePathAssetLoader()
        {
            Status = ParameterAssetLoaderStatus.NotStarted;
        }

        /// <inheritdoc cref="IParameterAssetLoader.LoadData"/>
        public virtual void LoadData(IMutableParameterManager parameterManager, IParameterDataLoader parameterDataLoader)
        {
            Status = ParameterAssetLoaderStatus.Loading;

            var resourceName = ParameterConstants.GeneratedAssetName + ParameterConstants.GeneratedParameterAssetFileExtension;
            var filePath = resourceName;
            if (!string.IsNullOrWhiteSpace(DirectoryPath))
            {
                filePath = Path.Combine(DirectoryPath, resourceName);
            }
            if (!File.Exists(filePath))
            {
                Debug.LogError($"Load data for IMutableParameterManager, cannot find parameter file {filePath}");
                Status = ParameterAssetLoaderStatus.Failed;
                return;
            }
            var bytes = File.ReadAllBytes(filePath);
            parameterDataLoader.LoadData(parameterManager, bytes);
            Status = ParameterAssetLoaderStatus.Loaded;
        }
    }
}
