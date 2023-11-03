using System;
using UnityEngine;

namespace PocketGems.Parameters.Parser
{
    public static class Vector3Parser
    {
        public static Vector3Int ParseVector3Int(string str)
        {
            var values = str.Split(':');
            if (values.Length != 3)
                throw new FormatException("string must have 3 elements delimited with :");
            return new Vector3Int(int.Parse(values[0]), int.Parse(values[1]), int.Parse(values[2]));
        }

        public static Vector3 ParseVector3Float(string str)
        {
            var values = str.Split(':');
            if (values.Length != 3)
                throw new FormatException("string must have 3 elements delimited with :");
            return new Vector3(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2]));
        }
    }
}
