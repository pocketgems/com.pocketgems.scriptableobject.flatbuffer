using System.IO;
using NSubstitute;
using NUnit.Framework;
using PocketGems.Parameters.Editor.Operation;

namespace PocketGems.Parameters.Operations.Code
{
    public class GenerateValidatorFilesOperationTest : BaseCodeOperationTest
    {
        private const string TestValidatorsDir = "TestValidatorsDir";

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            TearDown();

            _contextMock.GeneratedCodeValidatorsDir.ReturnsForAnyArgs(TestValidatorsDir);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(TestValidatorsDir))
                Directory.Delete(TestValidatorsDir, true);
        }

        [Test]
        public void Execute()
        {
            var operation = new GenerateValidatorFilesOperation();
            AssertExecute(operation, OperationState.Finished);

            void AssertFileCount()
            {
                int files = Directory.GetFiles(TestValidatorsDir, "*", SearchOption.TopDirectoryOnly).Length;
                Assert.AreEqual(_mockParameterInfos.Count + _mockParameterStructs.Count, files);
            }

            AssertFileCount();

            // running again shouldn't cause issues
            operation = new GenerateValidatorFilesOperation();
            AssertExecute(operation, OperationState.Finished);
            AssertFileCount();
        }
    }
}
