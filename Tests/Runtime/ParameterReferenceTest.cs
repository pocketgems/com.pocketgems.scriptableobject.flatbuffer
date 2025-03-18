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
            Assert.That(parameterReference.AssignedGUID, Is.Null);
            Assert.That(parameterReference.AssignedIdentifier, Is.Null);
            Assert.That(parameterReference.ToString(), Is.Not.Empty);

            parameterReference = new ParameterReference<IBaseInfo>(kTestGuid);
            Assert.That(parameterReference.AssignedGUID, Is.EqualTo(kTestGuid));
            Assert.That(parameterReference.AssignedIdentifier, Is.Null);
            Assert.That(parameterReference.ToString(), Is.Not.Empty);

            parameterReference = new ParameterReference<IBaseInfo>(kTestIdentifier, true);
            Assert.That(parameterReference.AssignedGUID, Is.Null);
            Assert.That(parameterReference.AssignedIdentifier, Is.EqualTo(kTestIdentifier));
            Assert.That(parameterReference.ToString(), Is.Not.Empty);
        }

        [Test]
        public void GetByGuid()
        {
            var parameterReference = new ParameterReference<IBaseInfo>(kTestGuid);
            Assert.That(parameterReference.Info, Is.EqualTo(_mockInfo));
        }

        [Test]
        public void GetByIdentifier()
        {
            var parameterReference = new ParameterReference<IBaseInfo>(kTestIdentifier, true);
            Assert.That(parameterReference.Info, Is.EqualTo(_mockInfo));
        }

        [Test]
        public void GetNothing()
        {
            var parameterReference = new ParameterReference<IBaseInfo>();
            Assert.That(parameterReference.Info, Is.Null);
        }

        [Test]
        public void MissingGuidInParameterManager()
        {
            var parameterReference = new ParameterReference<IBaseInfo>("bad guid");
            Assert.That(parameterReference.Info, Is.Null);
        }

        [Test]
        public void MissingIdentifierInParameterManager()
        {
            var parameterReference = new ParameterReference<IBaseInfo>("bad id", true);
            LogAssert.Expect(LogType.Error, new Regex("Cannot find.*"));
            Assert.That(parameterReference.Info, Is.Null);
        }

        [Test]
        public void NonExistentParameterManager()
        {
            Params.SetInstance(null);

            const string errorString = "Fetching Info before ParamsSetup.Setup() has been called.";

            var parameterReference = new ParameterReference<IBaseInfo>(kTestGuid);
            LogAssert.Expect(LogType.Error, errorString);
            Assert.That(parameterReference.Info, Is.Null);
            LogAssert.Expect(LogType.Error, errorString);
            Assert.That(parameterReference.ToString(), Is.Not.Empty);

            parameterReference = new ParameterReference<IBaseInfo>(kTestIdentifier, true);
            LogAssert.Expect(LogType.Error, errorString);
            Assert.That(parameterReference.Info, Is.Null);
            LogAssert.Expect(LogType.Error, errorString);
            Assert.That(parameterReference.ToString(), Is.Not.Empty);
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
                    Assert.That(reference.AssignedGUID, Is.Null);
                    Assert.That(reference.AssignedIdentifier, Is.EqualTo(identifier));
                }
                else
                {
                    Assert.That(reference.AssignedGUID, Is.EqualTo(guid));
                    Assert.That(reference.AssignedIdentifier, Is.Null);
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
            Assert.That(refList[0], Is.EqualTo(refNull));
            Assert.That(refList[1], Is.EqualTo(refA));
            Assert.That(refList[2], Is.EqualTo(refB));
            Assert.That(refList[3], Is.EqualTo(refC));
            Assert.That(refList[4], Is.EqualTo(refD));

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
                    Assert.That(ref1List[i].CompareTo(ref1List[j]), Is.Zero);
                }
            }

            // two nulls are equal
            Assert.That(refNull.CompareTo(new ParameterReference<IBaseInfo>()), Is.Zero);
        }

        [Test]
        [TestCase(null, false, false, false)]
        [TestCase(null, true, false, false)]
        [TestCase("", false, false, false)]
        [TestCase("", true, false, false)]
        [TestCase(kTestGuid, false, true, true)]
        [TestCase("blah", false, true, false)]
        [TestCase(kTestIdentifier, true, true, true)]
        [TestCase("blah", true, true, false)]
        public void InfoExists_HasAssignedValue(string value, bool isIdentifier, bool expectedHasAssignedValue, bool expectedInfoExists)
        {
            var parameterReference = new ParameterReference<IBaseInfo>(value, isIdentifier);
            Assert.That(parameterReference.HasAssignedValue, Is.EqualTo(expectedHasAssignedValue));
            if (isIdentifier && expectedHasAssignedValue && !expectedInfoExists)
                LogAssert.Expect(LogType.Error, new Regex("Cannot find info of type .*"));
            Assert.That(parameterReference.InfoExists, Is.EqualTo(expectedInfoExists));
        }
    }
}
