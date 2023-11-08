using System.Reflection;
using PocketGems.Parameters.DataTypes;

namespace PocketGems.Parameters.PropertyTypes
{
    internal class TimeSpanPropertyType : BaseTimePropertyType
    {
        public TimeSpanPropertyType(PropertyInfo propertyInfo) : base(propertyInfo, propertyInfo.PropertyType.Name)
        {
        }

        protected override string SerializedType() => nameof(SerializableTimeSpan);
    }
}
