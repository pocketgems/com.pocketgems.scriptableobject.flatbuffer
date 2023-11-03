namespace PocketGems.Parameters.AssetLoader
{
    /// <summary>
    /// The asset loader loads parameter Unity assets into memory and passes the data to the parameter data loader
    /// to be loaded into memory.
    /// </summary>
    public interface IParameterAssetLoader
    {
        /// <summary>
        /// Function used to use loaded asset data into memory as byte[] and use the IParameterDataLoader to process
        /// the data and load it into the IMutableParameterManager.
        /// </summary>
        /// <param name="parameterManager">Parameter manager to load parameter data into.</param>
        /// <param name="parameterDataLoader">The parameter data loader used to pass asset byte data to.</param>
        void LoadData(IMutableParameterManager parameterManager, IParameterDataLoader parameterDataLoader);

        /// <summary>
        /// The status of the data loading.  This provides the flexibility in case the LoadData cannot complete
        /// after the initial LoadData call.
        /// </summary>
        ParameterAssetLoaderStatus Status { get; }
    }
}
