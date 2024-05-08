#if !UNITY_2021_3_OR_NEWER
using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace PocketGems.Parameters.AssetLoader
{
    /// <summary>
    /// Parameter loader that loads assets as an Embedded Resource from a target assembly.
    /// </summary>
    public class AssemblyManifestResourceAssetLoader : IParameterAssetLoader
    {
    
        /// <inheritdoc cref="IParameterAssetLoader.Status"/>
        public ParameterAssetLoaderStatus Status { get; protected set; }
        
        /// <summary>
        /// Assembly to load resource from.
        /// </summary>
        public static Assembly? Assembly;
        
        /// <summary>
        /// Name of Resource to Load.
        /// </summary>
        public static string ResourceName;

        /// <summary>
        /// Constructor for the asset loader.
        /// </summary>
        public AssemblyManifestResourceAssetLoader()
        {
            Status = ParameterAssetLoaderStatus.NotStarted;
        }

        /// <inheritdoc cref="IParameterAssetLoader.LoadData"/>
        public virtual void LoadData(IMutableParameterManager parameterManager, IParameterDataLoader parameterDataLoader)
        {
            Status = ParameterAssetLoaderStatus.Loading;
            
            if (Assembly == null)
            {
                Debug.LogError("No Assembly specified to load from");
                Status = ParameterAssetLoaderStatus.Failed;
                return;
            }
            if (string.IsNullOrEmpty(ResourceName))
            {
                Debug.LogError("No resource specified to load.");
                Status = ParameterAssetLoaderStatus.Failed;
                return;
            }

            try
            {
                byte[] bytes;
                using(Stream stream = Assembly.GetManifestResourceStream(ResourceName))
                using(MemoryStream mStream = new MemoryStream())
                {
                    if (stream == null)
                    {
                        Debug.LogError(
                            $"Could not find Resource {ResourceName} in Assembly {Assembly.FullName}");
                        Status = ParameterAssetLoaderStatus.Failed;
                        return;
                    }
                    stream.CopyTo(mStream);
                    bytes = mStream.ToArray();
                }
                parameterDataLoader.LoadData(parameterManager, bytes);
                Status = ParameterAssetLoaderStatus.Loaded;
            } catch(Exception e)
            {
                Debug.LogError("Failed: " + e.Message);
                Status = ParameterAssetLoaderStatus.Failed;
            }
        }
    }
}


#endif
