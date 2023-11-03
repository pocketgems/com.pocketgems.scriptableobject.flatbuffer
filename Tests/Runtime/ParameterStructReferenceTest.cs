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
            Params.SetInstance(_parameterManagerMock);
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
            var reference = new ParameterStructReferenceRuntime<IBaseStruct>(kTestGuid);
            Assert.AreEqual(_mockStruct, reference.Struct);
        }

        [Test]
        [TestCase("bad guid")]
        [TestCase(null)]
        public void MissingGuid(string guid)
        {
            var reference = new ParameterStructReferenceRuntime<IBaseStruct>(null);
            Assert.IsNull(reference.Struct);
        }

        [Test]
        public void NonExistentParameterManager()
        {
            Params.SetInstance(null);

            const string errorString = "Fetching Struct before ParamsSetup.Setup() has been called.";

            var reference = new ParameterStructReferenceRuntime<IBaseStruct>(kTestGuid);
            LogAssert.Expect(LogType.Error, errorString);
            Assert.IsNull(reference.Struct);
            LogAssert.Expect(LogType.Error, errorString);
            Assert.IsNotEmpty(reference.ToString());

            reference = new ParameterStructReferenceRuntime<IBaseStruct>(null);
            Assert.IsNotEmpty(reference.ToString());

            reference = new ParameterStructReferenceRuntime<IBaseStruct>("asdf");
            LogAssert.Expect(LogType.Error, errorString);
            Assert.IsNotEmpty(reference.ToString());
        }

        [Test]
        public void ParameterStructReferenceEditor()
        {
            var someStruct = new SomeStruct();
            someStruct.Name = "name";

            var reference = new ParameterStructReferenceEditor<ISomeStruct>(someStruct);
            var fetchedStruct = reference.Struct;
            Assert.AreEqual(someStruct.Name, fetchedStruct.Name);
        }
    }
}
