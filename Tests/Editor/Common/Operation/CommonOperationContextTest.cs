using NUnit.Framework;

namespace PocketGems.Parameters.Common.Operation.Editor
{
    public class CommonOperationContextTest
    {
        [Test]
        public void Coverage()
        {
            var context = new CommonOperationContext();

            Assert.IsNotNull(context.ParameterEnums);
            Assert.IsNotNull(context.ParameterInfos);
            Assert.IsNotNull(context.ParameterStructs);
            Assert.IsNotNull(context.InterfaceDirectoryRootPath);
            Assert.IsNotNull(context.InterfaceAssemblyName);
            Assert.IsNotNull(context.InterfaceHash);
            Assert.IsNull(context.InterfaceAssemblyHash);
        }
    }
}
