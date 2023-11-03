using System.Text.RegularExpressions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using NUnit.Framework;
using PocketGems.Parameters.Interface;
using UnityEngine;
using UnityEngine.TestTools;

namespace PocketGems.Parameters
{
    public class ParameterReferenceTest
    {
        private const string kTestGuid = "some guid";
        private const string kTestIdentifier = "some id";
        private IMutableParameterManager _parameterManagerMock;
        private IBaseInfo _mockInfo;

        [SetUp]
        public void SetUp()
        {
            TearDown();

            _mockInfo = Substitute.For<IBaseInfo>();
            _parameterManagerMock = Substitute.For<IMutableParameterManager>();
            _parameterManagerMock.GetWithGUID<IBaseInfo>(default).ReturnsNullForAnyArgs();
            _parameterManagerMock.GetWithGUID<IBaseInfo>(kTestGuid).Returns(_mockInfo);
            _parameterManagerMock.Get<IBaseInfo>(default).ReturnsNullForAnyArgs();
            _parameterManagerMock.Get<IBaseInfo>(kTestIdentifier).Returns(_mockInfo);
            Params.SetInstance(_parameterManagerMock);
        }

        [TearDown]
        public void TearDown()
        {
            Params.SetInstance(null);
            EditorParams.Destroy();
        }

        [Test]
        public void Init()
        {
            var parameterReference = new ParameterReference<IBaseInfo>();
            Assert.IsNull(parameterReference.AssignedGUID);
            Assert.IsNull(parameterReference.AssignedIdentifier);
            Assert.IsNotEmpty(parameterReference.ToString());

            parameterReference = new ParameterReference<IBaseInfo>(kTestGuid);
            Assert.AreEqual(kTestGuid, parameterReference.AssignedGUID);
            Assert.IsNull(parameterReference.AssignedIdentifier);
            Assert.IsNotEmpty(parameterReference.ToString());

            parameterReference = new ParameterReference<IBaseInfo>(kTestIdentifier, true);
            Assert.IsNull(parameterReference.AssignedGUID);
            Assert.AreEqual(kTestIdentifier, parameterReference.AssignedIdentifier);
            Assert.IsNotEmpty(parameterReference.ToString());
        }

        [Test]
        public void GetByGuid()
        {
            var parameterReference = new ParameterReference<IBaseInfo>(kTestGuid);
            Assert.AreEqual(_mockInfo, parameterReference.Info);
        }

        [Test]
        public void GetByIdentifier()
        {
            var parameterReference = new ParameterReference<IBaseInfo>(kTestIdentifier, true);
            Assert.AreEqual(_mockInfo, parameterReference.Info);
        }

        [Test]
        public void GetNothing()
        {
            var parameterReference = new ParameterReference<IBaseInfo>();
            Assert.IsNull(parameterReference.Info);
        }

        [Test]
        public void MissingGuidInParameterManager()
        {
            var parameterReference = new ParameterReference<IBaseInfo>("bad guid");
            Assert.IsNull(parameterReference.Info);
        }

        [Test]
        public void MissingIdentifierInParameterManager()
        {
            var parameterReference = new ParameterReference<IBaseInfo>("bad id", true);
            LogAssert.Expect(LogType.Error, new Regex("Cannot find.*"));
            Assert.IsNull(parameterReference.Info);
        }

        [Test]
        public void NonExistentParameterManager()
        {
            Params.SetInstance(null);

            const string errorString = "Fetching Info before ParamsSetup.Setup() has been called.";

            var parameterReference = new ParameterReference<IBaseInfo>(kTestGuid);
            LogAssert.Expect(LogType.Error, errorString);
            Assert.IsNull(parameterReference.Info);
            LogAssert.Expect(LogType.Error, errorString);
            Assert.IsNotEmpty(parameterReference.ToString());

            parameterReference = new ParameterReference<IBaseInfo>(kTestIdentifier, true);
            LogAssert.Expect(LogType.Error, errorString);
            Assert.IsNull(parameterReference.Info);
            LogAssert.Expect(LogType.Error, errorString);
            Assert.IsNotEmpty(parameterReference.ToString());
        }
    }
}
