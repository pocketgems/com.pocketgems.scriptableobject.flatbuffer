using System;
using System.IO;
using NUnit.Framework;

namespace PocketGems.Parameters.DataGeneration.Util.Editor
{
    public class ParameterGuidCacheTest
    {
        private const string kDirectoryName = "ParameterGuidCacheTest";
        private string _originalFilePath;
        private string _filePath;

        [SetUp]
        public void SetUp()
        {
            _originalFilePath = ParameterGuidCache.FilePath;
            _filePath = Path.Combine(kDirectoryName, ".param_guids");
            ParameterGuidCache.FilePath = _filePath;
        }

        [TearDown]
        public void TearDown()
        {
            ParameterGuidCache.FilePath = _originalFilePath;
            if (Directory.Exists(kDirectoryName))
                Directory.Delete(kDirectoryName, true);
        }

        [Test]
        public void NoFile_MightBeParameter()
        {
            Assert.IsTrue(ParameterGuidCache.MightBeParameter("a"));
        }

        [Test]
        public void Replace()
        {
            ParameterGuidCache.Add(new[] { "old" });
            ParameterGuidCache.Replace(new[] { "a" });

            Assert.IsTrue(ParameterGuidCache.MightBeParameter("a"));
            Assert.IsFalse(ParameterGuidCache.MightBeParameter("b"));
            Assert.IsFalse(ParameterGuidCache.MightBeParameter("old"));
            CollectionAssert.AreEquivalent(new[] { "a" }, File.ReadAllLines(_filePath));
        }

        [Test]
        public void Add()
        {
            ParameterGuidCache.Replace(new[] { "a" });
            ParameterGuidCache.Add(new[] { "b" });

            Assert.IsTrue(ParameterGuidCache.MightBeParameter("a"));
            Assert.IsTrue(ParameterGuidCache.MightBeParameter("b"));
            CollectionAssert.AreEquivalent(new[] { "a", "b" }, File.ReadAllLines(_filePath));
        }

        [Test]
        public void Add_NoFile()
        {
            ParameterGuidCache.Add(new[] { "a" });

            Assert.IsTrue(ParameterGuidCache.MightBeParameter("a"));
            Assert.IsFalse(ParameterGuidCache.MightBeParameter("b"));
            CollectionAssert.AreEquivalent(new[] { "a" }, File.ReadAllLines(_filePath));
        }

        [Test]
        public void Add_FileDeleted_WritesAllGuids()
        {
            ParameterGuidCache.Replace(new[] { "a" });
            File.Delete(_filePath);

            ParameterGuidCache.Add(new[] { "b" });

            CollectionAssert.AreEquivalent(new[] { "a", "b" }, File.ReadAllLines(_filePath));
        }

        [Test]
        public void Add_ExistingGuid_DoesNotWrite()
        {
            ParameterGuidCache.Replace(new[] { "a" });
            var writeTime = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            File.SetLastWriteTimeUtc(_filePath, writeTime);

            ParameterGuidCache.Add(new[] { "a" });

            Assert.AreEqual(writeTime, File.GetLastWriteTimeUtc(_filePath));
        }

        [Test]
        public void LoadsFromFile()
        {
            ParameterGuidCache.Replace(new[] { "a" });

            // setting the path drops the loaded guids, like a domain reload
            ParameterGuidCache.FilePath = _filePath;

            Assert.IsTrue(ParameterGuidCache.MightBeParameter("a"));
            Assert.IsFalse(ParameterGuidCache.MightBeParameter("b"));
        }

        [Test]
        public void Replace_CreatesDirectory()
        {
            Assert.IsFalse(Directory.Exists(kDirectoryName));

            ParameterGuidCache.Replace(new[] { "a" });

            Assert.IsTrue(File.Exists(_filePath));
        }
    }
}
