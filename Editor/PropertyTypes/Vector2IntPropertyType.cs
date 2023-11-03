using System.Reflection;
using PocketGems.Parameters.Util;
using UnityEngine;

namespace PocketGems.Parameters.PropertyTypes
{
    internal class Vector2IntPropertyType : Vector2PropertyType
    {
        public Vector2IntPropertyType(PropertyInfo propertyInfo) : base(propertyInfo)
        {

        }

        protected override string VectorStructName => nameof(Vector2Int);
        protected override FlatBufferFieldType FlatBufferFieldType => FlatBufferFieldType.Int;
    }
}
