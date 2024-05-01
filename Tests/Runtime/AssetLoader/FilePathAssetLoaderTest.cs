namespace PocketGems.Parameters.AssetLoader
{
    public class FilePathAssetLoaderTest : BaseParameterAssetLoaderTest
    {
        protected override IParameterAssetLoader CreateParameterAssetLoader()
        {
            var loader = new FilePathAssetLoader();
            FilePathAssetLoader.DirectoryPath = TestDirectoryPath;
            return loader;
        }
    }
}
