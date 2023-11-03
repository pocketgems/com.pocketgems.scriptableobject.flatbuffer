using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace PocketGems.Parameters.Types
{
    public class LocalizedStringTest
    {
        private ParameterLocalizationHandler.TranslateStringDelegate _delegate;

        [SetUp]
        public void SetUp()
        {
            var _delegate = ParameterLocalizationHandler.GlobalTranslateStringDelegate;
            ParameterLocalizationHandler.GlobalTranslateStringDelegate = null;

        }

        [TearDown]
        public void TearDown()
        {
            ParameterLocalizationHandler.GlobalTranslateStringDelegate = _delegate;
        }

        [Test]
        public void Test()
        {
            var l = new LocalizedString("blah");
            Assert.AreEqual("blah", l.Key);
            LogAssert.Expect(LogType.Error, new Regex(".*"));
            Assert.AreEqual("blah", l.Text);
            LogAssert.Expect(LogType.Error, new Regex(".*"));
            Assert.AreEqual("blah", l.ToString());
            ParameterLocalizationHandler.GlobalTranslateStringDelegate = key => "boo";
            Assert.AreEqual("boo", l.Text);
            Assert.AreEqual("boo", l.ToString());
        }
    }
}
