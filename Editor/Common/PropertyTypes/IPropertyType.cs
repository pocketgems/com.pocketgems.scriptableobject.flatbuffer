using System.Collections.Generic;
using System.Reflection;
using PocketGems.Parameters.Common.Util.Editor;

namespace PocketGems.Parameters.Common.PropertyTypes.Editor
{
    public interface IPropertyType
    {
        PropertyInfo PropertyInfo { get; }

        // validation
        bool Validate(string interfaceName, out IReadOnlyList<string> errors);

        // scriptable object interface implementation code
        IReadOnlyList<string> ScriptableObjectFieldAttributesCode();
        string ScriptableObjectFieldDefinitionCode();
        string ScriptableObjectPropertyImplementationCode();
        string ScriptableObjectCollectLocalizationStringsCode(
            string localizationKeysArgumentName,
            string localizedScriptArgumentName);

        // flat buffer class interface implementation code
        string FlatBufferFieldDefinitionCode();
        string FlatBufferPropertyImplementationCode(int propertyIndex);
        string FlatBufferEditPropertyCode(int propertyIndex, int maxPropertyIndex, string variableName);
        string FlatBufferRevertEditPropertyCode(int propertyIndex);

        // FlatBufferBuilder code generation
        string FlatBufferBuilderPrepareCode(string tableName);
        string FlatBufferBuilderCode(string tableName);

        // CSV Bridge code generation
        string CSVBridgeColumnNameText { get; }
        string CSVBridgeColumnTypeText { get; }
        string CSVBridgeReadFromCSVCode(string variableName);
        string CSVBridgeUpdateCSVRowCode(string variableName);

        internal void DefineFlatBufferSchema(SchemaBuilder schemaBuilder, string tableName);
    }
}
