using System.Reflection;
using PocketGems.Parameters.Common.Util.Editor;
using UnityEngine;

namespace PocketGems.Parameters.Common.PropertyTypes.Editor
{
    internal class Vector3ListPropertyType : NDimensionListPropertyType
    {
        public Vector3ListPropertyType(PropertyInfo propertyInfo) : base(propertyInfo, nameof(Vector3), FlatBufferFieldType.Float)
        {

        }

        protected override string[] ObjectFieldNames() => new string[] { nameof(Vector3.x), nameof(Vector3.y), nameof(Vector3.z) };
    }
}
