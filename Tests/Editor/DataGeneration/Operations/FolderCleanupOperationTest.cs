using System.Collections.Generic;
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
        private string TempBackupPath => Path.Combine(new[] { "Assets", "Parameters", "_TestBackup" });

        private List<(string original, string backup)> _preservedDirectories;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _contextMock.GeneratedAssetDirectory.Returns(RootPath);

            // Move any real project directories that the operation would delete into a temp
            // backup so the test doesn't permanently destroy them.
            _preservedDirectories = new List<(string, string)>();
            PreserveDirectory(LegacyResourcesDirectory);
            var generatedAssetsRoot = Path.Combine(new[] { "Assets", "Parameters", "GeneratedAssets" });
            if (Directory.Exists(generatedAssetsRoot))
            {
                foreach (var dir in Directory.GetDirectories(generatedAssetsRoot, "*", SearchOption.TopDirectoryOnly))
                {
                    if (!RootPath.StartsWith(dir))
                        PreserveDirectory(dir);
                }
            }

            CleanupTestDirectories();
            Directory.CreateDirectory(LegacyResourcesDirectory);
            AssetDatabase.ImportAsset(LegacyResourcesDirectory);
            Directory.CreateDirectory(MiscDirectory1);
            AssetDatabase.ImportAsset(MiscDirectory1);
            Directory.CreateDirectory(MiscDirectory2);
            AssetDatabase.ImportAsset(MiscDirectory2);
        }

        private void PreserveDirectory(string path)
        {
            if (!Directory.Exists(path))
                return;
            if (!Directory.Exists(TempBackupPath))
            {
                Directory.CreateDirectory(TempBackupPath);
                AssetDatabase.ImportAsset(TempBackupPath);
            }
            var backupPath = Path.Combine(TempBackupPath, Path.GetFileName(path));
            AssetDatabase.MoveAsset(path, backupPath);
            _preservedDirectories.Add((path, backupPath));
        }

        private void CleanupTestDirectories()
        {
            if (Directory.Exists(LegacyResourcesDirectory))
                AssetDatabase.DeleteAsset(LegacyResourcesDirectory);
            if (Directory.Exists(MiscDirectory1))
                AssetDatabase.DeleteAsset(MiscDirectory1);
            if (Directory.Exists(MiscDirectory2))
                AssetDatabase.DeleteAsset(MiscDirectory2);
        }

        [TearDown]
        public void TearDown()
        {
            CleanupTestDirectories();

            if (_preservedDirectories != null)
            {
                foreach (var (original, backup) in _preservedDirectories)
                    AssetDatabase.MoveAsset(backup, original);
                _preservedDirectories = null;
            }
            if (Directory.Exists(TempBackupPath))
                AssetDatabase.DeleteAsset(TempBackupPath);
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
