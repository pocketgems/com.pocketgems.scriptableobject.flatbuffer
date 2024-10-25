using System.Collections.Generic;
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

        [Test]
        public void Comparable()
        {
            ParameterReference<IBaseInfo> CreateReference(string guid, string identifier, bool refIsIdentifier)
            {
                var mockInfo = Substitute.For<IBaseInfo>();
                mockInfo.Identifier.Returns(identifier);
                _parameterManagerMock.GetWithGUID<IBaseInfo>(guid).Returns(mockInfo);
                _parameterManagerMock.Get<IBaseInfo>(identifier).Returns(mockInfo);
                var reference = new ParameterReference<IBaseInfo>(refIsIdentifier ? identifier : guid, refIsIdentifier);
                Assert.AreEqual(identifier, reference.Info.Identifier);
                if (refIsIdentifier)
                {
                    Assert.IsNull(reference.AssignedGUID);
                    Assert.AreEqual(identifier, reference.AssignedIdentifier);
                }
                else
                {
                    Assert.AreEqual(guid, reference.AssignedGUID);
                    Assert.IsNull(reference.AssignedIdentifier);
                }
                return reference;
            }

            var refNull = new ParameterReference<IBaseInfo>();
            var refA = CreateReference("def", "A", true);
            var refB = CreateReference("abc", "B", false);
            var refC = CreateReference("aaa", "C", true);
            var refD = CreateReference("222", "D", false);
            var refList = new List<ParameterReference<IBaseInfo>> { refC, refB, refA, refNull, refD };
            refList.Sort();

            // sorted by identifier
            Assert.AreEqual(refNull, refList[0]);
            Assert.AreEqual(refA, refList[1]);
            Assert.AreEqual(refB, refList[2]);
            Assert.AreEqual(refC, refList[3]);
            Assert.AreEqual(refD, refList[4]);

            var ref1_1 = CreateReference("abc", "1", true);
            var ref1_2 = CreateReference("abc", "1", true);
            var ref1_3 = CreateReference("abc", "1", false);
            var ref1_4 = CreateReference("abc", "1", false);
            var ref1List = new List<ParameterReference<IBaseInfo>> { ref1_1, ref1_2, ref1_3, ref1_4 };
            for (int i = 0; i < ref1List.Count; i++)
            {
                for (int j = 0; j < ref1List.Count; j++)
                {
                    // verify all of these combinations are equal
                    Assert.AreEqual(0, ref1List[i].CompareTo(ref1List[j]));
                }
            }

            // two nulls are equal
            Assert.AreEqual(0, refNull.CompareTo(new ParameterReference<IBaseInfo>()));
        }
    }
}
