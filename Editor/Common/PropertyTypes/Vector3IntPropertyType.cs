using System.Reflection;
using PocketGems.Parameters.Common.Util.Editor;
using UnityEngine;

namespace PocketGems.Parameters.Common.PropertyTypes.Editor
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
