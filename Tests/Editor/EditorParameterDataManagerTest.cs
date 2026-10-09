using System.IO;
using NUnit.Framework;
using PocketGems.Parameters.DataGeneration.Util.Editor;
using UnityEditor;
using UnityEngine;

namespace PocketGems.Parameters.Editor
{
    public class EditorParameterDataManagerTest
    {
        private const string kAssetDirectory = "Assets/EditorParameterDataManagerTest";
        private const string kCacheDirectory = "EditorParameterDataManagerTest";
        private string _originalFilePath;

        [SetUp]
        public void SetUp()
        {
            _originalFilePath = ParameterGuidCache.FilePath;
            ParameterGuidCache.FilePath = Path.Combine(kCacheDirectory, ".param_guids");
            AssetDatabase.CreateFolder("Assets", "EditorParameterDataManagerTest");
        }

        [TearDown]
        public void TearDown()
        {
            ParameterGuidCache.FilePath = _originalFilePath;
            if (Directory.Exists(kCacheDirectory))
                Directory.Delete(kCacheDirectory, true);
            AssetDatabase.DeleteAsset(kAssetDirectory);
        }

        [Test]
        public void IsParameterFile_CSV()
        {
            Assert.IsTrue(EditorParameterDataManager.IsParameterFile("Assets/Parameters/LocalCSV/CurrencyInfo.csv"));
            Assert.IsFalse(EditorParameterDataManager.IsParameterFile("Assets/Other/CurrencyInfo.csv"));
        }

        [Test]
        public void IsParameterFile_OtherExtension()
        {
            Assert.IsFalse(EditorParameterDataManager.IsParameterFile("Assets/Parameters/LocalCSV/CurrencyInfo.txt"));
        }

        [Test]
        public void IsParameterFile_ExistingNonParameterAsset()
        {
            var path = CreateAsset("Mesh.asset");
            Assert.IsFalse(EditorParameterDataManager.IsParameterFile(path));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void IsParameterFile_DeletedAsset(bool isCachedParameter)
        {
            var path = CreateAsset("Mesh.asset");
            ParameterGuidCache.Replace(isCachedParameter ? new[] { AssetDatabase.AssetPathToGUID(path) } : new string[0]);

            AssetDatabase.DeleteAsset(path);

            Assert.AreEqual(isCachedParameter, EditorParameterDataManager.IsParameterFile(path));
        }

        // a file removed outside of Unity, such as by git pull, is found on the next refresh
        [TestCase(false)]
        [TestCase(true)]
        public void IsParameterFile_DeletedOnDisk(bool isCachedParameter)
        {
            var path = CreateAsset("Mesh.asset");
            ParameterGuidCache.Replace(isCachedParameter ? new[] { AssetDatabase.AssetPathToGUID(path) } : new string[0]);

            File.Delete(path);
            File.Delete($"{path}.meta");
            AssetDatabase.Refresh();

            Assert.AreEqual(isCachedParameter, EditorParameterDataManager.IsParameterFile(path));
        }

        [Test]
        public void IsParameterFile_DeletedAsset_NoCache()
        {
            var path = CreateAsset("Mesh.asset");
            AssetDatabase.DeleteAsset(path);

            Assert.IsTrue(EditorParameterDataManager.IsParameterFile(path));
        }

        // the old path of a moved asset isn't a parameter file - the new path is checked by its type
        [TestCase(false)]
        [TestCase(true)]
        public void IsParameterFile_MovedFromPath(bool isCachedParameter)
        {
            var path = CreateAsset("Mesh.asset");
            ParameterGuidCache.Replace(isCachedParameter ? new[] { AssetDatabase.AssetPathToGUID(path) } : new string[0]);

            Assert.IsEmpty(AssetDatabase.MoveAsset(path, $"{kAssetDirectory}/Moved.asset"));

            Assert.IsFalse(EditorParameterDataManager.IsParameterFile(path));
        }

        private static string CreateAsset(string fileName)
        {
            var path = $"{kAssetDirectory}/{fileName}";
            AssetDatabase.CreateAsset(new Mesh(), path);
            return path;
        }
    }
}
