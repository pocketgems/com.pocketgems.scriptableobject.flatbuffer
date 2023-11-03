using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace PocketGems.Parameters.Util
{
    public class UnusedFileRemoverTest
    {
        private const string TestDirectoryName = "TestDirectory";
        private const string Test1 = "Test1.cs";
        private const string Test1Meta = "Test1.cs.meta";
        private const string Test2 = "Test2.cs";
        private const string Test2Meta = "Test2.cs.meta";
        private const string Test3 = "Test3.cs";
        private const string Test3Meta = "Test3.cs.meta";
        private UnusedFileRemover _remover;

        [SetUp]
        public void SetUp()
        {
            TearDown();

            Directory.CreateDirectory(TestDirectoryName);
            CreateFile(Test1);
            CreateFile(Test1Meta);
            CreateFile(Test2);
            CreateFile(Test2Meta);
            CreateFile(Test3);
            CreateFile(Test3Meta);

            _remover = new UnusedFileRemover(TestDirectoryName);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(TestDirectoryName))
                Directory.Delete(TestDirectoryName, true);
        }

        private void CreateFile(string filename)
        {
            File.WriteAllText(Path.Combine(TestDirectoryName, filename), "");
        }

        private void AssertFilesExist(string[] expectedFiles)
        {
            HashSet<string> expected = new HashSet<string>(expectedFiles);
            var directoryFiles = Directory.GetFiles(TestDirectoryName);
            Assert.AreEqual(expectedFiles.Length, directoryFiles.Length);
            foreach (var filePath in directoryFiles)
            {
                var filename = Path.GetFileName(filePath);
                Assert.IsTrue(expected.Contains(filename));
            }
        }

        [Test]
        public void RemoveUnused()
        {
            _remover.UsedFile(Test1);
            _remover.UsedFile(Test2);
            _remover.RemoveUnusedFiles();

            AssertFilesExist(new[] { Test1, Test1Meta, Test2, Test2Meta });
        }

        [Test]
        public void RemoveAll()
        {
            _remover.RemoveUnusedFiles();
            AssertFilesExist(new string[0]);
        }

        [Test]
        public void NoDirectory()
        {
            Directory.Delete(TestDirectoryName, true);

            // no op
            _remover.RemoveUnusedFiles();
        }

        [Test]
        public void RemoveFiles()
        {
            UnusedFileRemover.RemoveFiles(TestDirectoryName, "cs");

            AssertFilesExist(new[] { Test1Meta, Test2Meta, Test3Meta });
        }

        [Test]
        public void RemoveUnusedMeta()
        {
            CreateFile("Test4.cs.meta");
            CreateFile("Test5.cs.meta");
            CreateFile("Test6.cs.meta");

            UnusedFileRemover.RemoveUnusedMetaFiles(TestDirectoryName);

            AssertFilesExist(new[] { Test1, Test1Meta, Test2, Test2Meta, Test3, Test3Meta });
        }
    }
}
