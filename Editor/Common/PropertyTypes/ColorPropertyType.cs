using System.Reflection;
using PocketGems.Parameters.Common.Util.Editor;
using PocketGems.Parameters.LocalCSV;

namespace PocketGems.Parameters.Common.PropertyTypes.Editor
{
    internal class ColorPropertyType : BasePropertyType, IPropertyType
    {
        public ColorPropertyType(PropertyInfo propertyInfo) : base(propertyInfo)
        {

        }

        public override string ScriptableObjectFieldDefinitionCode() =>
            $"public Color {FieldName};";

        public override string ScriptableObjectPropertyImplementationCode() =>
            $"public Color {PropertyName} => {FieldName};";

        public override string FlatBufferFieldDefinitionCode() =>
            $"private Color? {OverrideFieldName};";

        public override string FlatBufferPropertyImplementationCode() =>
            $"public Color {PropertyName} => {OverrideFieldName} ?? new Color(_fb.{fbPropertyR}, _fb.{fbPropertyG}, _fb.{fbPropertyB}, _fb.{fbPropertyA});";

        public override string FlatBufferEditPropertyCode(string variableName) =>
            $"{OverrideFieldName} = {FromStringCode(variableName)};";

        public override string FlatBufferRemoveEditCode() =>
            $"{OverrideFieldName} = null;";

        public override string FlatBufferBuilderPrepareCode(string tableName) => null;

        public override string FlatBufferBuilderCode(string tableName)
        {
            return $"{tableName}.Add{fbPropertyR}(_builder, data.{PropertyName}.r);\n" +
                   $"{tableName}.Add{fbPropertyG}(_builder, data.{PropertyName}.g);\n" +
                   $"{tableName}.Add{fbPropertyB}(_builder, data.{PropertyName}.b);\n" +
                   $"{tableName}.Add{fbPropertyA}(_builder, data.{PropertyName}.a);";
        }

        public override string CSVBridgeReadFromCSVCode(string variableName) =>
            $"data.{FieldName} = {FromStringCode(variableName)};";

        public override string CSVBridgeUpdateCSVRowCode(string variableName) =>
            $"{variableName} = {nameof(CSVValueConverter)}.Color.ToString(data.{FieldName});";

        void IPropertyType.DefineFlatBufferSchema(SchemaBuilder schemaBuilder, string tableName)
        {
            schemaBuilder.DefineField(tableName, fbPropertyR, FlatBufferFieldType.Float);
            schemaBuilder.DefineField(tableName, fbPropertyG, FlatBufferFieldType.Float);
            schemaBuilder.DefineField(tableName, fbPropertyB, FlatBufferFieldType.Float);
            schemaBuilder.DefineField(tableName, fbPropertyA, FlatBufferFieldType.Float);
        }

        private string FromStringCode(string variableName) =>
            $"{nameof(CSVValueConverter)}.Color.FromString({variableName})";

        private string fbPropertyR => FlatBufferStructPropertyName + "R";
        private string fbPropertyG => FlatBufferStructPropertyName + "G";
        private string fbPropertyB => FlatBufferStructPropertyName + "B";
        private string fbPropertyA => FlatBufferStructPropertyName + "A";
    }
}
