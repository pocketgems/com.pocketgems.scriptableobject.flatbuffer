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

        public override string FlatBufferFieldDefinitionCode() =>
            $"private {VectorStructName}? {OverrideFieldName};";

        public override string FlatBufferPropertyImplementationCode() =>
            $"public {VectorStructName} {PropertyName} => {OverrideFieldName} ?? new {VectorStructName}(_fb.{fbPropertyX}, _fb.{fbPropertyY}, _fb.{fbPropertyZ});";

        public override string FlatBufferEditPropertyCode(string variableName) =>
            $"{OverrideFieldName} = {FromStringCode(variableName)};";

        public override string FlatBufferRemoveEditCode() =>
            $"{OverrideFieldName} = null;";

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
