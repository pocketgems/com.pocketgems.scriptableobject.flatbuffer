using System.Reflection;
using System.Text;
using PocketGems.Parameters.Common.Util.Editor;
using PocketGems.Parameters.LocalCSV;

namespace PocketGems.Parameters.Common.PropertyTypes.Editor
{
    internal abstract class NDimensionListPropertyType : BasePropertyType, IPropertyType
    {
        protected readonly string _typeKeyword;
        protected readonly FlatBufferFieldType _fieldType;

        protected NDimensionListPropertyType(PropertyInfo propertyInfo, string typeKeyword, FlatBufferFieldType fieldType) : base(propertyInfo)
        {
            _typeKeyword = typeKeyword;
            _fieldType = fieldType;
        }

        protected abstract string[] ObjectFieldNames();

        public override string ScriptableObjectFieldDefinitionCode() =>
            $"public {_typeKeyword}[] {FieldName};";

        public override string ScriptableObjectPropertyImplementationCode() =>
            $"public IReadOnlyList<{_typeKeyword}> {PropertyName} => {FieldName};";

        public override string FlatBufferFieldDefinitionCode() =>
            $"private {_typeKeyword}[] {OverrideFieldName};";

        public override string FlatBufferPropertyImplementationCode()
        {
            int dimensions = ObjectFieldNames().Length;
            StringBuilder s = new StringBuilder();
            s.Append($"public IReadOnlyList<{_typeKeyword}> {PropertyName}\n" +
                     $"{{\n" +
                     $"    get\n" +
                     $"    {{\n" +
                     $"        if ({OverrideFieldName} != null)\n" +
                     $"            return {OverrideFieldName};\n" +
                     $"        return new ReadOnlyListContainer<{_typeKeyword}>(\n" +
                     $"            () => _fb.{FlatBufferStructPropertyName}Length / {dimensions},\n");
            s.Append($"            i =>\n" +
                     $"            {{\n" +
                     $"                int offset = i * {dimensions};\n" +
                     $"                return new {_typeKeyword}(\n");
            for (int i = 0; i < dimensions; i++)
            {
                string suffix = i == dimensions - 1 ? "" : ",";
                string offset = i == 0 ? "" : $" + {i}";
                s.Append($"                    _fb.{FlatBufferStructPropertyName}(offset{offset}){suffix}\n");
            }
            s.Append($"                    );\n");
            s.Append($"            }});\n" +
                     $"    }}\n" +
                     $"}}");
            return s.ToString();
        }

        public override string FlatBufferEditPropertyCode(string variableName) =>
            $"{OverrideFieldName} = {FromStringCode(variableName)};";

        public override string FlatBufferRemoveEditCode() =>
            $"{OverrideFieldName} = null;";

        public override string FlatBufferBuilderPrepareCode(string tableName)
        {
            var objectFieldNames = ObjectFieldNames();
            int dimension = objectFieldNames.Length;
            string arrayType = _fieldType.ToString().ToLower();
            StringBuilder s = new StringBuilder();
            s.Append($"VectorOffset vector{FlatBufferStructPropertyName} = default;\n" +
                     $"if (data.{FieldName}?.Length > 0)\n" +
                     $"{{\n" +
                     $"    {arrayType}[] tempArr = new {arrayType}[data.{FieldName}.Length * {dimension}];\n" +
                     $"    for (int j = 0; j < data.{FieldName}.Length; j++)\n" +
                     $"    {{\n" +
                     $"        var obj = data.{FieldName}[j];\n");
            for (int i = 0; i < dimension; i++)
            {
                var offset = i == 0 ? "" : $" + {i}";
                s.Append($"        tempArr[j * {dimension}{offset}] = obj.{objectFieldNames[i]};\n");
            }
            s.Append($"    }}\n" +
                     $"    vector{FlatBufferStructPropertyName} = {tableName}.Create{FlatBufferStructPropertyName}Vector(_builder, tempArr);\n" +
                     $"}}");
            return s.ToString();
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
            return $"{variableName} = {nameof(CSVValueConverter)}.ArrayFuncMapper<{_typeKeyword}>.ToString(data.{FieldName}, {nameof(CSVValueConverter)}.{_typeKeyword}.ToString);";
        }

        void IPropertyType.DefineFlatBufferSchema(SchemaBuilder schemaBuilder, string tableName)
        {
            schemaBuilder.DefineArrayField(tableName, FlatBufferStructPropertyName, _fieldType);
        }

        private string FromStringCode(string variableName)
        {
            return $"{nameof(CSVValueConverter)}.ArrayFuncMapper<{_typeKeyword}>.FromString({variableName}, {nameof(CSVValueConverter)}.{_typeKeyword}.FromString)";
        }
    }
}
