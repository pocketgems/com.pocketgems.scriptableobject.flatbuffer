using System.IO;
using NUnit.Framework;

namespace PocketGems.Parameters.DataGeneration.LocalCSV.Editor
{
    public abstract class BaseCSVFileCacheTest
    {
        protected static string TestDirectoryName = "CSVFileCacheTest";

        [SetUp]
        public void SetUp()
        {
            if (Directory.Exists(TestDirectoryName))
                Directory.Delete(TestDirectoryName, true);
            Directory.CreateDirectory(TestDirectoryName);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(TestDirectoryName))
                Directory.Delete(TestDirectoryName, true);
        }
    }
}
