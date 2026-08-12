using System.Text.RegularExpressions;
using NUnit.Framework;
using PocketGems.Parameters.Util;

namespace PocketGems.Parameters
{
    public class GuidConverterTest
    {
        private GuidConverter _guidConverter;

        [SetUp]
        public void SetUp()
        {
            _guidConverter = new GuidConverter();
        }

        [TearDown]
        public void TearDown()
        {
            _guidConverter.Dispose();
            _guidConverter = null;
        }

        [Test]
        public void ToGuid()
        {
            const string keyPath = "EventInfo[Adventure].EventRewards[1].Transaction";
            var guid = _guidConverter.ToGuid(keyPath);

            // guid formatted: 32 lowercase hex chars
            Assert.That(Regex.IsMatch(guid, "^[0-9a-f]{32}$"), Is.True);

            // deterministic (within & across instances) & input sensitive
            Assert.That(_guidConverter.ToGuid(keyPath), Is.EqualTo(guid));
            using (var otherGenerator = new GuidConverter())
                Assert.That(otherGenerator.ToGuid(keyPath), Is.EqualTo(guid));
            Assert.That(_guidConverter.ToGuid(keyPath + "x"), Is.Not.EqualTo(guid));

            // known vector: the hash can never change across versions or platforms since it's baked
            // into parameter data and used to resolve key path addressed overrides at runtime
            Assert.That(guid, Is.EqualTo("5d5da7d34495724757628f84439d5e5c"));
        }

        [Test]
        public void ToGuidUnassigned()
        {
            // null/empty pass through unhashed to preserve "not assigned" semantics
            Assert.That(_guidConverter.ToGuid(null), Is.Null);
            Assert.That(_guidConverter.ToGuid(""), Is.EqualTo(""));
        }
    }
}
