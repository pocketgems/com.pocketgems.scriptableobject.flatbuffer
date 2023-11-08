using System.Reflection;
using PocketGems.Parameters.DataTypes;

namespace PocketGems.Parameters.PropertyTypes
{
    internal class DateTimePropertyType : BaseTimePropertyType
    {
        public DateTimePropertyType(PropertyInfo propertyInfo) : base(propertyInfo, propertyInfo.PropertyType.Name)
        {
        }

        protected override string SerializedType() => nameof(SerializableDateTime);
    }
}
