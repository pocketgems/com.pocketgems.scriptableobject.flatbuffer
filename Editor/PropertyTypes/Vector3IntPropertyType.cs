using System.Reflection;
using PocketGems.Parameters.Util;
using UnityEngine;

namespace PocketGems.Parameters.PropertyTypes
{
    internal class Vector3IntPropertyType : Vector3PropertyType
    {
        public Vector3IntPropertyType(PropertyInfo propertyInfo) : base(propertyInfo)
        {

        }

        protected override string VectorStructName => nameof(Vector3Int);
        protected override FlatBufferFieldType FlatBufferFieldType => FlatBufferFieldType.Int;
    }
}
