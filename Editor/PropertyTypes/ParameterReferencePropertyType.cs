using System;
using System.Reflection;
using PocketGems.Parameters.LocalCSV;
using PocketGems.Parameters.Util;

namespace PocketGems.Parameters.PropertyTypes
{
    internal class ParameterReferencePropertyType : BaseParameterReferencePropertyType, IPropertyType
    {
        public ParameterReferencePropertyType(PropertyInfo propertyInfo, Type genericType) : base(propertyInfo,
            genericType)
        {

        }

        public static bool IsReferenceType(PropertyInfo propertyInfo, out Type genericType)
        {
            var propertyType = propertyInfo.PropertyType;
            if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(ParameterReference<>))
            {
                genericType = propertyType.GetGenericArguments()[0];
                return true;
            }

            genericType = null;
            return false;
        }

        private string TypeString() => nameof(ParameterReference);

        public override string ScriptableObjectFieldDefinitionCode() =>
            $"public {SanitizedPropertyTypeName()}<{_genericType.Name}> {FieldName};";

        public override string ScriptableObjectPropertyImplementationCode() =>
            $"public {SanitizedPropertyTypeName()}<{_genericType.Name}> {PropertyName} => {FieldName};";

        public override string FlatBufferFieldDefinitionCode() =>
            $"private {SanitizedPropertyTypeName()}<{_genericType.Name}> {OverrideFieldName};";

        public override string FlatBufferPropertyImplementationCode()
        {
            var referenceClassName = SanitizedPropertyTypeName();
            return
                $"public {referenceClassName}<{_genericType.Name}> {PropertyName} => {OverrideFieldName} ?? new {referenceClassName}<{_genericType.Name}>(_fb.{FlatBufferStructPropertyName});";
        }

        public override string FlatBufferEditPropertyCode(string variableName) =>
            $"{OverrideFieldName} = {FromStringCode(variableName)};";

        public override string FlatBufferRemoveEditCode() =>
            $"{OverrideFieldName} = null;";

        public override string FlatBufferBuilderPrepareCode(string tableName)
        {
            return $"var sharedString{FlatBufferStructPropertyName} = _builder.CreateSharedString(data.{PropertyName}?.AssignedGUID ?? \"\");";
        }

        public override string FlatBufferBuilderCode(string tableName)
        {
            return $"{tableName}.Add{FlatBufferStructPropertyName}(_builder, sharedString{FlatBufferStructPropertyName});";
        }

        public override string CSVBridgeColumnTypeText => _genericType.Name;

        public override string CSVBridgeReadFromCSVCode(string variableName) =>
            $"data.{FieldName} = {FromStringCode(variableName)};";

        public override string CSVBridgeUpdateCSVRowCode(string variableName) =>
            $"{variableName} = {nameof(CSVValueConverter)}.{TypeString()}.ToString<{_genericType.Name}>(data.{FieldName});";

        void IPropertyType.DefineFlatBufferSchema(SchemaBuilder schemaBuilder, string tableName)
        {
            schemaBuilder.DefineField(tableName, FlatBufferStructPropertyName, FlatBufferFieldType.String);
        }

        private string FromStringCode(string variableName) =>
            $"{nameof(CSVValueConverter)}.{TypeString()}.FromString<{_genericType.Name}>({variableName})";
    }
}
