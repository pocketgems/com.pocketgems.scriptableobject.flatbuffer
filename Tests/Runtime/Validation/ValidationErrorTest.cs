using System;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace PocketGems.Parameters.Validation
{
    public class ValidationErrorTest
    {
        [TestCase(null, null, null, null)]
        [TestCase(typeof(string), null, null, null)]
        [TestCase(typeof(string), "id", null, null)]
        [TestCase(typeof(string), "id", "property", null)]
        [TestCase(typeof(string), "id", "property", "msg")]
        public void ToString(Type type, string identifier, string property, string message)
        {
            var v = new ValidationError(type, identifier, property, message);
            Assert.IsNotEmpty(v.ToString());
        }

        [Test]
        public void JsonDeserializeError()
        {
            var originalError = new ValidationError(typeof(string), null, null, null);
            string errorJson = JsonUtility.ToJson(originalError);
            errorJson = errorJson.Replace(typeof(string).AssemblyQualifiedName, "blah");

            var deserializedError = JsonUtility.FromJson<ValidationError>(errorJson);
            LogAssert.Expect(LogType.Error, "Unable to find type blah");
            Assert.IsNull(deserializedError.InfoType);
            Assert.AreEqual(originalError.InfoIdentifier, deserializedError.InfoIdentifier);
            Assert.AreEqual(originalError.InfoProperty, deserializedError.InfoProperty);
            Assert.AreEqual(originalError.Message, deserializedError.Message);
        }
    }
}
