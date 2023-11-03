using System.Text.RegularExpressions;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace PocketGems.Parameters
{
    public class ParamsTest
    {
        [SetUp]
        public void SetUp()
        {
            Params.IsGettingSafe = true;
            Params.SetInstance(null);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            Params.IsGettingSafe = true;
            Params.SetInstance(null);
        }

        [Test]
        public void ParameterManager()
        {
            var pm = Substitute.For<IMutableParameterManager>();
            Params.SetInstance(pm);
            Assert.AreEqual(pm, Params.ParameterManager);
            Assert.AreEqual(pm, Params.MutableParameterManager);
        }

        [Test]
        public void GetIdentifier()
        {
            var pm = Substitute.For<IMutableParameterManager>();
            Params.SetInstance(pm);

            Params.Get<IMySpecialInfo>("blah");
            pm.Received().Get<IMySpecialInfo>("blah");
        }

        [Test]
        public void GetGUID()
        {
            var pm = Substitute.For<IMutableParameterManager>();
            Params.SetInstance(pm);

            Params.GetWithGUID<IMySpecialInfo>("blah");
            pm.Received().GetWithGUID<IMySpecialInfo>("blah");
        }

        [Test]
        public void Get()
        {
            var pm = Substitute.For<IMutableParameterManager>();
            Params.SetInstance(pm);

            Params.Get<IMySpecialInfo>();
            pm.Received().Get<IMySpecialInfo>();
        }

        [Test]
        public void GetSorted()
        {
            var pm = Substitute.For<IMutableParameterManager>();
            Params.SetInstance(pm);

            Params.GetSorted<IMySpecialInfo>();
            pm.Received().GetSorted<IMySpecialInfo>();
        }

        [Test]
        public void IsGettingSafe()
        {
            // default is true
            Params.IsGettingSafe = true;
            var pm = new ParameterManager();
            Assert.IsFalse(pm.HasGetBeenCalled);
            Params.SetInstance(pm);
            Assert.IsTrue(pm.IsGettingSafe);

            // default is false
            Params.IsGettingSafe = false;
            pm = new ParameterManager();
            Assert.IsFalse(pm.HasGetBeenCalled);
            Params.SetInstance(pm);
            Assert.IsFalse(pm.IsGettingSafe);

            // default is true but pm already had Get() called
            pm = new ParameterManager();
            _ = pm.Get<IMySpecialInfo>("some id");
            Assert.IsTrue(pm.HasGetBeenCalled);
            LogAssert.Expect(LogType.Error, new Regex(".*"));
            Params.SetInstance(pm);

            // default is true but switch to false
            Params.IsGettingSafe = true;
            pm = new ParameterManager();
            Assert.IsFalse(pm.HasGetBeenCalled);
            Params.SetInstance(pm);
            _ = pm.Get<IMySpecialInfo>("some id");
            Assert.IsTrue(pm.HasGetBeenCalled);
            LogAssert.Expect(LogType.Error, new Regex(".*"));
            Params.IsGettingSafe = false;
        }
    }
}
