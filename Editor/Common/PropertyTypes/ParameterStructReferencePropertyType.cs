using System;
using System.Reflection;
using PocketGems.Parameters.Common.Util.Editor;

namespace PocketGems.Parameters.Common.PropertyTypes.Editor
{
    internal class ParameterStructReferencePropertyType : BaseParameterReferencePropertyType, IPropertyType
    {
        public ParameterStructReferencePropertyType(PropertyInfo propertyInfo, Type genericType) : base(propertyInfo, genericType)
        {
        }

        public static bool IsReferenceType(PropertyInfo propertyInfo, out Type genericType)
        {
            var propertyType = propertyInfo.PropertyType;
            if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(ParameterStructReference<>))
            {
                genericType = propertyType.GetGenericArguments()[0];
                return true;
            }

            genericType = null;
            return false;
        }

        public override string ScriptableObjectFieldDefinitionCode()
        {
            var baseName = NamingUtil.BaseNameFromStructInterfaceName(_genericType.Name);
            var structName = NamingUtil.StructNameFromBaseName(baseName, false);
            return $"public {structName} {FieldName};";
        }

        public override string ScriptableObjectPropertyImplementationCode() =>
            // need the double cast to handle situations where PARAMS_DISABLE_INTERFACE_IMPLEMENTATION is enabled
            $"public {SanitizedPropertyTypeName()}<{_genericType.Name}> {PropertyName} => new ParameterStructReferenceEditor<{_genericType}>(({_genericType})(object){FieldName});";

        // flat buffer guids are always deterministic and shouldn't change via ab testing
        public override string FlatBufferFieldDefinitionCode() => null;

        public override string FlatBufferPropertyImplementationCode()
        {
            var referenceClassName = SanitizedPropertyTypeName();
            return
                $"public {referenceClassName}<{_genericType.Name}> {PropertyName} => new ParameterStructReferenceRuntime<{_genericType.Name}>(_parameterManager, _fb.{FlatBufferStructPropertyName});";
        }

        public override string FlatBufferEditPropertyCode(string variableName) =>
            "error = $\"Cannot modify a ParameterStructReference with key-value {propertyName}:{value}. ParameterStructReference always point to a predefined guid based on key path.)\";";

        public override string FlatBufferRemoveEditCode() => null;

        public override string FlatBufferBuilderPrepareCode(string tableName)
        {
            var baseName = NamingUtil.BaseNameFromStructInterfaceName(_genericType.Name);
            return $"keyPathBuilder.PushKey(\"{PropertyName}\");\n" +
                   $"var sharedString{FlatBufferStructPropertyName} = _builder.CreateSharedString(keyPathBuilder.KeyPath());\n" +
                   $"Build{baseName}(keyPathBuilder.KeyPath(), data.{FieldName}, keyPathBuilder);\n" +
                   $"keyPathBuilder.PopKey();";
        }

        public override string FlatBufferBuilderCode(string tableName)
        {
            return $"{tableName}.Add{FlatBufferStructPropertyName}(_builder, sharedString{FlatBufferStructPropertyName});";
        }

        public override string CSVBridgeColumnTypeText => _genericType.Name;

        public override string CSVBridgeReadFromCSVCode(string variableName)
        {
            var baseName = NamingUtil.BaseNameFromStructInterfaceName(_genericType.Name);
            var structName = NamingUtil.StructNameFromBaseName(baseName, false);
            return $"keyPathBuilder.PushKey(\"{PropertyName}\");\n" +
                   $"data.{FieldName} = Read{structName}(data.{FieldName}, keyPathBuilder);\n" +
                   $"keyPathBuilder.PopKey();";
        }

        public override string CSVBridgeUpdateCSVRowCode(string variableName)
        {
            var baseName = NamingUtil.BaseNameFromStructInterfaceName(_genericType.Name);
            var structName = NamingUtil.StructNameFromBaseName(baseName, false);
            return $"keyPathBuilder.PushKey(\"{PropertyName}\");\n" +
                   $"Update{structName}(structCSVFileCache, data.{FieldName}, keyPathBuilder);\n" +
                   $"{variableName} = keyPathBuilder.KeyPath();\n" +
                   $"keyPathBuilder.PopKey();";
        }

        void IPropertyType.DefineFlatBufferSchema(SchemaBuilder schemaBuilder, string tableName)
        {
            schemaBuilder.DefineField(tableName, FlatBufferStructPropertyName, FlatBufferFieldType.String);
        }
    }
}
