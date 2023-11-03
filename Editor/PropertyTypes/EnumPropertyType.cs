using System.Reflection;
using PocketGems.Parameters.Util;

namespace PocketGems.Parameters.PropertyTypes
{
    internal class EnumPropertyType : StandardPropertyType
    {
        public EnumPropertyType(PropertyInfo propertyInfo) :
            base(propertyInfo, propertyInfo.PropertyType.Name, FlatBufferFieldType.Long)
        {
        }

        public override string FlatBufferPropertyImplementationCode() =>
            $"public {PropertyTypeName} {PropertyName} => {OverrideFieldName} ?? ({PropertyTypeName})_fb.{FlatBufferStructPropertyName};";

        public override string FlatBufferBuilderCode(string tableName) =>
            $"{tableName}.Add{FlatBufferStructPropertyName}(_builder, (long)data.{PropertyName});";
    }
}
