namespace PocketGems.Parameters.Common.Models.Editor
{
    public interface IParameterStruct : IParameterInterface
    {
        /// <summary>
        /// Generated Struct that implements this interface and holds data for a Scriptable Object.
        /// </summary>
        /// <param name="includeExtension">if true, includes the file extension</param>
        /// <returns>the struct name</returns>
        public string StructName(bool includeExtension);
    }
}
