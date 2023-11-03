using NSubstitute;
using NUnit.Framework;
using PocketGems.Parameters.Interface;
using PocketGems.Parameters.Models;

namespace PocketGems.Parameters.Editor.LocalCSV
{
    public class InfoCSVFileCacheTest : BaseCSVFileCacheTest
    {
        private interface ICacheATestInfo : IBaseInfo { }
        private interface ICacheBTestInfo : IBaseInfo { }
        private interface ICacheCTestInfo : IBaseInfo { }

        private IParameterInfo CacheAParameterInfo => CacheParameterInfo("CacheATestInfo");
        private IParameterInfo CacheBParameterInfo => CacheParameterInfo("CacheBTestInfo");
        private IParameterInfo CacheCParameterInfo => CacheParameterInfo("CacheCTestInfo");

        private IParameterInfo CacheParameterInfo(string baseName)
        {
            var parameterStruct = Substitute.For<IParameterInfo>();
            parameterStruct.BaseName.ReturnsForAnyArgs(baseName);
            return parameterStruct;
        }

        private InfoCSVFileCache CreateCache(bool attemptLoadExistingOnLoad)
        {
            return new InfoCSVFileCache(TestDirectoryName, attemptLoadExistingOnLoad);
        }

        [Test]
        public void Settings()
        {
            var cache = CreateCache(false);
            var csvFile = cache.Load<ICacheATestInfo>();
            Assert.IsFalse(csvFile.AttemptLoadExisting);
            Assert.IsTrue(csvFile.RequiresIdentifier);

            cache.AttemptLoadExistingOnLoad = true;
            Assert.IsTrue(cache.AttemptLoadExistingOnLoad);
            cache.ClearCache();

            var reloadedCsvFile = cache.Load<ICacheATestInfo>();
            Assert.IsTrue(reloadedCsvFile.AttemptLoadExisting);
            Assert.IsTrue(reloadedCsvFile.RequiresIdentifier);
        }

        [Test]
        public void Load()
        {
            var cache = CreateCache(false);
            var csvFile1 = cache.Load<ICacheATestInfo>();
            var csvFile2 = cache.Load(CacheBParameterInfo);
            var csvFile3 = cache.Load<ICacheCTestInfo>();
            Assert.IsTrue(cache.IsLoaded<ICacheATestInfo>());
            Assert.IsTrue(cache.IsLoaded<ICacheBTestInfo>());
            Assert.IsTrue(cache.IsLoaded<ICacheCTestInfo>());
            var loadedFiles = cache.LoadedFiles();
            Assert.AreEqual(3, loadedFiles.Count);
            Assert.AreEqual(csvFile1, loadedFiles["CacheATestInfo"]);
            Assert.AreEqual(csvFile2, loadedFiles["CacheBTestInfo"]);
            Assert.AreEqual(csvFile3, loadedFiles["CacheCTestInfo"]);

            // loading again will not do anything
            Assert.AreEqual(csvFile1, cache.Load(CacheAParameterInfo));
            Assert.AreEqual(csvFile2, cache.Load<ICacheBTestInfo>());
            Assert.AreEqual(csvFile3, cache.Load(CacheCParameterInfo));
            Assert.AreEqual(3, cache.LoadedFiles().Count);
        }

        [Test]
        public void Clear()
        {
            var cache = CreateCache(false);
            Assert.IsNotNull(cache.Load<ICacheATestInfo>());
            Assert.IsNotNull(cache.Load<ICacheBTestInfo>());
            Assert.IsNotNull(cache.Load(CacheCParameterInfo));
            Assert.IsTrue(cache.IsLoaded<ICacheATestInfo>());
            Assert.IsTrue(cache.IsLoaded<ICacheBTestInfo>());
            Assert.AreEqual(3, cache.LoadedFiles().Count);

            cache.ClearCache();
            Assert.IsFalse(cache.IsLoaded<ICacheATestInfo>());
            Assert.IsFalse(cache.IsLoaded<ICacheBTestInfo>());
            Assert.AreEqual(0, cache.LoadedFiles().Count);
        }
    }
}
