using NSubstitute;
using NSubstitute.ReturnsExtensions;
using NUnit.Framework;
using PocketGems.Parameters.Interface;
using UnityEngine;
using UnityEngine.TestTools;

namespace PocketGems.Parameters
{
    public class ParameterStructReferenceTest
    {
        private const string kTestGuid = "some guid";
        private IMutableParameterManager _parameterManagerMock;
        private IBaseStruct _mockStruct;

        private interface ISomeStruct : IBaseStruct
        {
            string Name { get; }
        }

        private struct SomeStruct : ISomeStruct
        {
            public string Name { get; set; }
        }

        [SetUp]
        public void SetUp()
        {
            TearDown();

            _mockStruct = Substitute.For<IBaseStruct>();
            _parameterManagerMock = Substitute.For<IMutableParameterManager>();
            _parameterManagerMock.GetStructWithGuid<IBaseStruct>(default).ReturnsNullForAnyArgs();
            _parameterManagerMock.GetStructWithGuid<IBaseStruct>(kTestGuid).Returns(_mockStruct);
        }

        [TearDown]
        public void TearDown()
        {
            Params.SetInstance(null);
            EditorParams.Destroy();
        }

        [Test]
        public void GetStruct()
        {
            var reference = new ParameterStructReferenceRuntime<IBaseStruct>(_parameterManagerMock, kTestGuid);
            Assert.That(reference.Struct, Is.EqualTo(_mockStruct));
        }

        [Test]
        [TestCase("bad guid")]
        [TestCase(null)]
        public void MissingGuid(string guid)
        {
            var reference = new ParameterStructReferenceRuntime<IBaseStruct>(_parameterManagerMock, null);
            Assert.That(reference.Struct, Is.Null);
            Assert.That(reference.ToString(), Is.Not.Null);
        }

        [Test]
        public void NonExistentParameterManager()
        {
            const string errorString = "Must provide IParameterManager when constructing ParameterStructReference.";

            LogAssert.Expect(LogType.Error, errorString);
            var reference = new ParameterStructReferenceRuntime<IBaseStruct>(null, kTestGuid);
            Assert.That(reference.Struct, Is.Null);
            Assert.That(reference.ToString(), Is.Not.Null);
        }

        [Test]
        public void ParameterStructReferenceEditor()
        {
            var someStruct = new SomeStruct();
            someStruct.Name = "name";

            var reference = new ParameterStructReferenceEditor<ISomeStruct>(someStruct);
            var fetchedStruct = reference.Struct;
            Assert.That(fetchedStruct.Name, Is.EqualTo(someStruct.Name));
        }
    }
}
