using System.IO;
using NUnit.Framework;
using PocketGems.Parameters.CodeGeneration.Operation.Editor;
using PocketGems.Parameters.Common.Operation.Editor;
using PocketGems.Parameters.Common.Operations.Editor;
using UnityEditor;

namespace PocketGems.Parameters.CodeGeneration.Operations.Editor
{
    public class DeleteAssetOperationTest : BaseOperationTest<ICodeOperationContext>
    {
        private string _filePath1;
        private string _filePath2;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _filePath1 = Path.Combine("Assets", "test_schema.fbs");
            // path outside of Assets folder
            _filePath2 = Path.Combine("Temp", "test_schema.fbs");
        }

        [TearDown]
        public void TearDown()
        {
            if (File.Exists(_filePath1))
                File.Delete(_filePath1);
            if (File.Exists(_filePath2))
                File.Delete(_filePath2);
        }

        [Test]
        public void ExecutionPath1()
        {
            File.WriteAllText(_filePath1, "");
            AssetDatabase.ImportAsset(_filePath1);

            var operation = new DeleteAssetOperation(_filePath1);
            operation.Execute(null);
            AssertExecute(operation, OperationState.Finished);
            Assert.False(File.Exists(_filePath1));
        }

        [Test]
        public void ExecutionPath2()
        {
            File.WriteAllText(_filePath2, "");

            var operation = new DeleteAssetOperation(_filePath2);
            operation.Execute(null);
            AssertExecute(operation, OperationState.Finished);
            Assert.False(File.Exists(_filePath2));
        }

        [Test]
        public void NoErrorForNonExistingFile()
        {
            AssertExecute(new DeleteAssetOperation(null), OperationState.Finished);
            AssertExecute(new DeleteAssetOperation(""), OperationState.Finished);
            AssertExecute(new DeleteAssetOperation("blah"), OperationState.Finished);
        }
    }
}
