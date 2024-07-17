using System.Reflection;
using PocketGems.Parameters.Common.Util.Editor;

namespace PocketGems.Parameters.Common.PropertyTypes.Editor
{
    internal class StringPropertyType : StandardPropertyType
    {
        public StringPropertyType(PropertyInfo propertyInfo) : base(propertyInfo, "string", FlatBufferFieldType.String)
        {
        }

        /*
         * Return empty string if the field is null. This gives more predictability for the data returned from
         * parameters getters.
         *
         * It is an improvement over Unity's default behavior where newly added string fields default to null until
         * the object is re-serialized (which will either set the string to empty or a value if set).
         */
        public override string ScriptableObjectPropertyImplementationCode() =>
            $"public {_typeKeyword} {PropertyName} => {FieldName} ?? \"\";";

        public override string FlatBufferFieldDefinitionCode() =>
            $"private {_typeKeyword} {OverrideFieldName};";

        public override string FlatBufferEditPropertyCode(string variableName) =>
            $"{OverrideFieldName} = {variableName};";

        public override string FlatBufferBuilderPrepareCode(string tableName) =>
            $"var sharedString{FlatBufferStructPropertyName} = _builder.CreateSharedString(data.{PropertyName});";

        public override string FlatBufferBuilderCode(string tableName) =>
            $"{tableName}.Add{FlatBufferStructPropertyName}(_builder, sharedString{FlatBufferStructPropertyName});";

        public override string CSVBridgeReadFromCSVCode(string variableName) =>
            $"data.{FieldName} = {variableName};";

        public override string CSVBridgeUpdateCSVRowCode(string variableName) =>
            $"{variableName} = data.{FieldName};";
    }
}
