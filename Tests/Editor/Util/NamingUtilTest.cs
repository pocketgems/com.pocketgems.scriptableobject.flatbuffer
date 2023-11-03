using System.IO;
using NUnit.Framework;

namespace PocketGems.Parameters.Util
{
    public class NamingUtilTest
    {
        [Test]
        public void InfoNamingConversions()
        {
            string baseName = "SomeParameterInfo";

            // base name <-> scriptable object
            var soClassName = NamingUtil.ScriptableObjectClassNameFromBaseName(baseName, false);
            Assert.AreEqual($"{baseName}ScriptableObject", soClassName);
            var soClassNameExt = NamingUtil.ScriptableObjectClassNameFromBaseName(baseName, true);
            Assert.AreEqual($"{soClassName}.cs", soClassNameExt);
            Assert.AreEqual(baseName, NamingUtil.BaseNameFromScriptableObjectClassName(soClassName));
            Assert.AreEqual(baseName, NamingUtil.BaseNameFromScriptableObjectClassName(soClassNameExt));

            // base name <-> interface name
            var interfaceName = NamingUtil.InfoInterfaceNameFromBaseName(baseName);
            Assert.AreEqual($"I{baseName}", interfaceName);
            Assert.AreEqual(baseName, NamingUtil.BaseNameFromInfoInterfaceName(interfaceName));

            // base name <-> csv name
            var csvName = NamingUtil.CSVFileNameFromBaseName(baseName, false);
            Assert.AreEqual(baseName, csvName);
            var csvNameExt = NamingUtil.CSVFileNameFromBaseName(baseName, true);
            Assert.AreEqual($"{csvName}.csv", csvNameExt);
            Assert.AreEqual(baseName, NamingUtil.BaseNameFromCSVName(csvName));
            Assert.AreEqual(baseName, NamingUtil.BaseNameFromCSVName(csvNameExt));

            // base name -> flat buffer
            var fbClassName = NamingUtil.FlatBufferClassNameFromBaseName(baseName, false);
            Assert.AreEqual($"{baseName}FlatBuffer", fbClassName);
            var fbClassNameExt = NamingUtil.FlatBufferClassNameFromBaseName(baseName, true);
            Assert.AreEqual($"{fbClassName}.cs", fbClassNameExt);

            // base name -> flat buffer struct
            var fbStructName = NamingUtil.FlatBufferStructNameFromBaseName(baseName, false);
            Assert.AreEqual($"{baseName}FlatBufferStruct", fbStructName);
            var fbStructNameExt = NamingUtil.FlatBufferStructNameFromBaseName(baseName, true);
            Assert.AreEqual($"{fbStructName}.cs", fbStructNameExt);

            // base name -> validator class
            var validatorName = NamingUtil.ValidatorClassNameFromBaseName(baseName, false);
            Assert.AreEqual($"{baseName}Validator", validatorName);
            var validatorNameExt = NamingUtil.ValidatorClassNameFromBaseName(baseName, true);
            Assert.AreEqual($"{validatorName}.cs", validatorNameExt);

            // ensure runtime assembly function works in the same way as editor functions
            Assert.AreEqual(interfaceName, LocalCSV.CSVUtil.CSVToInterfaceFileName(csvName));
        }

        [Test]
        public void StructNamingConversions()
        {
            string baseName = "SomeParameterStruct";

            // base name <-> struct
            var structName = NamingUtil.StructNameFromBaseName(baseName, false);
            Assert.AreEqual(baseName, structName);
            var structNameExt = NamingUtil.StructNameFromBaseName(baseName, true);
            Assert.AreEqual($"{structName}.cs", structNameExt);
            Assert.AreEqual(baseName, NamingUtil.BaseNameFromStructName(structName));
            Assert.AreEqual(baseName, NamingUtil.BaseNameFromStructName(structNameExt));

            // base name <-> interface name
            var interfaceName = NamingUtil.StructInterfaceNameFromBaseName(baseName);
            Assert.AreEqual($"I{baseName}", interfaceName);
            Assert.AreEqual(baseName, NamingUtil.BaseNameFromStructInterfaceName(interfaceName));
        }

        [Test]
        public void RelativePath()
        {
            var projectPath = Directory.GetCurrentDirectory();
            string relativePath = "blah/blah";

            Assert.AreEqual(relativePath,
                NamingUtil.RelativePath(Path.Join(projectPath, relativePath)));
            Assert.AreEqual(relativePath, NamingUtil.RelativePath(relativePath));
        }

        [Test]
        public void ToCamelCase()
        {
            Assert.AreEqual(" hello", " hello".LowercaseFirstChar());
            Assert.AreEqual("hello", "hello".LowercaseFirstChar());
            Assert.AreEqual("helloWorld", "helloWorld".LowercaseFirstChar());
            Assert.AreEqual("helloWorld", "HelloWorld".LowercaseFirstChar());
            Assert.AreEqual("_HelloWorld", "_HelloWorld".LowercaseFirstChar());
            Assert.AreEqual("hELLO", "HELLO".LowercaseFirstChar());
            Assert.AreEqual("h", "h".LowercaseFirstChar());
            Assert.AreEqual("h", "H".LowercaseFirstChar());
            Assert.AreEqual("", "".LowercaseFirstChar());
        }

        [Test]
        public void ToPascalCase()
        {
            Assert.AreEqual(" hello", " hello".UppercaseFirstChar());
            Assert.AreEqual("Hello", "hello".UppercaseFirstChar());
            Assert.AreEqual("HelloWorld", "helloWorld".UppercaseFirstChar());
            Assert.AreEqual("HelloWorld", "HelloWorld".UppercaseFirstChar());
            Assert.AreEqual("_HelloWorld", "_HelloWorld".UppercaseFirstChar());
            Assert.AreEqual("HELLO", "HELLO".UppercaseFirstChar());
            Assert.AreEqual("H", "h".UppercaseFirstChar());
            Assert.AreEqual("H", "H".UppercaseFirstChar());
            Assert.AreEqual("", "".UppercaseFirstChar());
        }

        [Test]
        public void ToSnakeCase()
        {
            Assert.AreEqual(" hello", " hello".ToSnakeCase());
            Assert.AreEqual("hello", "hello".ToSnakeCase());
            Assert.AreEqual("hello_world", "helloWorld".ToSnakeCase());
            Assert.AreEqual("hello_world", "HelloWorld".ToSnakeCase());
            Assert.AreEqual("_hello_world", "_HelloWorld".ToSnakeCase());
            Assert.AreEqual("h_e_l_l_o", "HELLO".ToSnakeCase());
            Assert.AreEqual("h", "h".ToSnakeCase());
            Assert.AreEqual("h", "H".ToSnakeCase());
            Assert.AreEqual("", "".ToSnakeCase());
        }
    }
}
