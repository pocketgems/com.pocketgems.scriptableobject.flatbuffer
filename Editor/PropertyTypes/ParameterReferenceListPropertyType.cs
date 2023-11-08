using System;
using System.Collections.Generic;
using System.Reflection;
using PocketGems.Parameters.LocalCSV;

namespace PocketGems.Parameters.PropertyTypes
{
    internal class ParameterReferenceListPropertyType : StringListPropertyType
    {
        private readonly Type _genericType;

        public static bool IsListReferenceType(PropertyInfo propertyInfo, out Type genericType)
        {
            var propertyType = propertyInfo.PropertyType;
            if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(IReadOnlyList<>))
            {
                var innerPropertyType = propertyType.GetGenericArguments()[0];
                if (innerPropertyType.IsGenericType &&
                    innerPropertyType.GetGenericTypeDefinition() == typeof(ParameterReference<>))
                {
                    genericType = innerPropertyType.GetGenericArguments()[0];
                    return true;
                }
            }

            genericType = null;
            return false;
        }

        public ParameterReferenceListPropertyType(PropertyInfo propertyInfo, Type genericType) : base(propertyInfo)
        {
            _genericType = genericType;
        }

        protected override string StringGetterSnippet => $"data.{FieldName}[j].AssignedGUID";

        public override string ScriptableObjectFieldDefinitionCode() =>
            $"public ParameterReference<{_genericType.Name}>[] {FieldName};";

        public override string ScriptableObjectPropertyImplementationCode() =>
            $"public IReadOnlyList<ParameterReference<{_genericType.Name}>> {PropertyName} => {FieldName};";

        public override string FlatBufferPropertyImplementationCode()
        {
            return $"public IReadOnlyList<ParameterReference<{_genericType.Name}>> {PropertyName}\n" +
                   $"{{\n" +
                   $"    get\n" +
                   $"    {{\n" +
                   $"        if ({OverrideFieldName} != null)\n" +
                   $"            return new ReadOnlyListContainer<ParameterReference<{_genericType.Name}>>(\n" +
                   $"                () => {OverrideFieldName}.Length,\n" +
                   $"                i => new ParameterReference<{_genericType.Name}>({OverrideFieldName}[i], true));\n" +
                   $"        return new ReadOnlyListContainer<ParameterReference<{_genericType.Name}>>(\n" +
                   $"            () => _fb.{FlatBufferStructPropertyName}Length,\n" +
                   $"                i => new ParameterReference<{_genericType.Name}>(_fb.{FlatBufferStructPropertyName}(i)));\n" +
                   $"    }}\n" +
                   $"}}";
        }

        public override string CSVBridgeColumnTypeText => $"{_genericType.Name}[]";

        public override string CSVBridgeReadFromCSVCode(string variableName) =>
            $"data.{FieldName} = {nameof(CSVValueConverter)}.ParameterReferenceArray.FromString<{_genericType.Name}>({variableName});";

        public override string CSVBridgeUpdateCSVRowCode(string variableName) =>
            $"{variableName} = {nameof(CSVValueConverter)}.ParameterReferenceArray.ToString<{_genericType.Name}>(data.{FieldName});";
    }
}
