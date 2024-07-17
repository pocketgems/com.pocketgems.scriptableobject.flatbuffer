using NUnit.Framework;
using UnityEngine;

namespace PocketGems.Parameters.Common.DataTypes.Editor
{
    public class SerializableTimeSpanTest
    {
        [Test]
        public void Serialization()
        {
            var timeSpan = new SerializableTimeSpan();
            timeSpan.Ticks = 123;
            Assert.AreEqual(123, timeSpan.Ticks);

            string json = JsonUtility.ToJson(timeSpan);
            var newTimeSpan = JsonUtility.FromJson<SerializableTimeSpan>(json);
            Assert.AreEqual(timeSpan.Ticks, newTimeSpan.Ticks);
        }

        [Test]
        public void Converters()
        {
            var timeSpan = new System.TimeSpan(1, 2, 3, 4, 5);
            Assert.AreEqual(1, timeSpan.Days);
            Assert.AreEqual(2, timeSpan.Hours);
            Assert.AreEqual(3, timeSpan.Minutes);
            Assert.AreEqual(4, timeSpan.Seconds);
            Assert.AreEqual(5, timeSpan.Milliseconds);

            var sTimeSpan = (SerializableTimeSpan)timeSpan;
            Assert.AreEqual(timeSpan.Ticks, sTimeSpan.Ticks);

            var newTimeSpan = (System.TimeSpan)sTimeSpan;
            Assert.AreEqual(1, newTimeSpan.Days);
            Assert.AreEqual(2, newTimeSpan.Hours);
            Assert.AreEqual(3, newTimeSpan.Minutes);
            Assert.AreEqual(4, newTimeSpan.Seconds);
            Assert.AreEqual(5, newTimeSpan.Milliseconds);
        }
    }
}
