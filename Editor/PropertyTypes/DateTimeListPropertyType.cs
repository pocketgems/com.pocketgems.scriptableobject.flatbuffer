using System;
using System.Reflection;
using PocketGems.Parameters.DataTypes;

namespace PocketGems.Parameters.PropertyTypes
{
    internal class DateTimeListPropertyType : BaseTimeListPropertyType
    {
        public DateTimeListPropertyType(PropertyInfo propertyInfo) : base(propertyInfo, nameof(DateTime), nameof(SerializableDateTime))
        {

        }

        protected override string[] ObjectFieldNames() => new string[] { nameof(DateTime.Ticks) };
    }
}
