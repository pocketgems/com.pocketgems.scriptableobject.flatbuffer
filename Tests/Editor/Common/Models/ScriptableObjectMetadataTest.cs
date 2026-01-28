using System.Collections.Generic;
using NUnit.Framework;
using PocketGems.Parameters.Interface;
using PocketGems.Parameters.Validation;
using UnityEngine;

namespace PocketGems.Parameters.Common.Models.Editor
{
    public class ScriptableObjectMetadataTest
    {
        private class ParameterScriptableObjectClass : ParameterScriptableObject
        {
            public override ValidationError[] ValidationErrors() => throw new System.NotImplementedException();
            public override void CollectLocalizationStrings(HashSet<string> localizationKeys, HashSet<string> localizedScript) => throw new System.NotImplementedException();
        }

        [Test]
        public void Test()
        {
            var obj = (ParameterScriptableObjectClass)ScriptableObject.CreateInstance(typeof(ParameterScriptableObjectClass));
            var metadata = new ScriptableObjectMetadata("guid", "path", obj);
            Assert.AreEqual("guid", metadata.GUID);
            Assert.AreEqual("path", metadata.FilePath);
            Assert.AreEqual(obj, metadata.ScriptableObject);
        }
    }
}
