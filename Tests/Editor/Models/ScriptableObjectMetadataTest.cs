using NUnit.Framework;
using UnityEngine;

namespace PocketGems.Parameters.Models
{
    public class ScriptableObjectMetadataTest
    {
        [Test]
        public void Test()
        {
            var obj = ScriptableObject.CreateInstance(typeof(ScriptableObject));
            var metadata = new ScriptableObjectMetadata("guid", "path", obj);
            Assert.AreEqual("guid", metadata.GUID);
            Assert.AreEqual("path", metadata.FilePath);
            Assert.AreEqual(obj, metadata.ScriptableObject);
        }
    }
}
