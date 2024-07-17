using System.IO;
using NSubstitute;
using NUnit.Framework;
using PocketGems.Parameters.Common.Operation.Editor;

namespace PocketGems.Parameters.CodeGeneration.Operations.Editor
{
    public class GenerateCodeOperationTest : BaseCodeOperationTest
    {
        private const string TestCodeDir = "TestCodeDir";
        private const string TestFlatBufferBuilderDir = "TestFlatBufferBuilderDir";
        private const string TestCSVBridgeDir = "TestCSVBridgeDir";

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            TearDown();

            Directory.CreateDirectory(TestCodeDir);
            _contextMock.GeneratedCodeDir.ReturnsForAnyArgs(TestCodeDir);
            _contextMock.GeneratedCodeFlatBufferBuilderDir.ReturnsForAnyArgs(TestFlatBufferBuilderDir);
            _contextMock.GeneratedCodeCSVBridgeDir.ReturnsForAnyArgs(TestCSVBridgeDir);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(TestCodeDir))
                Directory.Delete(TestCodeDir, true);
            if (Directory.Exists(TestFlatBufferBuilderDir))
                Directory.Delete(TestFlatBufferBuilderDir, true);
            if (Directory.Exists(TestCSVBridgeDir))
                Directory.Delete(TestCSVBridgeDir, true);
        }

        [Test]
        public void Execute()
        {
            var operation = new GenerateCodeOperation();
            AssertExecute(operation, OperationState.Finished);

            int FileCount(string dir) =>
                Directory.GetFiles(dir, "*", SearchOption.TopDirectoryOnly).Length;

            // GenerateParamsValidation
            // GenerateDataLoader
            // GenerateParamsSetup
            Assert.AreEqual(3, FileCount(TestCodeDir));

            // GenerateFlatBufferBuilder
            Assert.AreEqual(
                1 + _mockParameterInfos.Count + _mockParameterStructs.Count,
                FileCount(TestFlatBufferBuilderDir));

            // GenerateCSVBridge
            Assert.AreEqual(
                1 + _mockParameterInfos.Count + _mockParameterStructs.Count,
                FileCount(TestCSVBridgeDir));
        }
    }
}
