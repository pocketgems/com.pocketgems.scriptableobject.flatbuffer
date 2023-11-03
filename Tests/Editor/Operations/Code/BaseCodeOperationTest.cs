using NSubstitute;
using NUnit.Framework;
using PocketGems.Parameters.Editor.Operation;

namespace PocketGems.Parameters.Operations.Code
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
