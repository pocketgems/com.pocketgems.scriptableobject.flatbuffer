using System.IO;
using NUnit.Framework;
using UnityEditor;

namespace PocketGems.Parameters.Util
{
    public class SchemaBuilderTest
    {
        private string _filePath;

        [SetUp]
        public void SetUp()
        {
            _filePath = Path.Combine(EditorParameterConstants.SanitizedDataPath(), "unit_test_schema.fbs");
            FileUtil.DeleteFileOrDirectory(_filePath);
        }

        [TearDown]
        public void TearDown()
        {
            FileUtil.DeleteFileOrDirectory(_filePath);
        }

        [Test]
        public void GenerateSchemaFile()
        {
            Assert.False(File.Exists(_filePath));
            const string containerName = "RootContainer";
            const string tableName = "Dragon";

            var generator = new SchemaBuilder(containerName);

            // data table
            generator.DefineField(tableName, "name", FlatBufferFieldType.String);
            generator.DefineField(tableName, "friendly", FlatBufferFieldType.Bool);
            generator.DefineField(tableName, "type", FlatBufferFieldType.Short);
            generator.DefineField(tableName, "attack", FlatBufferFieldType.Int);
            generator.DefineField(tableName, "health", FlatBufferFieldType.Long);
            generator.DefineField(tableName, "rarity", FlatBufferFieldType.Float);
            generator.DefineField(tableName, "rival", tableName);

            // container table
            generator.DefineArrayField(containerName, tableName + "Collection", tableName);
            generator.DefineArrayField(containerName, "IntArray", FlatBufferFieldType.Int);

            Assert.IsNotNull(generator.GenerateSchemaContent());
            Assert.AreEqual(2, generator.TableNames.Count);
            Assert.IsTrue(generator.TableNames.Contains(tableName));
            Assert.IsTrue(generator.TableNames.Contains(containerName));
        }
    }
}
