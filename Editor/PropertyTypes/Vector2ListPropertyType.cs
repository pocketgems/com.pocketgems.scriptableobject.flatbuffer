using System.Reflection;
using PocketGems.Parameters.Util;
using UnityEngine;

namespace PocketGems.Parameters.PropertyTypes
{
    internal class Vector2ListPropertyType : NDimensionListPropertyType
    {
        public Vector2ListPropertyType(PropertyInfo propertyInfo) : base(propertyInfo, nameof(Vector2), FlatBufferFieldType.Float)
        {

        }

        protected override string[] ObjectFieldNames() => new string[] { nameof(Vector2.x), nameof(Vector2.y) };
    }
}
