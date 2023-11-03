namespace PocketGems.Parameters.AssetLoader
{
    public class EditorResourcesParameterAssetLoaderTest : BaseParameterHotLoaderTest
    {
        protected override IParameterAssetLoader CreateParameterAssetLoader()
        {
            var loader = new EditorResourcesParameterAssetLoader(TestDirName);
            loader.PromptEditorMessage = false;
            loader.EditorResourceDirectoryPath = TestDirectoryPath;
            return loader;
        }
    }
}
