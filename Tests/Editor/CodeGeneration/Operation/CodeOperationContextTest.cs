using NUnit.Framework;

namespace PocketGems.Parameters.CodeGeneration.Operation.Editor
{
    public class CodeOperationContextTest
    {
        [Test]
        public void Coverage()
        {
            var context = new CodeOperationContext();
            context.GenerateCodeType = GenerateCodeType.Generate;

            Assert.AreEqual(GenerateCodeType.Generate, context.GenerateCodeType);
            Assert.IsNotEmpty(context.SchemaFilePath);
            Assert.IsNotEmpty(context.GeneratedCodeDir);
            Assert.IsNotEmpty(context.GeneratedCodeScriptableObjectsDir);
            Assert.IsNotEmpty(context.GeneratedCodeStructsDir);
            Assert.IsNotEmpty(context.GeneratedCodeFlatBufferClassesDir);
            Assert.IsNotEmpty(context.GeneratedCodeFlatBufferStructsDir);
            Assert.IsNotEmpty(context.GeneratedCodeFlatBufferBuilderDir);
            Assert.IsNotEmpty(context.GeneratedCodeCSVBridgeDir);
            Assert.IsNotEmpty(context.GeneratedCodeValidatorsDir);
        }
    }
}
