using System.Collections.Generic;
using System.Text.RegularExpressions;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace PocketGems.Parameters
{
    public class ParamsTest
    {
        private IMutableParameterManager _mockedParameterManager;
        private ISubInterfaceAInfo _mockedInfo;
        private const string SomeId = "someId";
        private const string SomeGUID = "someGUID";

        [SetUp]
        public void SetUp()
        {
            _mockedParameterManager = Substitute.For<IMutableParameterManager>();
            _mockedInfo = Substitute.For<ISubInterfaceAInfo>();

            Params.IsGettingSafe = true;
            Assert.That(Params.IsGettingSafe, Is.True);
            Params.SetInstance(null);
            Params.SetInstance(_mockedParameterManager);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            Params.IsGettingSafe = true;
            Params.SetInstance(null);
        }

        [Test]
        public void GetParameterManager()
        {
            Assert.That(Params.ParameterManager, Is.EqualTo(_mockedParameterManager));
            Assert.That(Params.MutableParameterManager, Is.EqualTo(_mockedParameterManager));
        }

        [Test]
        public void GetTIdentifier()
        {
            _mockedParameterManager.Get<ISubInterfaceAInfo>(SomeId).Returns(_mockedInfo);
            Assert.That(Params.Get<ISubInterfaceAInfo>(SomeId), Is.EqualTo(_mockedInfo));
            _mockedParameterManager.Received().Get<ISubInterfaceAInfo>(SomeId);
        }

        [Test]
        public void GetGUIDT()
        {
            _mockedParameterManager.GetWithGUID<ISubInterfaceAInfo>(SomeGUID).Returns(_mockedInfo);
            Assert.That(Params.GetWithGUID<ISubInterfaceAInfo>(SomeGUID), Is.EqualTo(_mockedInfo));
            _mockedParameterManager.Received().GetWithGUID<ISubInterfaceAInfo>(SomeGUID);
        }

        [Test]
        public void GetT()
        {
            IEnumerable<ISubInterfaceAInfo> expectedList = new List<ISubInterfaceAInfo>();
            _mockedParameterManager.Get<ISubInterfaceAInfo>().Returns(expectedList);
            Assert.That(Params.Get<ISubInterfaceAInfo>(), Is.EqualTo(expectedList));
            _mockedParameterManager.Received().Get<ISubInterfaceAInfo>();
        }

        [Test]
        public void Get()
        {
            var expectedType = typeof(ISubInterfaceAInfo);
            _mockedParameterManager.Get(SomeId, expectedType).Returns(_mockedInfo);
            Assert.That(Params.Get(SomeId, expectedType), Is.EqualTo(_mockedInfo));
            _mockedParameterManager.Received().Get(SomeId, expectedType);
        }

        [Test]
        public void TryGetTrue()
        {
            // mock the response from the parameter manager
            _mockedParameterManager.TryGet<ISubInterfaceAInfo>(SomeId, out _).Returns(x =>
            {
                x[1] = _mockedInfo;
                return true;
            });

            // test the cal
            Assert.That(Params.TryGet<ISubInterfaceAInfo>(SomeId, out var info), Is.True);
            Assert.That(info, Is.EqualTo(_mockedInfo));

            // assert the received call
            _mockedParameterManager.Received().TryGet<ISubInterfaceAInfo>(SomeId, out _);
        }

        [Test]
        public void TryGetFalse()
        {
            // mock the response from the parameter manager
            _mockedParameterManager.TryGet<ISubInterfaceAInfo>(SomeId, out _).Returns(x =>
            {
                x[1] = null;
                return false;
            });

            // test the cal
            Assert.That(Params.TryGet<ISubInterfaceAInfo>(SomeId, out var info), Is.False);
            Assert.That(info, Is.Null);

            // assert the received call
            _mockedParameterManager.Received().TryGet<ISubInterfaceAInfo>(SomeId, out _);
        }

        [Test]
        public void GetSortedT()
        {
            IEnumerable<ISubInterfaceAInfo> expectedList = new List<ISubInterfaceAInfo>();
            _mockedParameterManager.GetSorted<ISubInterfaceAInfo>().Returns(expectedList);
            Assert.That(Params.GetSorted<ISubInterfaceAInfo>(), Is.EqualTo(expectedList));
            _mockedParameterManager.Received().GetSorted<ISubInterfaceAInfo>();
        }

        [Test]
        public void IsGettingSafe()
        {
            // default is true
            Params.IsGettingSafe = true;
            ParameterManager parameterManager = new();
            Assert.IsFalse(parameterManager.HasGetBeenCalled);
            Params.SetInstance(parameterManager);
            Assert.IsTrue(parameterManager.IsGettingSafe);

            // default is false
            Params.IsGettingSafe = false;
            parameterManager = new();
            Assert.IsFalse(parameterManager.HasGetBeenCalled);
            Params.SetInstance(parameterManager);
            Assert.IsFalse(parameterManager.IsGettingSafe);

            // default is true but pm already had Get() called
            parameterManager = new();
            _ = parameterManager.Get<ISubInterfaceAInfo>("some id");
            Assert.IsTrue(parameterManager.HasGetBeenCalled);
            LogAssert.Expect(LogType.Error, new Regex(".*"));
            Params.SetInstance(parameterManager);

            // default is true but switch to false
            Params.IsGettingSafe = true;
            parameterManager = new();
            Assert.IsFalse(parameterManager.HasGetBeenCalled);
            Params.SetInstance(parameterManager);
            _ = parameterManager.Get<ISubInterfaceAInfo>("some id");
            Assert.IsTrue(parameterManager.HasGetBeenCalled);
            LogAssert.Expect(LogType.Error, new Regex(".*"));
            Params.IsGettingSafe = false;
        }
    }
}
