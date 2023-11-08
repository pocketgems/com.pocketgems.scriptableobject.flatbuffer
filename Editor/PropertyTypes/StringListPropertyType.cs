using System.Reflection;
using PocketGems.Parameters.LocalCSV;
using PocketGems.Parameters.Util;

namespace PocketGems.Parameters.PropertyTypes
{
    internal class StringListPropertyType : BasePropertyType, IPropertyType
    {
        public StringListPropertyType(PropertyInfo propertyInfo) : base(propertyInfo)
        {
        }

        protected virtual string StringGetterSnippet => $"data.{FieldName}[j]";

        public override string ScriptableObjectFieldDefinitionCode() =>
            $"public string[] {FieldName};";

        public override string ScriptableObjectPropertyImplementationCode() =>
            $"public IReadOnlyList<string> {PropertyName} => {FieldName};";

        public override string FlatBufferFieldDefinitionCode() =>
            $"private string[] {OverrideFieldName};";

        public override string FlatBufferPropertyImplementationCode()
        {
            return $"public IReadOnlyList<string> {PropertyName}\n" +
                   $"{{\n" +
                   $"    get\n" +
                   $"    {{\n" +
                   $"        if ({OverrideFieldName} != null)\n" +
                   $"            return {OverrideFieldName};\n" +
                   $"        return new ReadOnlyListContainer<string>(\n" +
                   $"            () => _fb.{FlatBufferStructPropertyName}Length,\n" +
                   $"            i => _fb.{FlatBufferStructPropertyName}(i));\n" +
                   $"    }}\n" +
                   $"}}";
        }

        public override string FlatBufferEditPropertyCode(string variableName) =>
            $"{OverrideFieldName} = {FromStringCode(variableName)};";

        public override string FlatBufferRemoveEditCode() =>
            $"{OverrideFieldName} = null;";

        public override string FlatBufferBuilderPrepareCode(string tableName)
        {
            return $"VectorOffset vector{FlatBufferStructPropertyName} = default;\n" +
                   $"if (data.{FieldName}?.Length > 0)\n" +
                   $"{{\n" +
                   $"    var stringOffsets = new StringOffset[data.{PropertyName}.Count];\n" +
                   $"    for (int j = 0; j < data.{PropertyName}.Count; j++)\n" +
                   $"        stringOffsets[j] = _builder.CreateSharedString({StringGetterSnippet});\n" +
                   $"    vector{FlatBufferStructPropertyName} = {tableName}.Create{FlatBufferStructPropertyName}Vector(_builder, stringOffsets);\n" +
                   $"}}";
        }

        public override string FlatBufferBuilderCode(string tableName)
        {
            return $"if (data.{FieldName}?.Length > 0)\n" +
                   $"    {tableName}.Add{FlatBufferStructPropertyName}(_builder, vector{FlatBufferStructPropertyName});";
        }

        public override string CSVBridgeColumnTypeText => "string[]";

        public override string CSVBridgeReadFromCSVCode(string variableName) =>
            $"data.{FieldName} = {FromStringCode(variableName)};";

        public override string CSVBridgeUpdateCSVRowCode(string variableName) =>
            $"{variableName} = {nameof(CSVValueConverter)}.StringArray.ToString(data.{FieldName});";

        void IPropertyType.DefineFlatBufferSchema(SchemaBuilder schemaBuilder, string tableName)
        {
            schemaBuilder.DefineArrayField(tableName, FlatBufferStructPropertyName, FlatBufferFieldType.String);
        }

        private string FromStringCode(string variableName) =>
            $"{nameof(CSVValueConverter)}.StringArray.FromString({variableName})";
    }
}
