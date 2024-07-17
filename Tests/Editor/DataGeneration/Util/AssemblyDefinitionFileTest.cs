using System.IO;
using NUnit.Framework;

namespace PocketGems.Parameters.DataGeneration.Util.Editor
{
    public class AssemblyDefinitionFileTest
    {
        private string _directory;
        private string _name;
        private string _fileName;
        private string _filePath;

        [SetUp]
        public void SetUp()
        {
            _directory = "Assets";
            _name = "UnitTestAssembly";
            _fileName = $"{_name}.asmdef";
            _filePath = Path.Combine(_directory, _fileName);
        }

        [TearDown]
        public void TearDown()
        {
            if (File.Exists(_filePath)) File.Delete(_filePath);
        }

        [Test]
        public void CreateAndSaveAssembly()
        {
            Assert.IsFalse(File.Exists(_filePath));

            // create & save
            var createdAssembly = new AssemblyDefinitionFile(_name);
            createdAssembly.includePlatforms = new[] { "Android" };
            createdAssembly.autoReferenced = true;
            createdAssembly.WriteFile(_directory);

            Assert.IsTrue(File.Exists(_filePath));

            // load
            var loadedAssembly = AssemblyDefinitionFile.LoadFile(_filePath);

            // compare
            Assert.AreEqual(createdAssembly, loadedAssembly);
        }
    }
}
