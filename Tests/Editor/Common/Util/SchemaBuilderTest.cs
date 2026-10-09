using System;
using System.IO;
using NUnit.Framework;
using PocketGems.Parameters.Common.Editor;
using PocketGems.Parameters.Editor;
using UnityEditor;

namespace PocketGems.Parameters.Common.Util.Editor
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

        [Test]
        public void TablesAndFieldsKeepDefinitionOrder()
        {
            const string containerName = "RootContainer";
            SchemaBuilder generator = new(containerName);
            string[] tableNames = { "Zebra", "Apple", "Mango" };
            foreach (string tableName in tableNames)
            {
                generator.DefineField(tableName, "name", FlatBufferFieldType.String);
                generator.DefineArrayField(containerName, tableName + "Collection", tableName);
            }

            // the container is first defined right after Zebra
            CollectionAssert.AreEqual(new[] { "Zebra", containerName, "Apple", "Mango" }, generator.TableNames);

            string schema = generator.GenerateSchemaContent();
            int zebra = schema.IndexOf("zebra_collection", StringComparison.Ordinal);
            int apple = schema.IndexOf("apple_collection", StringComparison.Ordinal);
            int mango = schema.IndexOf("mango_collection", StringComparison.Ordinal);
            Assert.That(zebra, Is.GreaterThanOrEqualTo(0));
            Assert.That(zebra, Is.LessThan(apple));
            Assert.That(apple, Is.LessThan(mango));
        }

        [Test]
        public void FieldTypesIgnoreCulture()
        {
            using (CultureScope.Turkish())
            {
                SchemaBuilder generator = new("RootContainer");
                generator.DefineField("Dragon", "attack", FlatBufferFieldType.Int);
                generator.DefineArrayField("Dragon", "costs", FlatBufferFieldType.Int);

                string schema = generator.GenerateSchemaContent();
                StringAssert.Contains("attack:int;", schema);
                StringAssert.Contains("costs:[int];", schema);
                StringAssert.DoesNotContain("ı", schema);
            }
        }
    }
}
