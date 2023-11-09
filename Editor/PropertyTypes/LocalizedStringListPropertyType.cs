using System.Reflection;
using PocketGems.Parameters.Types;

namespace PocketGems.Parameters.PropertyTypes
{
    internal class LocalizedStringListPropertyType : StringListPropertyType
    {
        public LocalizedStringListPropertyType(PropertyInfo propertyInfo) : base(propertyInfo)
        {
        }

        public override string ScriptableObjectFieldDefinitionCode() =>
            $"public string[] {FieldName};";

        public override string ScriptableObjectPropertyImplementationCode()
        {
            var typeName = nameof(LocalizedString);
            return $"public IReadOnlyList<{typeName}> {PropertyName} => new ReadOnlyListContainer<{typeName}>(\n" +
                   $"    () => {FieldName}?.Length ?? 0,\n" +
                   $"    i => new {typeName}({FieldName}[i]));";
        }

        public override string FlatBufferPropertyImplementationCode()
        {
            var typeName = nameof(LocalizedString);
            return $"public IReadOnlyList<{typeName}> {PropertyName}\n" +
                   $"{{\n" +
                   $"    get\n" +
                   $"    {{\n" +
                   $"        if ({OverrideFieldName} != null)\n" +
                   $"            return new ReadOnlyListContainer<{typeName}>(\n" +
                   $"                () => {OverrideFieldName}.Length,\n" +
                   $"                i => new {typeName}({OverrideFieldName}[i]));\n" +
                   $"        return new ReadOnlyListContainer<{typeName}>(\n" +
                   $"            () => _fb.{FlatBufferStructPropertyName}Length,\n" +
                   $"            i => new {typeName}(_fb.{FlatBufferStructPropertyName}(i)));\n" +
                   $"    }}\n" +
                   $"}}";
        }

        public override string CSVBridgeColumnTypeText => $"{nameof(LocalizedString)}[]";
    }
}
