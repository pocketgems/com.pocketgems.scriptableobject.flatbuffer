using System.Reflection;
using PocketGems.Parameters.Common.DataTypes.Editor;

namespace PocketGems.Parameters.Common.PropertyTypes.Editor
{
    internal class DateTimePropertyType : BaseTimePropertyType
    {
        public DateTimePropertyType(PropertyInfo propertyInfo) : base(propertyInfo, propertyInfo.PropertyType.Name)
        {
        }

        protected override string SerializedType() => nameof(SerializableDateTime);
    }
}
