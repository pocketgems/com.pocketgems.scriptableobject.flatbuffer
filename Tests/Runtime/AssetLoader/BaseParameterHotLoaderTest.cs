using NUnit.Framework;

namespace PocketGems.Parameters.AssetLoader
{
    public abstract class BaseParameterHotLoaderTest : BaseEditorAssetLoaderTest
    {
        [Test]
        public void HotLoader()
        {
            LoadParameterFile();
            var hotLoader = (IParameterHotLoader)ParameterAssetLoader;
            Assert.AreEqual(MockParameterManager, hotLoader.MutableParameterManager);
        }
    }
}
