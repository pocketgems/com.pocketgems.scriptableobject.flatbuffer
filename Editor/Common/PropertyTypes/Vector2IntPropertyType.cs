using System.Reflection;
using PocketGems.Parameters.Common.Util.Editor;
using UnityEngine;

namespace PocketGems.Parameters.Common.PropertyTypes.Editor
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
