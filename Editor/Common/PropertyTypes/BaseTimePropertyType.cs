using System.Reflection;
using PocketGems.Parameters.Common.Util.Editor;
using PocketGems.Parameters.LocalCSV;

namespace PocketGems.Parameters.Common.PropertyTypes.Editor
{
    internal abstract class BaseTimePropertyType : StandardPropertyType
    {
        public BaseTimePropertyType(PropertyInfo propertyInfo, string typeKeyword) :
            base(propertyInfo, typeKeyword, FlatBufferFieldType.Long)
        {
        }

        protected abstract string SerializedType();

        public override string ScriptableObjectFieldDefinitionCode() =>
            $"public {SerializedType()} {FieldName};";

        public override string FlatBufferPropertyImplementationCode() =>
            $"public {PropertyTypeName} {PropertyName} => {OverrideFieldName} ?? new {PropertyTypeName}(_fb.{FlatBufferStructPropertyName});";

        public override string FlatBufferBuilderCode(string tableName) =>
            $"{tableName}.Add{FlatBufferStructPropertyName}(_builder, data.{PropertyName}.Ticks);";

        public override string CSVBridgeReadFromCSVCode(string variableName)
        {
            return $"data.{FieldName} = ({SerializedType()}){FromStringCode(variableName)};";
        }

        public override string CSVBridgeUpdateCSVRowCode(string variableName)
        {
            return $"{variableName} = {nameof(CSVValueConverter)}.{_typeKeyword}.ToString(data.{FieldName});";
        }

        protected override string FromStringCode(string variableName)
        {
            return $"{nameof(CSVValueConverter)}.{_typeKeyword}.FromString({variableName})";
        }
    }
}
