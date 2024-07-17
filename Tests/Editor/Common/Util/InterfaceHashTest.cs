using System.IO;
using NUnit.Framework;

namespace PocketGems.Parameters.Common.Util.Editor
{
    public class InterfaceHashTest
    {
        private const string assemblyFilePath = "hash_assembly";
        private const string assemblyEditorFilePath = "hash_assembly_editor";
        private const string dataFilePath = "hash_data";

        private InterfaceHash _interfaceHash;

        [SetUp]
        public void SetUp()
        {
            TearDown();
            CreateInstance();
        }

        [TearDown]
        public void TearDown()
        {
            Delete(assemblyFilePath);
            Delete(assemblyEditorFilePath);
            Delete(dataFilePath);
        }

        private void CreateInstance()
        {
            _interfaceHash = new InterfaceHash(
                assemblyFilePath,
                assemblyEditorFilePath,
                dataFilePath);
        }

        private void Delete(string path)
        {
            if (File.Exists(path))
                File.Delete(path);
        }

        private void WriteLines(int count, string path)
        {
            string[] lines = new string[count];
            for (int i = 0; i < count; i++)
                lines[i] = "line";
            File.WriteAllText(path, string.Join("\n", lines));
        }

        private void AssertLines(int count, string path)
        {
            var text = File.ReadAllText(path);
            Assert.AreEqual(count, text.Split('\n').Length);
        }

        [Test]
        public void NoHashExist()
        {
            Assert.IsNull(_interfaceHash.AssemblyInfoHash);
            Assert.IsNull(_interfaceHash.AssemblyInfoEditorHash);
            Assert.IsNull(_interfaceHash.GeneratedDataHash);
        }

        [Test]
        [TestCase("a", null, null)]
        [TestCase(null, "b", null)]
        [TestCase(null, null, "c")]
        [TestCase("a", "b", "c")]
        public void WriteHashes(string h1, string h2, string h3)
        {
            if (h1 != null)
                _interfaceHash.AssemblyInfoHash = h1;
            if (h2 != null)
                _interfaceHash.AssemblyInfoEditorHash = h2;
            if (h3 != null)
                _interfaceHash.GeneratedDataHash = h3;

            Assert.AreEqual(h1, _interfaceHash.AssemblyInfoHash);
            Assert.AreEqual(h2, _interfaceHash.AssemblyInfoEditorHash);
            Assert.AreEqual(h3, _interfaceHash.GeneratedDataHash);

            CreateInstance();

            Assert.AreEqual(h1, _interfaceHash.AssemblyInfoHash);
            Assert.AreEqual(h2, _interfaceHash.AssemblyInfoEditorHash);
            Assert.AreEqual(h3, _interfaceHash.GeneratedDataHash);
        }

        [Test]
        public void WriteMultipleTimes()
        {
            WriteLines(3, assemblyFilePath);
            WriteLines(4, assemblyEditorFilePath);
            WriteLines(5, dataFilePath);

            Assert.IsNull(_interfaceHash.AssemblyInfoHash);
            Assert.IsNull(_interfaceHash.AssemblyInfoEditorHash);
            Assert.IsNull(_interfaceHash.GeneratedDataHash);

            _interfaceHash.AssemblyInfoHash = "a";
            _interfaceHash.AssemblyInfoEditorHash = "b";
            _interfaceHash.GeneratedDataHash = "c";

            Assert.AreEqual("a", _interfaceHash.AssemblyInfoHash);
            Assert.AreEqual("b", _interfaceHash.AssemblyInfoEditorHash);
            Assert.AreEqual("c", _interfaceHash.GeneratedDataHash);

            // appends the hash
            AssertLines(6, assemblyFilePath);
            AssertLines(7, assemblyEditorFilePath);
            // over write whole file
            AssertLines(1, dataFilePath);

            _interfaceHash.AssemblyInfoHash = "d";
            _interfaceHash.AssemblyInfoEditorHash = "e";
            _interfaceHash.GeneratedDataHash = "f";

            Assert.AreEqual("d", _interfaceHash.AssemblyInfoHash);
            Assert.AreEqual("e", _interfaceHash.AssemblyInfoEditorHash);
            Assert.AreEqual("f", _interfaceHash.GeneratedDataHash);

            // updates the hash
            AssertLines(6, assemblyFilePath);
            AssertLines(7, assemblyEditorFilePath);
            // over write whole file
            AssertLines(1, dataFilePath);
        }
    }
}
