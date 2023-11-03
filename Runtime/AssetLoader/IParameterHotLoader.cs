namespace PocketGems.Parameters.AssetLoader
{
    internal interface IParameterHotLoader : IParameterAssetLoader
    {
        /// <summary>
        /// API used by editor tools to hot load parameters into the app at runtime.
        /// </summary>
        /// <param name="editorFilePath">The file path to the FlatBuffer to load.</param>
        void HotLoadResource(string editorFilePath);

        /// <summary>
        /// Mutable parameter manger loading data into
        /// </summary>
        IMutableParameterManager MutableParameterManager { get; }
    }
}
