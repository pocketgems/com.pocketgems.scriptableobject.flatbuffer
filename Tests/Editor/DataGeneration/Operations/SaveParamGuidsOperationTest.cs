using System.Collections.Generic;
using System.IO;
using NSubstitute;
using NUnit.Framework;
using PocketGems.Parameters.Common.Models.Editor;
using PocketGems.Parameters.Common.Operation.Editor;
using PocketGems.Parameters.Common.Operations.Editor;
using PocketGems.Parameters.DataGeneration.Operation.Editor;
using PocketGems.Parameters.DataGeneration.Util.Editor;

namespace PocketGems.Parameters.DataGeneration.Operations.Editor
{
    public class SaveParamGuidsOperationTest : BaseOperationTest<IDataOperationContext>
    {
        private const string kDirectoryName = "SaveParamGuidsOperationTest";
        private string _originalFilePath;
        private Dictionary<IParameterInfo, List<IScriptableObjectMetadata>> _scriptableObjectMetadatas;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            _originalFilePath = ParameterGuidCache.FilePath;
            ParameterGuidCache.FilePath = Path.Combine(kDirectoryName, ".param_guids");
            ParameterGuidCache.Replace(new[] { "old" });

            _scriptableObjectMetadatas = new Dictionary<IParameterInfo, List<IScriptableObjectMetadata>>
            {
                [_mockParameterInfo1] = new List<IScriptableObjectMetadata> { Metadata("a"), Metadata("b") },
                [_mockParameterInfo2] = new List<IScriptableObjectMetadata> { Metadata("c") },
            };
            _contextMock.ScriptableObjectMetadatas.ReturnsForAnyArgs(_scriptableObjectMetadatas);
        }

        [TearDown]
        public void TearDown()
        {
            ParameterGuidCache.FilePath = _originalFilePath;
            if (Directory.Exists(kDirectoryName))
                Directory.Delete(kDirectoryName, true);
        }

        [Test]
        public void All_ReplacesGuids()
        {
            _contextMock.GenerateDataType.ReturnsForAnyArgs(GenerateDataType.All);

            AssertExecute(new SaveParamGuidsOperation(), OperationState.Finished);

            Assert.IsTrue(ParameterGuidCache.MightBeParameter("a"));
            Assert.IsTrue(ParameterGuidCache.MightBeParameter("b"));
            Assert.IsTrue(ParameterGuidCache.MightBeParameter("c"));
            Assert.IsFalse(ParameterGuidCache.MightBeParameter("old"));
        }

        [TestCase(GenerateDataType.ScriptableObjectDiff)]
        [TestCase(GenerateDataType.CSVDiff)]
        public void Diff_AddsGuids(GenerateDataType generateDataType)
        {
            _contextMock.GenerateDataType.ReturnsForAnyArgs(generateDataType);

            AssertExecute(new SaveParamGuidsOperation(), OperationState.Finished);

            Assert.IsTrue(ParameterGuidCache.MightBeParameter("a"));
            Assert.IsTrue(ParameterGuidCache.MightBeParameter("b"));
            Assert.IsTrue(ParameterGuidCache.MightBeParameter("c"));
            Assert.IsTrue(ParameterGuidCache.MightBeParameter("old"));
            Assert.IsFalse(ParameterGuidCache.MightBeParameter("d"));
        }

        private static IScriptableObjectMetadata Metadata(string guid)
        {
            var metadata = Substitute.For<IScriptableObjectMetadata>();
            metadata.GUID.Returns(guid);
            return metadata;
        }
    }
}
