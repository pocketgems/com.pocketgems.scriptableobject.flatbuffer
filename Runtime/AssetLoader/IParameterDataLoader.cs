namespace PocketGems.Parameters.AssetLoader
{
    /// <summary>
    /// Interface that loads data from byte array format and constructs IFlatBufferObjects
    /// to be loaded into the parameterManager.
    ///
    /// This is used by the generated code for a way for apps to load games from generated scripts.
    /// </summary>
    public interface IParameterDataLoader
    {
        /// <summary>
        /// Function that takes raw byte file data and populates the parameter manager with parameter data.
        /// </summary>
        /// <param name="parameterManager">The parameter manager to load parameters into.</param>
        /// <param name="bytes">The raw byte data from a parameter file.</param>
        void LoadData(IMutableParameterManager parameterManager, byte[] bytes);
    }
}
