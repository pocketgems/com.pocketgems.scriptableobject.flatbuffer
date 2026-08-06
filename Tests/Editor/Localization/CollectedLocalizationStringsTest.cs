using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PocketGems.Parameters.Editor.Localization;

namespace PocketGems.Parameters.Localization.Editor
{
    public class CollectedLocalizationStringsTest
    {
        [Test]
        public void Success()
        {
            HashSet<string> keys = new();
            HashSet<string> scripts = new();
            var collected = new CollectedLocalizationStrings(keys, scripts);
            Assert.That(collected.Result, Is.EqualTo(ICollectedLocalizationStrings.State.Success));
            Assert.That(collected.ErrorMessage, Is.Null);

            var outputKeys = collected.LocalizationKeys.ToList();
            var outputScripts = collected.LocalizableScripts.ToList();

            Assert.That(outputKeys.Count, Is.EqualTo(keys.Count));
            foreach (var key in outputKeys)
                Assert.That(keys, Does.Contain(key));
            Assert.That(outputScripts.Count, Is.EqualTo(scripts.Count));
            foreach (var script in outputScripts)
                Assert.That(scripts, Does.Contain(script));
        }

        [Test]
        public void Error()
        {
            const string errorMsg = "errrr msg";
            var collected = new CollectedLocalizationStrings(errorMsg);
            Assert.That(collected.Result, Is.EqualTo(ICollectedLocalizationStrings.State.Error));
            Assert.That(collected.ErrorMessage, Is.EqualTo(errorMsg));
            Assert.That(collected.LocalizationKeys, Is.Null);
            Assert.That(collected.LocalizableScripts, Is.Null);
        }
    }
}
