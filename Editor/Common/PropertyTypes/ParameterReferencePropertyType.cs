using System;
using System.Reflection;
using PocketGems.Parameters.Common.Util.Editor;
using PocketGems.Parameters.LocalCSV;

namespace PocketGems.Parameters.Common.PropertyTypes.Editor
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

        public override string FlatBufferPropertyImplementationCode(int propertyIndex)
        {
            var referenceClassName = SanitizedPropertyTypeName();
            return
                $"public {referenceClassName}<{_genericType.Name}> {PropertyName} => TryGetOverride<{SanitizedPropertyTypeName()}<{_genericType.Name}>>({propertyIndex}, out var val) ? val : new {referenceClassName}<{_genericType.Name}>(_parameterManager, _fb.{FlatBufferStructPropertyName});";
        }

        public override string FlatBufferEditPropertyCode(int propertyIndex, int maxPropertyIndex, string variableName) =>
            $"return TrySetOverride<{SanitizedPropertyTypeName()}<{_genericType.Name}>>({propertyIndex}, {FromStringCode(variableName, "FromString")}, {maxPropertyIndex}, out error);";

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
            $"data.{FieldName} = {FromStringCode(variableName, "FromCSVString")};";

        public override string CSVBridgeUpdateCSVRowCode(string variableName) =>
            $"{variableName} = {nameof(CSVValueConverter)}.{TypeString()}.ToString<{_genericType.Name}>(data.{FieldName});";

        void IPropertyType.DefineFlatBufferSchema(SchemaBuilder schemaBuilder, string tableName)
        {
            schemaBuilder.DefineField(tableName, FlatBufferStructPropertyName, FlatBufferFieldType.String);
        }

        private string FromStringCode(string variableName, string methodName) =>
            $"{nameof(CSVValueConverter)}.{TypeString()}.{methodName}<{_genericType.Name}>(parameterManager, {variableName})";
    }
}
