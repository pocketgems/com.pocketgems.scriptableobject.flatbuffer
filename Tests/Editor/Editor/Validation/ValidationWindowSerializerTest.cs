using System;
using System.Collections.Generic;
using NUnit.Framework;
using PocketGems.Parameters.Validation;

namespace PocketGems.Parameters.Editor.Validation
{
    public class ValidationWindowSerializerTest
    {
        private IReadOnlyList<ValidationError> _prevError;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _prevError = ValidationWindowSerializer.DeserializeFromStorage();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            ValidationWindowSerializer.SerializeToStorage(_prevError);
        }

        [TestCase(null)]
        [TestCase(typeof(string))]
        [TestCase(typeof(string), "id")]
        [TestCase(typeof(string), "id", "property")]
        [TestCase(typeof(string), "id", "property", "msg")]
        [TestCase(typeof(string), "id", "property", "msg", "key path")]
        [TestCase(typeof(string), "id", "property", "msg", "key path", "property")]
        public void SerializedFields(Type type, string identifier = null, string property = null,
            string message = null, string structKeyPath = null, string structProperty = null)
        {
            var e = new ValidationError(type, identifier, property, message, structKeyPath, structProperty);

            var list = new List<ValidationError> { e };
            ValidationWindowSerializer.SerializeToStorage(list);
            var deserializedList = ValidationWindowSerializer.DeserializeFromStorage();

            Assert.AreEqual(1, deserializedList.Count);
            Assert.AreEqual(type, deserializedList[0].InfoType);
            Assert.AreEqual(e.InfoIdentifier, deserializedList[0].InfoIdentifier);
            Assert.AreEqual(e.InfoProperty, deserializedList[0].InfoProperty);
            Assert.AreEqual(e.Message, deserializedList[0].Message);
            Assert.AreEqual(e.StructKeyPath, deserializedList[0].StructKeyPath);
            Assert.AreEqual(e.StructProperty, deserializedList[0].StructProperty);
            Assert.IsNotEmpty(e.ToString());
        }

        [Test]
        public void SerializeMultiple()
        {
            var e1 = new ValidationError(typeof(int), "id1", "prop1", "message1", "keypath1", "sprop1");
            var e2 = new ValidationError(typeof(float), "id2", "prop2", "message2", "keypath2", "sprop2");
            var list = new List<ValidationError> { e1, e2 };
            ValidationWindowSerializer.SerializeToStorage(list);
            var deserializedList = ValidationWindowSerializer.DeserializeFromStorage();
            Assert.AreEqual(2, deserializedList.Count);

            Assert.AreEqual(e1.InfoType, deserializedList[0].InfoType);
            Assert.AreEqual(e1.InfoIdentifier, deserializedList[0].InfoIdentifier);
            Assert.AreEqual(e1.InfoProperty, deserializedList[0].InfoProperty);
            Assert.AreEqual(e1.Message, deserializedList[0].Message);
            Assert.AreEqual(e1.StructKeyPath, deserializedList[0].StructKeyPath);
            Assert.AreEqual(e1.StructProperty, deserializedList[0].StructProperty);

            Assert.AreEqual(e2.InfoType, deserializedList[1].InfoType);
            Assert.AreEqual(e2.InfoIdentifier, deserializedList[1].InfoIdentifier);
            Assert.AreEqual(e2.InfoProperty, deserializedList[1].InfoProperty);
            Assert.AreEqual(e2.Message, deserializedList[1].Message);
            Assert.AreEqual(e2.StructKeyPath, deserializedList[1].StructKeyPath);
            Assert.AreEqual(e2.StructProperty, deserializedList[1].StructProperty);
        }


        [Test]
        public void ClearStorage()
        {
            void Setup()
            {
                var e = new ValidationError(typeof(int), "id", "prop", "message");
                var list = new List<ValidationError> { e };
                ValidationWindowSerializer.SerializeToStorage(list);
                Assert.AreEqual(1, ValidationWindowSerializer.DeserializeFromStorage().Count);
            }

            Setup();
            ValidationWindowSerializer.SerializeToStorage(null);
            Assert.IsNull(ValidationWindowSerializer.DeserializeFromStorage());

            Setup();
            ValidationWindowSerializer.SerializeToStorage(new List<ValidationError>());
            Assert.IsNull(ValidationWindowSerializer.DeserializeFromStorage());
        }
    }
}
