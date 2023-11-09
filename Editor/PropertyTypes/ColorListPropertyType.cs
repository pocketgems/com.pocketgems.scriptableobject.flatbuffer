using System.Reflection;
using PocketGems.Parameters.Util;
using UnityEngine;

namespace PocketGems.Parameters.PropertyTypes
{
    internal class ColorListPropertyType : NDimensionListPropertyType
    {
        public ColorListPropertyType(PropertyInfo propertyInfo) : base(propertyInfo, nameof(Color), FlatBufferFieldType.Float)
        {

        }

        protected override string[] ObjectFieldNames() => new string[] { nameof(Color.r), nameof(Color.g), nameof(Color.b), nameof(Color.a) };
    }
}
