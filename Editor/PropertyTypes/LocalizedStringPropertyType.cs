using System.Reflection;
using PocketGems.Parameters.LocalCSV;
using PocketGems.Parameters.Util;

namespace PocketGems.Parameters.PropertyTypes
{
    internal class LocalizedStringPropertyType : BasePropertyType, IPropertyType
    {
        public LocalizedStringPropertyType(PropertyInfo propertyInfo) : base(propertyInfo)
        {
        }

        public override string ScriptableObjectFieldDefinitionCode() =>
            $"public string {FieldName};";

        public override string ScriptableObjectPropertyImplementationCode() =>
            $"public {PropertyTypeName} {PropertyName} => new {PropertyTypeName}({FieldName});";

        public override string FlatBufferFieldDefinitionCode() =>
            $"private string {OverrideFieldName};";

        public override string FlatBufferPropertyImplementationCode() =>
            $"public {PropertyTypeName} {PropertyName} => new {PropertyTypeName}({OverrideFieldName} ?? _fb.{FlatBufferStructPropertyName});";

        public override string FlatBufferEditPropertyCode(string variableName) =>
            $"{OverrideFieldName} = {FromStringCode(variableName)};";

        public override string FlatBufferRemoveEditCode() =>
            $"{OverrideFieldName} = null;";

        public override string FlatBufferBuilderPrepareCode(string tableName) =>
            $"var sharedString{FlatBufferStructPropertyName} = _builder.CreateSharedString(data.{PropertyName}.Key);";

        public override string FlatBufferBuilderCode(string tableName) =>
            $"{tableName}.Add{FlatBufferStructPropertyName}(_builder, sharedString{FlatBufferStructPropertyName});";

        public override string CSVBridgeReadFromCSVCode(string variableName) =>
            $"data.{FieldName} = {FromStringCode(variableName)};";

        public override string CSVBridgeUpdateCSVRowCode(string variableName) =>
            $"{variableName} = {nameof(CSVValueConverter)}.LocalizedString.ToString(data.{FieldName});";

        void IPropertyType.DefineFlatBufferSchema(SchemaBuilder schemaBuilder, string tableName) =>
            schemaBuilder.DefineField(tableName, FlatBufferStructPropertyName, FlatBufferFieldType.String);

        private string FromStringCode(string variableName) =>
            $"{nameof(CSVValueConverter)}.LocalizedString.FromString({variableName})";
    }
}
