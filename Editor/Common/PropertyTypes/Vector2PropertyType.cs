using System.Reflection;
using PocketGems.Parameters.Common.Util.Editor;
using PocketGems.Parameters.LocalCSV;
using UnityEngine;

namespace PocketGems.Parameters.Common.PropertyTypes.Editor
{
    internal class Vector2PropertyType : BasePropertyType, IPropertyType
    {
        public Vector2PropertyType(PropertyInfo propertyInfo) : base(propertyInfo)
        {

        }

        protected virtual string VectorStructName => nameof(Vector2);
        protected virtual FlatBufferFieldType FlatBufferFieldType => FlatBufferFieldType.Float;

        private string fbPropertyX => FlatBufferStructPropertyName + "X";
        private string fbPropertyY => FlatBufferStructPropertyName + "Y";

        public override string ScriptableObjectFieldDefinitionCode() =>
            $"public {VectorStructName} {FieldName};";

        public override string ScriptableObjectPropertyImplementationCode() =>
            $"public {VectorStructName} {PropertyName} => {FieldName};";

        public override string FlatBufferPropertyImplementationCode(int propertyIndex) =>
            $"public {VectorStructName} {PropertyName} => TryGetOverride<{VectorStructName}>({propertyIndex}, out var val) ? val : new {VectorStructName}(_fb.{fbPropertyX}, _fb.{fbPropertyY});";

        public override string FlatBufferEditPropertyCode(int propertyIndex, int maxPropertyIndex, string variableName) =>
            $"return TrySetOverride<{VectorStructName}>({propertyIndex}, {FromStringCode(variableName)}, {maxPropertyIndex}, out error);";

        public override string FlatBufferBuilderPrepareCode(string tableName) => null;

        public override string FlatBufferBuilderCode(string tableName)
        {
            return $"{tableName}.Add{fbPropertyX}(_builder, data.{PropertyName}.x);\n" +
                   $"{tableName}.Add{fbPropertyY}(_builder, data.{PropertyName}.y);";
        }

        public override string CSVBridgeReadFromCSVCode(string variableName) =>
            $"data.{FieldName} = {FromStringCode(variableName)};";

        public override string CSVBridgeUpdateCSVRowCode(string variableName) =>
            $"{variableName} = {nameof(CSVValueConverter)}.{VectorStructName}.ToString(data.{FieldName});";

        void IPropertyType.DefineFlatBufferSchema(SchemaBuilder schemaBuilder, string tableName)
        {
            schemaBuilder.DefineField(tableName, fbPropertyX, FlatBufferFieldType);
            schemaBuilder.DefineField(tableName, fbPropertyY, FlatBufferFieldType);
        }

        private string FromStringCode(string variableName) =>
            $"{nameof(CSVValueConverter)}.{VectorStructName}.FromString({variableName})";
    }
}
