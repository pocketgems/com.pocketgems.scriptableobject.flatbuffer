using NSubstitute;
using NUnit.Framework;
using PocketGems.Parameters.CodeGeneration.Operation.Editor;
using PocketGems.Parameters.Common.Operations.Editor;

namespace PocketGems.Parameters.CodeGeneration.Operations.Editor
{
    public class BaseCodeOperationTest : BaseOperationTest<ICodeOperationContext>
    {
        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _contextMock.ParameterInfos.ReturnsForAnyArgs(_mockParameterInfos);
            _contextMock.ParameterStructs.ReturnsForAnyArgs(_mockParameterStructs);
        }
    }
}
