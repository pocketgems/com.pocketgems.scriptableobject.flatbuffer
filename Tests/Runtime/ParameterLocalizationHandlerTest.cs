using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace PocketGems.Parameters
{
    public class ParameterLocalizationHandlerTest
    {
        [SetUp]
        public void SetUp()
        {
            ParameterLocalizationHandler.GlobalTranslateStringDelegate = null;
        }

        [TearDown]
        public void TearDown()
        {
            ParameterLocalizationHandler.GlobalTranslateStringDelegate = null;
        }

        [Test]
        public void NoDelegate()
        {
            Assert.IsNull(ParameterLocalizationHandler.GlobalTranslateStringDelegate);

            // no delegate returns key
            const string text = "abc";
            LogAssert.Expect(LogType.Error, "GlobalTranslateStringDelegate not set prior to calling GetTranslation");
            Assert.AreEqual(text, ParameterLocalizationHandler.GetTranslation(text));
        }

        [Test]
        public void HasDelegate()
        {
            Assert.IsNull(ParameterLocalizationHandler.GlobalTranslateStringDelegate);
            ParameterLocalizationHandler.GlobalTranslateStringDelegate = key => $"loc+{key}";

            // return localization
            const string text = "abc";
            Assert.AreEqual($"loc+{text}", ParameterLocalizationHandler.GetTranslation(text));

            // use trimmed key
            const string text2 = " abc   ";
            Assert.AreEqual($"loc+{text2.Trim()}", ParameterLocalizationHandler.GetTranslation(text2));

            // whitespace key returns key
            const string text3 = "   ";
            Assert.AreEqual(text3, ParameterLocalizationHandler.GetTranslation(text3));
        }
    }
}
