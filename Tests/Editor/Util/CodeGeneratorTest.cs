using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using PocketGems.Parameters.Models;

namespace PocketGems.Parameters.Util
{
    public class CodeGeneratorTest
    {
        private const string TestDirectoryName = "TestDirectory";
        private const string TestFilename = "TestClassFile";
        private IParameterInfo _mockParameterInfo1;
        private IParameterInfo _mockParameterInfo2;
        private IParameterInfo _mockParameterInfo3;
        private IParameterStruct _mockParameterStruct1;
        private IParameterStruct _mockParameterStruct2;
        private List<IParameterInfo> _mockParameterInfos;
        private List<IParameterStruct> _mockParameterStructs;

        [SetUp]
        public void SetUp()
        {
            TearDown();

            _mockParameterInfo1 = MockedInterfaces.ParameterInfo("CurrencyInfo");
            _mockParameterInfo2 = MockedInterfaces.ParameterInfo("BuildingInfo");
            _mockParameterInfo3 = MockedInterfaces.ParameterInfo("DragonInfo");

            _mockParameterStruct1 = MockedInterfaces.ParameterStruct("RewardStruct");
            _mockParameterStruct2 = MockedInterfaces.ParameterStruct("KeyValueStruct");
            _mockParameterInfos = new List<IParameterInfo> { _mockParameterInfo1, _mockParameterInfo2, _mockParameterInfo3 };
            _mockParameterStructs = new List<IParameterStruct> { _mockParameterStruct1, _mockParameterStruct2 };
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(TestDirectoryName))
                Directory.Delete(TestDirectoryName, true);

            if (File.Exists(TestFilename))
                File.Delete(TestFilename);
        }

        private void AssertFileExists(string expectedFilename)
        {
            if (!Directory.Exists(TestDirectoryName))
                Assert.Fail();
            var files = Directory.GetFiles(TestDirectoryName);
            for (int i = 0; i < files.Length; i++)
            {
                var filename = Path.GetFileName(files[i]);
                if (filename == expectedFilename)
                    return;
            }
            Assert.Fail($"Cannot find file {expectedFilename}");
        }

        private void AssertFileCount(int expectedCount)
        {
            int actualCount = 0;
            if (Directory.Exists(TestDirectoryName))
                actualCount = Directory.GetFiles(TestDirectoryName).Length;
            Assert.AreEqual(expectedCount, actualCount);
        }

        [Test]
        public void GenerateAssemblyInfo()
        {
            Directory.CreateDirectory(TestDirectoryName);
            var filename = "AssemblyInfo.cs";
            var filePath = Path.Combine(TestDirectoryName, filename);
            CodeGenerator.GenerateAssemblyInfo(filePath);

            AssertFileCount(1);
            AssertFileExists(filename);
        }

        [Test]
        public void GenerateParamsSetup()
        {
            Directory.CreateDirectory(TestDirectoryName);
            CodeGenerator.GenerateParamsSetup(TestDirectoryName);

            AssertFileCount(1);
            AssertFileExists($"{EditorParameterConstants.ParamsSetupClass.ClassName}.cs");
        }

        [Test]
        public void GenerateParamsValidation()
        {
            Directory.CreateDirectory(TestDirectoryName);
            CodeGenerator.GenerateParamsValidation(_mockParameterInfos, TestDirectoryName);

            AssertFileCount(1);
            AssertFileExists($"{EditorParameterConstants.ParamsValidationClass.ClassName}.cs");
        }

        [Test]
        public void AttemptGenerateValidationFile()
        {
            // create new validators
            Assert.IsTrue(CodeGenerator.AttemptGenerateValidationFile(_mockParameterInfo1, TestDirectoryName));
            AssertFileCount(1);
            Assert.IsTrue(CodeGenerator.AttemptGenerateValidationFile(_mockParameterInfo2, TestDirectoryName));
            AssertFileCount(2);
            Assert.IsTrue(CodeGenerator.AttemptGenerateValidationFile(_mockParameterInfo3, TestDirectoryName));
            AssertFileCount(3);

            // not created - already exists
            Assert.IsFalse(CodeGenerator.AttemptGenerateValidationFile(_mockParameterInfo2, TestDirectoryName));
            AssertFileCount(3);

            // create new validators
            Assert.IsTrue(CodeGenerator.AttemptGenerateValidationFile(_mockParameterStruct1, TestDirectoryName));
            AssertFileCount(4);
            Assert.IsTrue(CodeGenerator.AttemptGenerateValidationFile(_mockParameterStruct2, TestDirectoryName));
            AssertFileCount(5);

            // not created - already exists
            Assert.IsFalse(CodeGenerator.AttemptGenerateValidationFile(_mockParameterStruct2, TestDirectoryName));
            AssertFileCount(5);
        }

        [Test]
        public void GenerateScriptableObjectFile()
        {
            CodeGenerator.GenerateScriptableObjectFile(_mockParameterInfo1, 0, TestDirectoryName);

            AssertFileCount(1);
            AssertFileExists(_mockParameterInfo1.ScriptableObjectClassName(true));
        }

        [Test]
        public void GenerateStructFile()
        {
            CodeGenerator.GenerateStructFile(_mockParameterStruct1, TestDirectoryName);

            AssertFileCount(1);
            AssertFileExists(_mockParameterStruct1.StructName(true));
        }

        [Test]
        public void GenerateFlatBufferClassFile()
        {
            CodeGenerator.GenerateFlatBufferClassFile(_mockParameterInfo1, true, TestDirectoryName);

            AssertFileCount(1);
            AssertFileExists(_mockParameterInfo1.FlatBufferClassName(true));
        }

        [Test]
        public void GenerateFlatBufferBuilder()
        {
            CodeGenerator.GenerateFlatBufferBuilder(
                _mockParameterInfos,
                _mockParameterStructs,
                TestDirectoryName);

            AssertFileCount(1 + _mockParameterInfos.Count + _mockParameterStructs.Count);
            AssertFileExists($"{EditorParameterConstants.FlatBufferBuilderClass.ClassName}.cs");
        }

        [Test]
        public void GenerateCSVBridge()
        {
            CodeGenerator.GenerateCSVBridge(
                _mockParameterInfos,
                _mockParameterStructs,
                TestDirectoryName);

            AssertFileCount(1 + _mockParameterInfos.Count + _mockParameterStructs.Count);
            AssertFileExists($"{EditorParameterConstants.CSVBridgeClass.ClassName}.cs");
        }

        [Test]
        public void GenerateDataLoader()
        {
            CodeGenerator.GenerateDataLoader("some_hash", _mockParameterInfos, _mockParameterStructs, TestDirectoryName);

            AssertFileCount(1);
            AssertFileExists($"{EditorParameterConstants.DataLoaderClass.ClassName}.cs");
        }

        [Test]
        public void DisableInterfaceImplementations()
        {
            // write test file
            var testContents = "test contents";
            File.WriteAllText(TestFilename, testContents);

            CodeGenerator.DisableInterfaceImplementations(TestFilename);
            var text = File.ReadAllText(TestFilename);
            Assert.IsTrue(text.Contains(EditorParameterConstants.Interface.DisableImplementationSymbol));
            Assert.IsTrue(text.Contains(testContents));
        }

        [Test]
        public void ConvertFileToMacNewlines()
        {
            // write test file
            var testContents = "\r\n\n\r";
            File.WriteAllText(TestFilename, testContents);

            CodeGenerator.ConvertFileToMacNewlines(TestFilename);
            var text = File.ReadAllText(TestFilename);
            Assert.AreEqual("\n\n\n", text);
        }
    }
}
