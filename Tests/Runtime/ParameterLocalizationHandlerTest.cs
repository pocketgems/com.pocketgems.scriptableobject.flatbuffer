using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace PocketGems.Parameters
{
    public class ParameterLocalizationHandlerTest
    {
        private ParameterLocalizationHandler.TranslateLocalizationKeyDelegate _prevKeyDelegate;
        private ParameterLocalizationHandler.TranslateLocalizableScriptDelegate _prevScriptDelegate;

        [SetUp]
        public void SetUp()
        {
            _prevKeyDelegate = ParameterLocalizationHandler.GlobalTranslateLocalizationKeyDelegate;
            _prevScriptDelegate = ParameterLocalizationHandler.GlobalTranslateLocalizableScriptDelegate;

            ParameterLocalizationHandler.GlobalTranslateLocalizationKeyDelegate = null;
            ParameterLocalizationHandler.GlobalTranslateLocalizableScriptDelegate = null;

            Assert.That(ParameterLocalizationHandler.GlobalTranslateLocalizationKeyDelegate, Is.Null);
            Assert.That(ParameterLocalizationHandler.GlobalTranslateLocalizableScriptDelegate, Is.Null);
        }

        [TearDown]
        public void TearDown()
        {
            ParameterLocalizationHandler.GlobalTranslateLocalizationKeyDelegate = _prevKeyDelegate;
            ParameterLocalizationHandler.GlobalTranslateLocalizableScriptDelegate = _prevScriptDelegate;
        }

        [Test]
        public void NoKeyDelegate()
        {
            const string text = "abc";
            LogAssert.Expect(LogType.Error, "GlobalTranslateLocalizationKeyDelegate not set prior to calling GetLocalizationKeyTranslation");

            // no delegate returns key
            Assert.That(ParameterLocalizationHandler.GetLocalizationKeyTranslation(text), Is.EqualTo(text));
        }

        [Test]
        [TestCase("abc", "key+abc+key")]
        [TestCase("abc   ", "key+abc   +key")]
        [TestCase("  abc   ", "key+  abc   +key")]
        [TestCase("    ", "    ")]
        [TestCase("", "")]
        [TestCase(null, null)]
        public void KeyDelegate(string input, string expectedOutput)
        {
            ParameterLocalizationHandler.GlobalTranslateLocalizationKeyDelegate = key => $"key+{key}+key";
            Assert.That(ParameterLocalizationHandler.GetLocalizationKeyTranslation(input), Is.EqualTo(expectedOutput));
        }

        [Test]
        public void NoScriptDelegate()
        {
            const string text = "abc";
            LogAssert.Expect(LogType.Error, "GlobalTranslateLocalizableScriptDelegate not set prior to calling GetLocalizableScriptTranslation");

            // no delegate returns key
            Assert.That(ParameterLocalizationHandler.GetLocalizableScriptTranslation(text), Is.EqualTo(text));
        }

        [Test]
        [TestCase("abc", "script+abc+script")]
        [TestCase("abc   ", "script+abc   +script")]
        [TestCase("  abc   ", "script+  abc   +script")]
        [TestCase("    ", "    ")]
        [TestCase("", "")]
        [TestCase(null, null)]
        public void ScriptDelegate(string input, string expectedOutput)
        {
            ParameterLocalizationHandler.GlobalTranslateLocalizableScriptDelegate = key => $"script+{key}+script";
            Assert.That(ParameterLocalizationHandler.GetLocalizableScriptTranslation(input), Is.EqualTo(expectedOutput));
        }
    }
}
