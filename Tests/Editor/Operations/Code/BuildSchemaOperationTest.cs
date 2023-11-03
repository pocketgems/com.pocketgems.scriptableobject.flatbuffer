using System.Collections.Generic;
using System.IO;
using NSubstitute;
using NUnit.Framework;
using PocketGems.Parameters.Editor.Operation;
using PocketGems.Parameters.Models;
using UnityEditor;

namespace PocketGems.Parameters.Operations.Code
{
    public class BuildSchemaOperationTest : BaseOperationTest<ICodeOperationContext>
    {
        private string _filePath;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _filePath = Path.Combine("Assets", "Unit_test.fbs");
        }

        [TearDown]
        public void TearDown()
        {
            if (File.Exists(_filePath)) AssetDatabase.DeleteAsset(_filePath);
        }

        [Test]
        public void Operation()
        {
            var infos = new List<IParameterInfo> { MockedInterfaces.ParameterInfo("Currency") };
            var structs = new List<IParameterStruct> { MockedInterfaces.ParameterStruct("RewardStruct") };
            _contextMock.SchemaFilePath.ReturnsForAnyArgs(_filePath);
            _contextMock.ParameterInfos.ReturnsForAnyArgs(infos);
            _contextMock.ParameterStructs.ReturnsForAnyArgs(structs);

            var operation = new BuildSchemaOperation();
            AssertExecute(operation, OperationState.Finished);
            Assert.True(File.Exists(_filePath));
        }
    }
}
