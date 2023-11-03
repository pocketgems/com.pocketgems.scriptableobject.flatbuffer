using System.IO;
using System.Text.RegularExpressions;
using NSubstitute;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace PocketGems.Parameters.AssetLoader
{
    public abstract class BaseParameterAssetLoaderTest
    {
        protected string TestDirName;
        protected string TestDirectoryPath;
        protected IParameterAssetLoader ParameterAssetLoader;
        protected IMutableParameterManager MockParameterManager;
        protected IParameterDataLoader MockDataLoader;
        private string _rootDirectoryPath;

        [SetUp]
        public virtual void SetUp()
        {
            TestDirName = "AssetLoaderTest";
            _rootDirectoryPath = Path.Combine(new[] { "Assets", TestDirName });
            TestDirectoryPath = Path.Combine(new[] { _rootDirectoryPath, "Resources", TestDirName });
            if (Directory.Exists(_rootDirectoryPath))
                AssetDatabase.DeleteAsset(_rootDirectoryPath);
            Directory.CreateDirectory(TestDirectoryPath);

            MockParameterManager = Substitute.For<IMutableParameterManager>();
            MockDataLoader = Substitute.For<IParameterDataLoader>();
            ParameterAssetLoader = CreateParameterAssetLoader();
        }

        [TearDown]
        public virtual void TearDown()
        {
            if (Directory.Exists(_rootDirectoryPath))
                AssetDatabase.DeleteAsset(_rootDirectoryPath);
        }

        protected abstract IParameterAssetLoader CreateParameterAssetLoader();

        protected void CreateByteFile(string fileName)
        {
            var ext = ParameterConstants.GeneratedParameterAssetFileExtension;
            var filePath = Path.Combine(TestDirectoryPath, $"{fileName}{ext}");
            File.WriteAllText(filePath, "");
            AssetDatabase.ImportAsset(filePath);
        }

        [Test]
        public void NoParameterFile()
        {
            LogAssert.Expect(LogType.Error, new Regex(".*"));
            Assert.AreEqual(ParameterAssetLoaderStatus.NotStarted, ParameterAssetLoader.Status);
            ParameterAssetLoader.LoadData(MockParameterManager, MockDataLoader);
            Assert.AreEqual(ParameterAssetLoaderStatus.Failed, ParameterAssetLoader.Status);
        }

        [Test]
        public void LoadParameterFile()
        {
            CreateByteFile(ParameterConstants.GeneratedAssetName);

            Assert.AreEqual(ParameterAssetLoaderStatus.NotStarted, ParameterAssetLoader.Status);
            ParameterAssetLoader.LoadData(MockParameterManager, MockDataLoader);

            MockDataLoader.Received(1).LoadData(MockParameterManager, Arg.Any<byte[]>());
            Assert.AreEqual(ParameterAssetLoaderStatus.Loaded, ParameterAssetLoader.Status);
        }
    }
}
