using System.Collections.Generic;
using System.Reflection;
using PocketGems.Parameters.LocalCSV;

namespace PocketGems.Parameters.Common.PropertyTypes.Editor
{
    internal class IdentifierPropertyType : StringPropertyType
    {
        public IdentifierPropertyType(PropertyInfo propertyInfo) : base(propertyInfo)
        {
        }

        private const string CachedFieldName = "_identifier";

        public override IReadOnlyList<string> ScriptableObjectFieldAttributesCode() => null;

        public override string ScriptableObjectFieldDefinitionCode() => null;

        public override string ScriptableObjectPropertyImplementationCode() =>
            $"public {_typeKeyword} {PropertyName} => name;";

        public override string FlatBufferFieldDefinitionCode() =>
            "// cached identifier\n" +
            $"private string {CachedFieldName};";

        public override string FlatBufferPropertyImplementationCode() =>
            $"public {_typeKeyword} {PropertyName} => {CachedFieldName} ??= _fb.{FlatBufferStructPropertyName};";

        public override string FlatBufferEditPropertyCode(string variableName) =>
            $"throw new Exception(\"Cannot edit {PropertyName}.\");";

        public override string FlatBufferRemoveEditCode() => null;

        public override string CSVBridgeReadFromCSVCode(string variableName)
        {
            // must rename the scriptable object to rename identifier
            return $"var newIdentifier = {nameof(CSVValueConverter)}.Identifier.FromString({variableName});\n" +
                   $"if (data.name != newIdentifier)\n" +
                   $"    metadata.Rename(newIdentifier);";
        }

        public override string CSVBridgeUpdateCSVRowCode(string variableName) =>
            $"{variableName} = data.name;";
    }
}
