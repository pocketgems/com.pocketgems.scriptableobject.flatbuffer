#if ADDRESSABLE_PARAMS
using System.Reflection;
using PocketGems.Parameters.Common.Util.Editor;
using PocketGems.Parameters.LocalCSV;
using UnityEngine.AddressableAssets;

namespace PocketGems.Parameters.Common.PropertyTypes.Editor
{
    internal class AssetReferencePropertyType : BasePropertyType, IPropertyType
    {
        public AssetReferencePropertyType(PropertyInfo propertyInfo) : base(propertyInfo)
        {
        }

        protected virtual string ClassName => nameof(AssetReference);
        protected string SubObjectNameFlatBufferStructPropertyName => FlatBufferStructPropertyName + "SubObjectName";

        public override string ScriptableObjectFieldDefinitionCode() =>
            $"public {ClassName} {FieldName};";

        public override string ScriptableObjectPropertyImplementationCode() =>
            $"public {ClassName} {PropertyName} => {FieldName};";


        public override string FlatBufferFieldDefinitionCode() =>
            $"private {ClassName} {OverrideFieldName};";

        public override string FlatBufferPropertyImplementationCode()
        {
            return $"public {ClassName} {PropertyName}\n" +
                   $"{{\n" +
                   $"    get\n" +
                   $"    {{\n" +
                   $"        if ({OverrideFieldName} != null)\n" +
                   $"            return {OverrideFieldName};\n" +
                   $"        var assetRef = new {ClassName}(_fb.{FlatBufferStructPropertyName});\n" +
                   $"        var subObjectName = _fb.{SubObjectNameFlatBufferStructPropertyName};\n" +
                   $"        if (!string.IsNullOrEmpty(subObjectName))\n" +
                   $"            assetRef.SubObjectName = subObjectName;\n" +
                   $"        return assetRef;\n" +
                   $"    }}\n" +
                   $"}}";
        }

        // no need to set validateGuid to true in the this call to FromString - the check only checks the AssetDatabase in editor.
        // Also, this check an potentially give false positive if the asset is a new asset from an addresss catalog we're ab testing in.
        public override string FlatBufferEditPropertyCode(string variableName) =>
            $"{OverrideFieldName} = {nameof(CSVValueConverter)}.{ClassName}.FromString({variableName});";

        public override string FlatBufferRemoveEditCode() =>
            $"{OverrideFieldName} = null;";

        public override string FlatBufferBuilderPrepareCode(string tableName)
        {
            return
                $"StringOffset sharedString{FlatBufferStructPropertyName} = _builder.CreateSharedString(data.{PropertyName}.AssetGUID ?? \"\");\n" +
                $"StringOffset sharedString{SubObjectNameFlatBufferStructPropertyName} = _builder.CreateSharedString(data.{PropertyName}.SubObjectName ?? \"\");";
        }

        public override string FlatBufferBuilderCode(string tableName)
        {
            return $"{tableName}.Add{FlatBufferStructPropertyName}(_builder, sharedString{FlatBufferStructPropertyName});\n" +
                   $"{tableName}.Add{SubObjectNameFlatBufferStructPropertyName}(_builder, sharedString{SubObjectNameFlatBufferStructPropertyName});";
        }

        public override string CSVBridgeReadFromCSVCode(string variableName) =>
            $"data.{FieldName} = {nameof(CSVValueConverter)}.{ClassName}.FromString({variableName}, true);";

        public override string CSVBridgeUpdateCSVRowCode(string variableName) =>
            $"{variableName} = {nameof(CSVValueConverter)}.{ClassName}.ToString(data.{FieldName});";

        void IPropertyType.DefineFlatBufferSchema(SchemaBuilder schemaBuilder, string tableName)
        {
            schemaBuilder.DefineField(tableName, FlatBufferStructPropertyName, FlatBufferFieldType.String);
            schemaBuilder.DefineField(tableName, SubObjectNameFlatBufferStructPropertyName, FlatBufferFieldType.String);
        }
    }
}
#endif
