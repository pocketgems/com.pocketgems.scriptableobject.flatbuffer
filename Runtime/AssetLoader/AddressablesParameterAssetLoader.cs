#if ADDRESSABLE_PARAMS && UNITY_2021_3_OR_NEWER
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.TestTools;

namespace PocketGems.Parameters.AssetLoader
{
    /// <summary>
    /// Parameter Asset Loader that loads from addressables
    /// </summary>
    [ExcludeFromCoverage]
    internal class AddressablesParameterAssetLoader : IParameterAssetLoader
    {
        /// <inheritdoc/>
        public ParameterAssetLoaderStatus Status { get; protected set; }

        protected IMutableParameterManager ParameterManager;
        protected IParameterDataLoader ParameterDataLoader;
        private readonly string _parameterAddress;
        private AsyncOperationHandle<TextAsset> _asyncOperationHandle;

        private byte[] _parameterData;

        /// <summary>
        /// General constructor
        /// </summary>
        /// <param name="parameterAddress">address of the parameter file</param>
        public AddressablesParameterAssetLoader(string parameterAddress = null)
        {
            _parameterAddress = parameterAddress ?? ParameterConstants.Addressables.GeneratedAssetAddress;
            Status = ParameterAssetLoaderStatus.NotStarted;
        }

        /// <inheritdoc/>
        public void LoadData(IMutableParameterManager parameterManager, IParameterDataLoader parameterDataLoader)
        {
            Status = ParameterAssetLoaderStatus.Loading;
            ParameterManager = parameterManager;
            ParameterDataLoader = parameterDataLoader;
            _asyncOperationHandle = Addressables.LoadAssetAsync<TextAsset>(_parameterAddress);
            _asyncOperationHandle.Completed += LoadOperationCompleted;
        }

        private void LoadOperationCompleted(AsyncOperationHandle<TextAsset> operationHandle)
        {
            if (operationHandle.Status == AsyncOperationStatus.Failed)
            {
                Debug.LogError($"Unable to load assets with address: {_parameterAddress}");
                Status = ParameterAssetLoaderStatus.Failed;
            }
            else
            {
                _parameterData = operationHandle.Result.bytes;
                LoadData();
                Status = ParameterAssetLoaderStatus.Loaded;
            }

            Addressables.Release(_asyncOperationHandle);
        }

        protected virtual void LoadData()
        {
            ParameterDataLoader.LoadData(ParameterManager, _parameterData);
        }
    }
}
#endif
