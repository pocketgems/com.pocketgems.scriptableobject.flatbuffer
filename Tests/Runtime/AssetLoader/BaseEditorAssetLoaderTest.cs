using NSubstitute;
using NUnit.Framework;

namespace PocketGems.Parameters.AssetLoader
{
    public abstract class BaseEditorAssetLoaderTest : BaseParameterAssetLoaderTest
    {
        [Test]
        public void LoadEditorParameterFiles()
        {
            CreateByteFile(ParameterConstants.GeneratedAssetName);
            CreateByteFile("test1");
            CreateByteFile("test2");

            Assert.AreEqual(ParameterAssetLoaderStatus.NotStarted, ParameterAssetLoader.Status);
            ParameterAssetLoader.LoadData(MockParameterManager, MockDataLoader);

            MockDataLoader.Received(3).LoadData(MockParameterManager, Arg.Any<byte[]>());
            Assert.AreEqual(ParameterAssetLoaderStatus.Loaded, ParameterAssetLoader.Status);
        }
    }
}
