using NUnit.Framework;
using UnityEngine;

namespace PocketGems.Parameters.Common.DataTypes.Editor
{
    public class SerializableDateTimeTest
    {
        [Test]
        public void Serialization()
        {
            var dateTime = new SerializableDateTime();
            dateTime.Ticks = 123;
            Assert.AreEqual(123, dateTime.Ticks);

            string json = JsonUtility.ToJson(dateTime);
            var newDateTime = JsonUtility.FromJson<SerializableDateTime>(json);
            Assert.AreEqual(dateTime.Ticks, newDateTime.Ticks);
        }

        [Test]
        public void Converters()
        {
            var dateTime = new System.DateTime(1, 2, 3, 4, 5, 6);
            Assert.AreEqual(1, dateTime.Year);
            Assert.AreEqual(2, dateTime.Month);
            Assert.AreEqual(3, dateTime.Day);
            Assert.AreEqual(4, dateTime.Hour);
            Assert.AreEqual(5, dateTime.Minute);
            Assert.AreEqual(6, dateTime.Second);

            var sDateTime = (SerializableDateTime)dateTime;
            Assert.AreEqual(dateTime.Ticks, sDateTime.Ticks);

            var newDateTime = (System.DateTime)sDateTime;
            Assert.AreEqual(1, newDateTime.Year);
            Assert.AreEqual(2, newDateTime.Month);
            Assert.AreEqual(3, newDateTime.Day);
            Assert.AreEqual(4, newDateTime.Hour);
            Assert.AreEqual(5, newDateTime.Minute);
            Assert.AreEqual(6, newDateTime.Second);
        }
    }
}
