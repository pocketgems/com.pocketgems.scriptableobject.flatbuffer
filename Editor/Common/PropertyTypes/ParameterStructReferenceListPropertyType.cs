using System;
using System.Collections.Generic;
using System.Reflection;
using PocketGems.Parameters.Common.Util.Editor;
using PocketGems.Parameters.LocalCSV;

namespace PocketGems.Parameters.Common.PropertyTypes.Editor
{
    internal class ParameterStructReferenceListPropertyType : StringListPropertyType
    {
        private readonly Type _genericType;

        public static bool IsListReferenceType(PropertyInfo propertyInfo, out Type genericType)
        {
            var propertyType = propertyInfo.PropertyType;
            if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(IReadOnlyList<>))
            {
                var innerPropertyType = propertyType.GetGenericArguments()[0];
                if (innerPropertyType.IsGenericType &&
                    innerPropertyType.GetGenericTypeDefinition() == typeof(ParameterStructReference<>))
                {
                    genericType = innerPropertyType.GetGenericArguments()[0];
                    return true;
                }
            }

            genericType = null;
            return false;
        }

        public ParameterStructReferenceListPropertyType(PropertyInfo propertyInfo, Type genericType) : base(propertyInfo)
        {
            _genericType = genericType;
        }

        public override string ScriptableObjectFieldDefinitionCode()
        {
            var baseName = NamingUtil.BaseNameFromStructInterfaceName(_genericType.Name);
            var structName = NamingUtil.StructNameFromBaseName(baseName, false);
            return $"public {structName}[] {FieldName};";
        }

        public override string ScriptableObjectPropertyImplementationCode() =>
            $"public IReadOnlyList<ParameterStructReference<{_genericType.Name}>> {PropertyName}\n" +
            $"{{\n" +
            $"    get\n" +
            $"    {{\n" +
            $"        var structs = {FieldName};\n" +
            $"        return new ReadOnlyListContainer<ParameterStructReference<{_genericType.Name}>>(\n" +
            $"            () => structs?.Length ?? 0,\n" +
            // need the double cast to handle situations where PARAMS_DISABLE_INTERFACE_IMPLEMENTATION is enabled
            $"            i => new ParameterStructReferenceEditor<{_genericType.Name}>(({_genericType.Name})(object)structs[i]));\n" +
            $"    }}\n" +
            $"}}";

        public override string FlatBufferPropertyImplementationCode()
        {
            return $"public IReadOnlyList<ParameterStructReference<{_genericType.Name}>> {PropertyName}\n" +
                   $"{{\n" +
                   $"    get\n" +
                   $"    {{\n" +
                   $"        if ({OverrideFieldName} != null)\n" +
                   $"            return new ReadOnlyListContainer<ParameterStructReference<{_genericType.Name}>>(\n" +
                   $"                () => {OverrideFieldName}.Length,\n" +
                   $"                i => new ParameterStructReferenceRuntime<{_genericType.Name}>({OverrideFieldName}[i]));\n" +
                   $"        return new ReadOnlyListContainer<ParameterStructReference<{_genericType.Name}>>(\n" +
                   $"            () => _fb.{FlatBufferStructPropertyName}Length,\n" +
                   $"                i => new ParameterStructReferenceRuntime<{_genericType.Name}>(_fb.{FlatBufferStructPropertyName}(i)));\n" +
                   $"    }}\n" +
                   $"}}";
        }

        public override string FlatBufferBuilderPrepareCode(string tableName)
        {
            var baseName = NamingUtil.BaseNameFromStructInterfaceName(_genericType.Name);
            return $"VectorOffset vector{FlatBufferStructPropertyName} = default;\n" +
                   $"if (data.{FieldName}?.Length > 0)\n" +
                   $"{{\n" +
                   $"    var stringOffsets = new StringOffset[data.{PropertyName}.Count];\n" +
                   $"    for (int j = 0; j < data.{PropertyName}.Count; j++)\n" +
                   $"    {{\n" +
                   $"        keyPathBuilder.PushKey(\"{PropertyName}\", j);\n" +
                   $"        stringOffsets[j] = _builder.CreateSharedString(keyPathBuilder.KeyPath());\n" +
                   $"        Build{baseName}(keyPathBuilder.KeyPath(), data.{FieldName}[j], keyPathBuilder);\n" +
                   $"        keyPathBuilder.PopKey();\n" +
                   $"    }}\n" +
                   $"    vector{FlatBufferStructPropertyName} = {tableName}.Create{FlatBufferStructPropertyName}Vector(_builder, stringOffsets);\n" +
                   $"}}";
        }

        public override string CSVBridgeColumnTypeText => $"{_genericType.Name}[]";

        public override string CSVBridgeReadFromCSVCode(string variableName)
        {
            var baseName = NamingUtil.BaseNameFromStructInterfaceName(_genericType.Name);
            var structName = NamingUtil.StructNameFromBaseName(baseName, false);
            return
                $"data.{FieldName} = AttemptReadStructsArray<{structName}>(\"{PropertyName}\", data.{FieldName}, keyPathBuilder, AttemptRead{structName});";
        }

        public override string CSVBridgeUpdateCSVRowCode(string variableName)
        {

            var baseName = NamingUtil.BaseNameFromStructInterfaceName(_genericType.Name);
            var structName = NamingUtil.StructNameFromBaseName(baseName, false);
            return $"{{\n" +
                   $"    var arrayKeyPaths = new string[data.{FieldName}?.Length ?? 0];\n" +
                   $"    for (int j = 0; j < data.{FieldName}?.Length; j++)\n" +
                   $"    {{\n" +
                   $"        keyPathBuilder.PushKey(\"{PropertyName}\", j);\n" +
                   $"        arrayKeyPaths[j] = keyPathBuilder.KeyPath();\n" +
                   $"        Update{structName}(structCSVFileCache, data.{FieldName}[j], keyPathBuilder);\n" +
                   $"        keyPathBuilder.PopKey();\n" +
                   $"    }}\n" +
                   $"    {variableName} = {nameof(CSVValueConverter)}.StringArray.ToString(arrayKeyPaths);\n" +
                   $"}}";
        }
    }
}
