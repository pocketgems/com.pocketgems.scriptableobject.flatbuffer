using System;
using System.Reflection;
using PocketGems.Parameters.Common.DataTypes.Editor;

namespace PocketGems.Parameters.Common.PropertyTypes.Editor
{
    internal class DateTimeListPropertyType : BaseTimeListPropertyType
    {
        public DateTimeListPropertyType(PropertyInfo propertyInfo) : base(propertyInfo, nameof(DateTime), nameof(SerializableDateTime))
        {

        }

        protected override string[] ObjectFieldNames() => new string[] { nameof(DateTime.Ticks) };
    }
}
