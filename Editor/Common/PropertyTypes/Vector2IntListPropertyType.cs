using System.Reflection;
using PocketGems.Parameters.Common.Util.Editor;
using UnityEngine;

namespace PocketGems.Parameters.Common.PropertyTypes.Editor
{
    internal class Vector2IntListPropertyType : NDimensionListPropertyType
    {
        public Vector2IntListPropertyType(PropertyInfo propertyInfo) : base(propertyInfo, nameof(Vector2Int), FlatBufferFieldType.Int)
        {


        }

        protected override string[] ObjectFieldNames() => new string[] { nameof(Vector2Int.x), nameof(Vector2Int.y) };
    }
}
