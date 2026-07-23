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

        [Test]
        public void GetHashCode_BasedOnName()
        {
            var assembly = new AssemblyDefinitionFile(_name);
            Assert.AreEqual(_name.GetHashCode(), assembly.GetHashCode());
        }

        [Test]
        public void Equals_Null_ReturnsFalse()
        {
            var assembly = new AssemblyDefinitionFile(_name);
            Assert.IsFalse(assembly.Equals(null));
        }

        [Test]
        public void Equals_DifferentType_ReturnsFalse()
        {
            var assembly = new AssemblyDefinitionFile(_name);
            Assert.IsFalse(assembly.Equals("not an assembly definition"));
        }

        [Test]
        public void Equals_SameValues_ReturnsTrue()
        {
            var a = new AssemblyDefinitionFile(_name);
            var b = new AssemblyDefinitionFile(_name);
            Assert.IsTrue(a.Equals(b));
        }

        [Test]
        public void Equals_DifferentFields_ReturnsFalse()
        {
            var a = new AssemblyDefinitionFile(_name);

            var differentName = new AssemblyDefinitionFile("OtherName");
            Assert.IsFalse(a.Equals(differentName));

            var differentReferences = new AssemblyDefinitionFile(_name) { references = new[] { "SomeRef" } };
            Assert.IsFalse(a.Equals(differentReferences));

            var differentPlatforms = new AssemblyDefinitionFile(_name) { includePlatforms = new[] { "Android" } };
            Assert.IsFalse(a.Equals(differentPlatforms));

            var differentUnsafeCode = new AssemblyDefinitionFile(_name) { allowUnsafeCode = true };
            Assert.IsFalse(a.Equals(differentUnsafeCode));
        }
    }
}
