using System.Reflection;
using PocketGems.Parameters.Common.DataTypes.Editor;

namespace PocketGems.Parameters.Common.PropertyTypes.Editor
{
    internal class TimeSpanPropertyType : BaseTimePropertyType
    {
        public TimeSpanPropertyType(PropertyInfo propertyInfo) : base(propertyInfo, propertyInfo.PropertyType.Name)
        {
        }

        protected override string SerializedType() => nameof(SerializableTimeSpan);
    }
}
