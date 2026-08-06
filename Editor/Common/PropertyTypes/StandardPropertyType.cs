using System.Reflection;
using PocketGems.Parameters.Common.Util.Editor;
using PocketGems.Parameters.LocalCSV;

namespace PocketGems.Parameters.Common.PropertyTypes.Editor
{
    internal class StandardPropertyType : BasePropertyType, IPropertyType
    {
        protected readonly string _typeKeyword;
        private readonly FlatBufferFieldType _fieldType;

        public StandardPropertyType(PropertyInfo propertyInfo, string typeKeyword, FlatBufferFieldType fieldType) : base(propertyInfo)
        {
            _typeKeyword = typeKeyword;
            _fieldType = fieldType;
        }

        public override string ScriptableObjectFieldDefinitionCode() =>
            $"public {_typeKeyword} {FieldName};";

        public override string ScriptableObjectPropertyImplementationCode() =>
            $"public {_typeKeyword} {PropertyName} => {FieldName};";

        public override string FlatBufferPropertyImplementationCode(int propertyIndex) =>
            $"public {_typeKeyword} {PropertyName} => TryGetOverride<{_typeKeyword}>({propertyIndex}, out var val) ? val : _fb.{FlatBufferStructPropertyName};";

        public override string FlatBufferEditPropertyCode(int propertyIndex, int maxPropertyIndex, string variableName) =>
            $"return TrySetOverride<{_typeKeyword}>({propertyIndex}, {FromStringCode(variableName)}, {maxPropertyIndex}, out error);";

        public override string FlatBufferBuilderPrepareCode(string tableName) => null;

        public override string FlatBufferBuilderCode(string tableName) =>
            $"{tableName}.Add{FlatBufferStructPropertyName}(_builder, data.{PropertyName});";

        public override string CSVBridgeColumnTypeText => _typeKeyword;

        public override string CSVBridgeReadFromCSVCode(string variableName)
        {
            return $"data.{FieldName} = {FromStringCode(variableName)};";
        }

        public override string CSVBridgeUpdateCSVRowCode(string variableName)
        {
            var conversionClass = _typeKeyword == "bool" ? "Bool" : $"Numeric<{_typeKeyword}>";
            return $"{variableName} = {nameof(CSVValueConverter)}.{conversionClass}.ToString(data.{FieldName});";
        }

        void IPropertyType.DefineFlatBufferSchema(SchemaBuilder schemaBuilder, string tableName)
        {
            schemaBuilder.DefineField(tableName, FlatBufferStructPropertyName, _fieldType);
        }

        protected virtual string FromStringCode(string variableName)
        {
            var conversionClass = _typeKeyword == "bool" ? "Bool" : $"Numeric<{_typeKeyword}>";
            return $"{nameof(CSVValueConverter)}.{conversionClass}.FromString({variableName})";
        }
    }
}
