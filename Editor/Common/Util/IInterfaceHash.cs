namespace PocketGems.Parameters.Common.Util.Editor
{
    public interface IInterfaceHash
    {
        string AssemblyInfoHash { get; set; }
        string AssemblyInfoEditorHash { get; set; }
        string GeneratedDataHash { get; set; }
        string GeneratedDataLoaderHash { get; }
    }
}
