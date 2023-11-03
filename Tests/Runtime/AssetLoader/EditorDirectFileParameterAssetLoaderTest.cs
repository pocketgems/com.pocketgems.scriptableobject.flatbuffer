namespace PocketGems.Parameters.AssetLoader
{
    public class EditorDirectFileParameterAssetLoaderTest : BaseParameterHotLoaderTest
    {
        protected override IParameterAssetLoader CreateParameterAssetLoader()
        {
            return new EditorDirectFileParameterAssetLoader(TestDirectoryPath);
        }
    }
}
