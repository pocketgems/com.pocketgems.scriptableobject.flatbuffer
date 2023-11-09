using System;
using System.Reflection;
using PocketGems.Parameters.DataTypes;

namespace PocketGems.Parameters.PropertyTypes
{
    internal class TimeSpanListPropertyType : BaseTimeListPropertyType
    {
        public TimeSpanListPropertyType(PropertyInfo propertyInfo) : base(propertyInfo, nameof(TimeSpan), nameof(SerializableTimeSpan))
        {

        }

        protected override string[] ObjectFieldNames() => new string[] { nameof(TimeSpan.Ticks) };
    }
}
