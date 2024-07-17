using System;
using System.Reflection;
using PocketGems.Parameters.Common.DataTypes.Editor;

namespace PocketGems.Parameters.Common.PropertyTypes.Editor
{
    internal class TimeSpanListPropertyType : BaseTimeListPropertyType
    {
        public TimeSpanListPropertyType(PropertyInfo propertyInfo) : base(propertyInfo, nameof(TimeSpan), nameof(SerializableTimeSpan))
        {

        }

        protected override string[] ObjectFieldNames() => new string[] { nameof(TimeSpan.Ticks) };
    }
}
