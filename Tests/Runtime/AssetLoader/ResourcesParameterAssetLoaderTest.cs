using NUnit.Framework;

namespace PocketGems.Parameters.AssetLoader
{
    public class ResourcesParameterAssetLoaderTest : BaseParameterAssetLoaderTest
    {
        protected override IParameterAssetLoader CreateParameterAssetLoader()
        {
            var loader = new ResourcesParameterAssetLoader(TestDirName);
            return loader;
        }

        [Test]
        public void LoadMainParameterFileOnly()
        {
            // these file shouldn't be loaded
            CreateByteFile("test1");
            CreateByteFile("test2");
            LoadParameterFile();
        }
    }
}
