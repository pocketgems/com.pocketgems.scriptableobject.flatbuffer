using System.Reflection;
using PocketGems.Parameters.Common.Util.Editor;
using PocketGems.Parameters.LocalCSV;

namespace PocketGems.Parameters.Common.PropertyTypes.Editor
{
    internal class StringListPropertyType : BasePropertyType, IPropertyType
    {
        protected override bool CanSupportLocalization => true;

        public StringListPropertyType(PropertyInfo propertyInfo) : base(propertyInfo) { }

        protected virtual string StringGetterSnippet => $"data.{FieldName}[j]";

        public override string ScriptableObjectFieldDefinitionCode() =>
            $"public string[] {FieldName};";

        public override string ScriptableObjectPropertyImplementationCode() =>
            $"public IReadOnlyList<string> {PropertyName} => {FieldName};";

        public override string ScriptableObjectCollectLocalizationStringsCode(string localizationKeysArgumentName, string localizedScriptArgumentName)
        {
            if (HasLocalizationKeyAttribute || HasLocalizedScriptAttribute)
            {
                return $"if ({FieldName} != null && {FieldName}.Length > 0)\n" +
                       $"    foreach (var str in {FieldName})\n" +
                       $"        if (!string.IsNullOrWhiteSpace(str))\n" +
                       $"            {(HasLocalizationKeyAttribute ? localizationKeysArgumentName : localizedScriptArgumentName)}.Add(str);";
            }

            return null;
        }

        public override string FlatBufferPropertyImplementationCode(int propertyIndex)
        {
            if (HasLocalizationKeyAttribute || HasLocalizedScriptAttribute)
            {
                string localizationCall;
                if (HasLocalizationKeyAttribute)
                    localizationCall = $"{nameof(ParameterLocalizationHandler)}.{nameof(ParameterLocalizationHandler.GetLocalizationKeyTranslation)}";
                else
                    localizationCall = $"{nameof(ParameterLocalizationHandler)}.{nameof(ParameterLocalizationHandler.GetLocalizableScriptTranslation)}";

                return $"public IReadOnlyList<string> {PropertyName}\n" +
                     $"{{\n" +
                     $"    get\n" +
                     $"    {{\n" +
                     $"        if (TryGetOverride<string[]>({propertyIndex}, out var val))\n" +
                     $"            return new ReadOnlyListContainer<string>(\n" +
                     $"                () => val.Length,\n" +
                     $"                i => {localizationCall}(val[i]));\n" +
                     $"        return new ReadOnlyListContainer<string>(\n" +
                     $"            () => _fb.{FlatBufferStructPropertyName}Length,\n" +
                     $"            i => {localizationCall}(_fb.{FlatBufferStructPropertyName}(i)));\n" +
                     $"    }}\n" +
                     $"}}";
            }

            return $"public IReadOnlyList<string> {PropertyName}\n" +
                   $"{{\n" +
                   $"    get\n" +
                   $"    {{\n" +
                   $"        if (TryGetOverride<string[]>({propertyIndex}, out var val))\n" +
                   $"            return val;\n" +
                   $"        return new ReadOnlyListContainer<string>(\n" +
                   $"            () => _fb.{FlatBufferStructPropertyName}Length,\n" +
                   $"            i => _fb.{FlatBufferStructPropertyName}(i));\n" +
                   $"    }}\n" +
                   $"}}";
        }

        public override string FlatBufferEditPropertyCode(int propertyIndex, int maxPropertyIndex, string variableName) =>
            $"return TrySetOverride<string[]>({propertyIndex}, {FromStringCode(variableName)}, {maxPropertyIndex}, out error);";

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
