using System.Collections.Generic;
using NUnit.Framework;

namespace PocketGems.Parameters.DataGeneration.Operation.Editor
{
    public class DataOperationContextTest
    {
        [Test]
        public void Coverage()
        {
            var context = new DataOperationContext();
            context.GenerateDataType = GenerateDataType.All;
            var list1 = new List<string>();
            var list2 = new List<string>();
            context.ModifiedCSVPaths = list1;
            context.ModifiedScriptableObjectPaths = list2;

            Assert.AreEqual(GenerateDataType.All, context.GenerateDataType);
            Assert.IsFalse(context.GenerateAllAgain);
            Assert.AreEqual(list1, context.ModifiedCSVPaths);
            Assert.AreEqual(list2, context.ModifiedScriptableObjectPaths);
            Assert.IsNotNull(context.InfoCSVFileCache);
            Assert.IsNotNull(context.StructCSVFileCache);
            Assert.IsNotEmpty(context.GeneratedLocalCSVDirectory);
            Assert.IsNotEmpty(context.GeneratedAssetDirectory);
            Assert.IsNotEmpty(context.GeneratedAssetFileName);
#if ADDRESSABLE_PARAMS
            Assert.IsNotEmpty(context.GeneratedAssetGuid);
            Assert.IsNotEmpty(context.GeneratedAddressableGroup);
            Assert.IsNotEmpty(context.GeneratedAddressableAddress);
#endif
            Assert.AreEqual(0, context.ScriptableObjectMetadatas.Count);
            Assert.AreEqual(0, context.GeneratedFilePaths.Count);
        }
    }
}
