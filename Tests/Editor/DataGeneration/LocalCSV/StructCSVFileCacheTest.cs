using NSubstitute;
using NUnit.Framework;
using PocketGems.Parameters.Common.Models.Editor;
using PocketGems.Parameters.Interface;

namespace PocketGems.Parameters.DataGeneration.LocalCSV.Editor
{
    public class StructCSVFileCacheTest : BaseCSVFileCacheTest
    {
        private interface ICacheATestStruct : IBaseStruct { }
        private interface ICacheBTestStruct : IBaseStruct { }
        private interface ICacheCTestStruct : IBaseStruct { }
        private IParameterStruct CacheAParameterStruct => CacheParameterStruct("CacheATestStruct");
        private IParameterStruct CacheBParameterStruct => CacheParameterStruct("CacheBTestStruct");
        private IParameterStruct CacheCParameterStruct => CacheParameterStruct("CacheCTestStruct");

        private IParameterStruct CacheParameterStruct(string baseName)
        {
            var parameterStruct = Substitute.For<IParameterStruct>();
            parameterStruct.BaseName.ReturnsForAnyArgs(baseName);
            return parameterStruct;
        }

        private StructCSVFileCache CreateCache(bool attemptLoadExistingOnLoad)
        {
            return new StructCSVFileCache(TestDirectoryName, attemptLoadExistingOnLoad);
        }

        [Test]
        public void Settings()
        {
            var cache = CreateCache(false);
            var csvFile = cache.Load<ICacheATestStruct>();
            Assert.IsFalse(csvFile.AttemptLoadExisting);
            Assert.IsFalse(csvFile.RequiresIdentifier);

            cache.AttemptLoadExistingOnLoad = true;
            Assert.IsTrue(cache.AttemptLoadExistingOnLoad);
            cache.ClearCache();

            var reloadedCsvFile = cache.Load<ICacheATestStruct>();
            Assert.IsTrue(reloadedCsvFile.AttemptLoadExisting);
            Assert.IsFalse(reloadedCsvFile.RequiresIdentifier);
        }

        [Test]
        public void Load()
        {
            var cache = CreateCache(false);
            var csvFile1 = cache.Load<ICacheATestStruct>();
            var csvFile2 = cache.Load(CacheBParameterStruct);
            var csvFile3 = cache.Load<ICacheCTestStruct>();
            Assert.IsTrue(cache.IsLoaded<ICacheATestStruct>());
            Assert.IsTrue(cache.IsLoaded<ICacheBTestStruct>());
            Assert.IsTrue(cache.IsLoaded<ICacheCTestStruct>());
            var loadedFiles = cache.LoadedFiles();
            Assert.AreEqual(3, loadedFiles.Count);
            Assert.AreEqual(csvFile1, loadedFiles["CacheATestStruct"]);
            Assert.AreEqual(csvFile2, loadedFiles["CacheBTestStruct"]);
            Assert.AreEqual(csvFile3, loadedFiles["CacheCTestStruct"]);

            // loading again will not do anything
            Assert.AreEqual(csvFile1, cache.Load(CacheAParameterStruct));
            Assert.AreEqual(csvFile2, cache.Load<ICacheBTestStruct>());
            Assert.AreEqual(csvFile3, cache.Load(CacheCParameterStruct));
            Assert.AreEqual(3, cache.LoadedFiles().Count);
        }

        [Test]
        public void Clear()
        {
            var cache = CreateCache(false);
            Assert.IsNotNull(cache.Load<ICacheATestStruct>());
            Assert.IsNotNull(cache.Load<ICacheBTestStruct>());
            Assert.IsNotNull(cache.Load(CacheCParameterStruct));
            Assert.IsTrue(cache.IsLoaded<ICacheATestStruct>());
            Assert.IsTrue(cache.IsLoaded<ICacheBTestStruct>());
            Assert.AreEqual(3, cache.LoadedFiles().Count);

            cache.ClearCache();
            Assert.IsFalse(cache.IsLoaded<ICacheATestStruct>());
            Assert.IsFalse(cache.IsLoaded<ICacheBTestStruct>());
            Assert.AreEqual(0, cache.LoadedFiles().Count);
        }

        [Test]
        public void LoadRowHistory()
        {
            var cache = CreateCache(false);
            // load various rows
            var csvRow1 = cache.LoadRow<ICacheATestStruct>("a");
            var csvRow2 = cache.LoadRow<ICacheATestStruct>("b");
            var csvRow3 = cache.LoadRow<ICacheBTestStruct>("c");
            Assert.IsNotNull(csvRow1);
            Assert.IsNotNull(csvRow2);
            Assert.IsNotNull(csvRow3);

            var loadRowHistory = cache.LoadRowHistory;
            Assert.AreEqual(2, loadRowHistory.Count);
            Assert.AreEqual(2, loadRowHistory["CacheATestStruct"].Count);
            Assert.IsTrue(loadRowHistory["CacheATestStruct"].Contains("a"));
            Assert.IsTrue(loadRowHistory["CacheATestStruct"].Contains("b"));
            Assert.AreEqual(1, loadRowHistory["CacheBTestStruct"].Count);
            Assert.IsTrue(loadRowHistory["CacheBTestStruct"].Contains("c"));

            // clear history
            cache.ClearLoadRowHistory();
            Assert.AreEqual(0, cache.LoadRowHistory.Count);

            // call load row again
            Assert.AreEqual(csvRow1, cache.LoadRow<ICacheATestStruct>("a"));
            Assert.AreEqual(1, loadRowHistory.Count);
            Assert.AreEqual(1, loadRowHistory["CacheATestStruct"].Count);
            Assert.IsTrue(loadRowHistory["CacheATestStruct"].Contains("a"));
        }
    }
}
