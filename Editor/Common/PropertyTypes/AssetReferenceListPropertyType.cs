#if ADDRESSABLE_PARAMS
using System.Reflection;
using PocketGems.Parameters.Common.Util.Editor;
using PocketGems.Parameters.LocalCSV;
using UnityEngine.AddressableAssets;

namespace PocketGems.Parameters.Common.PropertyTypes.Editor
{
    internal class AssetReferenceListPropertyType : BasePropertyType, IPropertyType
    {
        public AssetReferenceListPropertyType(PropertyInfo propertyInfo) : base(propertyInfo)
        {
        }

        private string SubObjectNameFlatBufferStructPropertyName => FlatBufferStructPropertyName + "SubObjectName";
        protected virtual string ClassName => nameof(AssetReference);

        public override string ScriptableObjectFieldDefinitionCode() =>
            $"public {ClassName}[] {FieldName};";

        public override string ScriptableObjectPropertyImplementationCode() =>
            $"public IReadOnlyList<{ClassName}> {PropertyName} => {FieldName};";

        public override string FlatBufferFieldDefinitionCode() =>
            $"private {ClassName}[] {OverrideFieldName};";

        public override string FlatBufferPropertyImplementationCode()
        {
            return $"public IReadOnlyList<{ClassName}> {PropertyName}\n" +
                   $"{{\n" +
                   $"    get\n" +
                   $"    {{\n" +
                   $"        if ({OverrideFieldName} != null)\n" +
                   $"            return {OverrideFieldName};\n" +
                   $"        return new ReadOnlyListContainer<{ClassName}>(\n" +
                   $"            () => _fb.{FlatBufferStructPropertyName}Length,\n" +
                   $"            i =>\n" +
                   $"            {{\n" +
                   $"                var reference = new {ClassName}(_fb.{FlatBufferStructPropertyName}(i));\n" +
                   $"                var subObjectName = _fb.{SubObjectNameFlatBufferStructPropertyName}(i);\n" +
                   $"                if (!string.IsNullOrEmpty(subObjectName))\n" +
                   $"                    reference.SubObjectName = subObjectName;\n" +
                   $"                return reference;\n" +
                   $"            }});\n" +
                   $"    }}\n" +
                   $"}}";
        }

        // no need to set validateGuid to true in the this call to FromString - the check only checks the AssetDatabase in editor.
        // Also, this check an potentially give false positive if the asset is a new asset from an addresss catalog we're ab testing in.
        public override string FlatBufferEditPropertyCode(string variableName) =>
            $"{OverrideFieldName} = {nameof(CSVValueConverter)}.{ClassName}Array.FromString({variableName});";

        public override string FlatBufferRemoveEditCode() =>
            $"{OverrideFieldName} = null;";

        public override string FlatBufferBuilderPrepareCode(string tableName)
        {
            return $"VectorOffset vector{FlatBufferStructPropertyName} = default;\n" +
                   $"VectorOffset vector{SubObjectNameFlatBufferStructPropertyName} = default;\n" +
                   $"if (data.{FieldName}?.Length > 0)\n" +
                   $"{{\n" +
                   $"    var guidStringOffsets = new StringOffset[data.{PropertyName}.Count];\n" +
                   $"    var subObjectNameStringOffsets = new StringOffset[data.{PropertyName}.Count];\n" +
                   $"    for (int j = 0; j < data.{PropertyName}.Count; j++)\n" +
                   $"    {{\n" +
                   $"        guidStringOffsets[j] = _builder.CreateSharedString(data.{FieldName}[j].AssetGUID ?? \"\");\n" +
                   $"        subObjectNameStringOffsets[j] = _builder.CreateSharedString(data.{FieldName}[j].SubObjectName ?? \"\");\n" +
                   $"    }}\n" +
                   $"    vector{FlatBufferStructPropertyName} = {tableName}.Create{FlatBufferStructPropertyName}Vector(_builder, guidStringOffsets);\n" +
                   $"    vector{SubObjectNameFlatBufferStructPropertyName} = {tableName}.Create{SubObjectNameFlatBufferStructPropertyName}Vector(_builder, subObjectNameStringOffsets);\n" +
                   $"}}";
        }

        public override string FlatBufferBuilderCode(string tableName)
        {
            return $"if (data.{FieldName}?.Length > 0)\n" +
                   $"{{\n" +
                   $"    {tableName}.Add{FlatBufferStructPropertyName}(_builder, vector{FlatBufferStructPropertyName});\n" +
                   $"    {tableName}.Add{SubObjectNameFlatBufferStructPropertyName}(_builder, vector{SubObjectNameFlatBufferStructPropertyName});\n" +
                   $"}}";
        }

        public override string CSVBridgeColumnTypeText => $"{ClassName}[]";

        public override string CSVBridgeReadFromCSVCode(string variableName) =>
            $"data.{FieldName} = {nameof(CSVValueConverter)}.{ClassName}Array.FromString({variableName}, true);";

        public override string CSVBridgeUpdateCSVRowCode(string variableName) =>
            $"{variableName} = {nameof(CSVValueConverter)}.{ClassName}Array.ToString(data.{FieldName});";

        void IPropertyType.DefineFlatBufferSchema(SchemaBuilder schemaBuilder, string tableName)
        {
            schemaBuilder.DefineArrayField(tableName, FlatBufferStructPropertyName, FlatBufferFieldType.String);
            schemaBuilder.DefineArrayField(tableName, SubObjectNameFlatBufferStructPropertyName, FlatBufferFieldType.String);
        }
    }
}
#endif
