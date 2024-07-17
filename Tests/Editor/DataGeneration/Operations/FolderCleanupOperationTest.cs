using System.IO;
using NSubstitute;
using NUnit.Framework;
using PocketGems.Parameters.Common.Operation.Editor;
using PocketGems.Parameters.Common.Operations.Editor;
using PocketGems.Parameters.DataGeneration.Operation.Editor;
using UnityEditor;

namespace PocketGems.Parameters.DataGeneration.Operations.Editor
{
    public class FolderCleanupOperationTest : BaseOperationTest<IDataOperationContext>
    {
        private string RootPath => Path.Combine(new[] { "Assets", "Parameters", "GeneratedAssets", "SomeFolder" });
        private string LegacyResourcesDirectory => Path.Combine(new[] { "Assets", "Parameters", "Resources" });
        private string MiscDirectory1 => Path.Combine(new[] { "Assets", "Parameters", "GeneratedAssets", "Blah1" });
        private string MiscDirectory2 => Path.Combine(new[] { "Assets", "Parameters", "GeneratedAssets", "Blah2" });

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _contextMock.GeneratedAssetDirectory.Returns(RootPath);

            TearDown();
            Directory.CreateDirectory(LegacyResourcesDirectory);
            AssetDatabase.ImportAsset(LegacyResourcesDirectory);
            Directory.CreateDirectory(MiscDirectory1);
            AssetDatabase.ImportAsset(MiscDirectory1);
            Directory.CreateDirectory(MiscDirectory2);
            AssetDatabase.ImportAsset(MiscDirectory2);
        }

        [TearDown]
        public void TearDown()
        {
            // delete test folders
            if (Directory.Exists(LegacyResourcesDirectory))
                AssetDatabase.DeleteAsset(LegacyResourcesDirectory);
            if (Directory.Exists(MiscDirectory1))
                AssetDatabase.DeleteAsset(MiscDirectory1);
            if (Directory.Exists(MiscDirectory2))
                AssetDatabase.DeleteAsset(MiscDirectory2);
        }

        [Test]
        public void FolderRemoval()
        {
            Assert.IsTrue(Directory.Exists(LegacyResourcesDirectory));
            Assert.IsTrue(Directory.Exists(MiscDirectory1));
            Assert.IsTrue(Directory.Exists(MiscDirectory2));
            var operation = new FolderCleanupOperation();
            operation.Execute(_contextMock);
            AssertExecute(operation, OperationState.Finished);
            Assert.IsFalse(Directory.Exists(LegacyResourcesDirectory));
            Assert.IsFalse(Directory.Exists(MiscDirectory1));
            Assert.IsFalse(Directory.Exists(MiscDirectory2));
        }
    }
}
