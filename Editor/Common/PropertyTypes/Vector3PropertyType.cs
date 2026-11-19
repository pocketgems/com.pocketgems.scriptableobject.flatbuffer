using System.Reflection;
using PocketGems.Parameters.Common.Util.Editor;
using PocketGems.Parameters.LocalCSV;
using UnityEngine;

namespace PocketGems.Parameters.Common.PropertyTypes.Editor
{
    internal class Vector3PropertyType : BasePropertyType, IPropertyType
    {
        public Vector3PropertyType(PropertyInfo propertyInfo) : base(propertyInfo)
        {

        }

        protected virtual string VectorStructName => nameof(Vector3);
        protected virtual FlatBufferFieldType FlatBufferFieldType => FlatBufferFieldType.Float;

        public override string ScriptableObjectFieldDefinitionCode() =>
            $"public {VectorStructName} {FieldName};";

        public override string ScriptableObjectPropertyImplementationCode() =>
            $"public {VectorStructName} {PropertyName} => {FieldName};";

        public override string FlatBufferPropertyImplementationCode(int propertyIndex)
        {
            return $"public {VectorStructName} {PropertyName}\n" +
                   $"{{\n" +
                   $"    get\n" +
                   $"    {{\n" +
                   $"        if (TryGetOverride<{VectorStructName}>({propertyIndex}, out var val))\n" +
                   $"            return val;\n" +
                   $"        var fb = FlatBufferData();\n" +
                   $"        return new {VectorStructName}(fb.{fbPropertyX}, fb.{fbPropertyY}, fb.{fbPropertyZ});\n" +
                   $"    }}\n" +
                   $"}}";
        }

        public override string FlatBufferEditPropertyCode(int propertyIndex, int maxPropertyIndex, string variableName) =>
            $"return TrySetOverride<{VectorStructName}>({propertyIndex}, {FromStringCode(variableName)}, {maxPropertyIndex}, out error);";

        private string fbPropertyX => FlatBufferStructPropertyName + "X";
        private string fbPropertyY => FlatBufferStructPropertyName + "Y";
        private string fbPropertyZ => FlatBufferStructPropertyName + "Z";

        public override string FlatBufferBuilderPrepareCode(string tableName) => null;

        public override string FlatBufferBuilderCode(string tableName)
        {
            return $"{tableName}.Add{fbPropertyX}(_builder, data.{PropertyName}.x);\n" +
                   $"{tableName}.Add{fbPropertyY}(_builder, data.{PropertyName}.y);\n" +
                   $"{tableName}.Add{fbPropertyZ}(_builder, data.{PropertyName}.z);";
        }

        public override string CSVBridgeReadFromCSVCode(string variableName) =>
            $"data.{FieldName} = {FromStringCode(variableName)};";

        public override string CSVBridgeUpdateCSVRowCode(string variableName) =>
            $"{variableName} = {nameof(CSVValueConverter)}.{VectorStructName}.ToString(data.{FieldName});";

        void IPropertyType.DefineFlatBufferSchema(SchemaBuilder schemaBuilder, string tableName)
        {
            schemaBuilder.DefineField(tableName, fbPropertyX, FlatBufferFieldType);
            schemaBuilder.DefineField(tableName, fbPropertyY, FlatBufferFieldType);
            schemaBuilder.DefineField(tableName, fbPropertyZ, FlatBufferFieldType);
        }

        private string FromStringCode(string variableName) =>
            $"{nameof(CSVValueConverter)}.{VectorStructName}.FromString({variableName})";
    }
}
