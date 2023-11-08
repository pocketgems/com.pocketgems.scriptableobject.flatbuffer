using System.Reflection;
using PocketGems.Parameters.LocalCSV;
using PocketGems.Parameters.Util;

namespace PocketGems.Parameters.PropertyTypes
{
    internal class StandardListPropertyType : BasePropertyType, IPropertyType
    {
        protected readonly string _typeKeyword;
        protected readonly FlatBufferFieldType _fieldType;

        public StandardListPropertyType(PropertyInfo propertyInfo, string typeKeyword, FlatBufferFieldType fieldType) : base(propertyInfo)
        {
            _typeKeyword = typeKeyword;
            _fieldType = fieldType;
        }

        public override string ScriptableObjectFieldDefinitionCode() =>
            $"public {_typeKeyword}[] {FieldName};";

        public override string ScriptableObjectPropertyImplementationCode() =>
            $"public IReadOnlyList<{_typeKeyword}> {PropertyName} => {FieldName};";

        public override string FlatBufferFieldDefinitionCode() =>
            $"private {_typeKeyword}[] {OverrideFieldName};";

        public override string FlatBufferPropertyImplementationCode()
        {
            return $"public IReadOnlyList<{_typeKeyword}> {PropertyName}\n" +
                   $"{{\n" +
                   $"    get\n" +
                   $"    {{\n" +
                   $"        if ({OverrideFieldName} != null)\n" +
                   $"            return {OverrideFieldName};\n" +
                   $"        return new ReadOnlyListContainer<{_typeKeyword}>(\n" +
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
                   $"    vector{FlatBufferStructPropertyName} = {tableName}.Create{FlatBufferStructPropertyName}Vector(_builder, data.{FieldName});";
        }

        public override string FlatBufferBuilderCode(string tableName)
        {
            return $"if (data.{FieldName}?.Length > 0)\n" +
                   $"    {tableName}.Add{FlatBufferStructPropertyName}(_builder, vector{FlatBufferStructPropertyName});";
        }

        public override string CSVBridgeColumnTypeText => $"{_typeKeyword}[]";

        public override string CSVBridgeReadFromCSVCode(string variableName) =>
            $"data.{FieldName} = {FromStringCode(variableName)};";

        public override string CSVBridgeUpdateCSVRowCode(string variableName)
        {
            var conversionClass = _typeKeyword == "bool" ? "BoolArray" : $"NumericArray<{_typeKeyword}>";
            return $"{variableName} = {nameof(CSVValueConverter)}.{conversionClass}.ToString(data.{FieldName});";
        }

        void IPropertyType.DefineFlatBufferSchema(SchemaBuilder schemaBuilder, string tableName)
        {
            schemaBuilder.DefineArrayField(tableName, FlatBufferStructPropertyName, _fieldType);
        }

        private string FromStringCode(string variableName)
        {
            var conversionClass = _typeKeyword == "bool" ? "BoolArray" : $"NumericArray<{_typeKeyword}>";
            return $"{nameof(CSVValueConverter)}.{conversionClass}.FromString({variableName})";
        }
    }
}
