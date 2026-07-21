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

        [Test]
        public void LoadEditorParameterFilesWithIterationSubfolder()
        {
            // Mirrors the production layout: the main file lives at the root while the editor-only
            // iteration files live in the Unity-ignored subfolder and are found via a recursive search.
            CreateByteFile(ParameterConstants.GeneratedAssetName);
            CreateByteFileInSubDirectory(ParameterConstants.GeneratedAsset.IterationDirectoryName, "test1");
            CreateByteFileInSubDirectory(ParameterConstants.GeneratedAsset.IterationDirectoryName, "test2");

            Assert.AreEqual(ParameterAssetLoaderStatus.NotStarted, ParameterAssetLoader.Status);
            ParameterAssetLoader.LoadData(MockParameterManager, MockDataLoader);

            MockDataLoader.Received(3).LoadData(MockParameterManager, Arg.Any<byte[]>());
            Assert.AreEqual(ParameterAssetLoaderStatus.Loaded, ParameterAssetLoader.Status);
        }
    }
}
