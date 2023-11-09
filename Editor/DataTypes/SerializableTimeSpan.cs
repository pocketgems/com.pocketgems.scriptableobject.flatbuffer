using System;
using UnityEngine;

namespace PocketGems.Parameters.DataTypes
{
    [Serializable]
    public class SerializableTimeSpan
    {
        [SerializeField] public long Ticks;

        public static implicit operator TimeSpan(SerializableTimeSpan serializableTimeSpan) =>
            TimeSpan.FromTicks(serializableTimeSpan.Ticks);

        public static explicit operator SerializableTimeSpan(TimeSpan t) =>
            new SerializableTimeSpan { Ticks = t.Ticks };
    }
}
