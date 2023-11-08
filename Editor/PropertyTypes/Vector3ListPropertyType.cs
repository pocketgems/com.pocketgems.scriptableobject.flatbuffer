using System.Reflection;
using PocketGems.Parameters.Util;
using UnityEngine;

namespace PocketGems.Parameters.PropertyTypes
{
    internal class Vector3ListPropertyType : NDimensionListPropertyType
    {
        public Vector3ListPropertyType(PropertyInfo propertyInfo) : base(propertyInfo, nameof(Vector3), FlatBufferFieldType.Float)
        {

        }

        protected override string[] ObjectFieldNames() => new string[] { nameof(Vector3.x), nameof(Vector3.y), nameof(Vector3.z) };
    }
}
