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
            Assert.That(Params.IsGettingSafe, Is.True);
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

            Params.Get<ISubInterfaceAInfo>("blah");
            pm.Received().Get<ISubInterfaceAInfo>("blah");
        }

        [Test]
        public void GetGUID()
        {
            var pm = Substitute.For<IMutableParameterManager>();
            Params.SetInstance(pm);

            Params.GetWithGUID<ISubInterfaceAInfo>("blah");
            pm.Received().GetWithGUID<ISubInterfaceAInfo>("blah");
        }

        [Test]
        public void GetT()
        {
            var pm = Substitute.For<IMutableParameterManager>();
            Params.SetInstance(pm);

            Params.Get<ISubInterfaceAInfo>();
            pm.Received().Get<ISubInterfaceAInfo>();
        }

        [Test]
        public void Get()
        {
            var pm = Substitute.For<IMutableParameterManager>();
            Params.SetInstance(pm);

            var theType = typeof(ISubInterfaceAInfo);
            Params.Get("id", theType);
            pm.Received().Get("id", theType);
        }

        [Test]
        public void GetSorted()
        {
            var pm = Substitute.For<IMutableParameterManager>();
            Params.SetInstance(pm);

            Params.GetSorted<ISubInterfaceAInfo>();
            pm.Received().GetSorted<ISubInterfaceAInfo>();
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
            _ = pm.Get<ISubInterfaceAInfo>("some id");
            Assert.IsTrue(pm.HasGetBeenCalled);
            LogAssert.Expect(LogType.Error, new Regex(".*"));
            Params.SetInstance(pm);

            // default is true but switch to false
            Params.IsGettingSafe = true;
            pm = new ParameterManager();
            Assert.IsFalse(pm.HasGetBeenCalled);
            Params.SetInstance(pm);
            _ = pm.Get<ISubInterfaceAInfo>("some id");
            Assert.IsTrue(pm.HasGetBeenCalled);
            LogAssert.Expect(LogType.Error, new Regex(".*"));
            Params.IsGettingSafe = false;
        }
    }
}
