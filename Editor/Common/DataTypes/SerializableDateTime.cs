using System;
using UnityEngine;

namespace PocketGems.Parameters.Common.DataTypes.Editor
{
    [Serializable]
    public class SerializableDateTime
    {
        [SerializeField] public long Ticks;

        public static implicit operator DateTime(SerializableDateTime serializableDateTime) =>
            new DateTime(serializableDateTime.Ticks);

        public static explicit operator SerializableDateTime(DateTime d) =>
            new SerializableDateTime { Ticks = d.Ticks };
    }
}


