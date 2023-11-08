using System.Reflection;
using PocketGems.Parameters.Util;
using UnityEngine;

namespace PocketGems.Parameters.PropertyTypes
{
    internal class Vector3IntListPropertyType : NDimensionListPropertyType
    {
        public Vector3IntListPropertyType(PropertyInfo propertyInfo) : base(propertyInfo, nameof(Vector3Int), FlatBufferFieldType.Int)
        {

        }

        protected override string[] ObjectFieldNames() => new string[] { nameof(Vector3Int.x), nameof(Vector3Int.y), nameof(Vector3Int.z) };
    }
}
