using System.Collections.Generic;
using System.Reflection;
using PocketGems.Parameters.Util;

namespace PocketGems.Parameters.PropertyTypes
{
    public interface IPropertyType
    {
        PropertyInfo PropertyInfo { get; }

        // scriptable object interface implementation code
        IReadOnlyList<string> ScriptableObjectFieldAttributesCode();
        string ScriptableObjectFieldDefinitionCode();
        string ScriptableObjectPropertyImplementationCode();

        // flat buffer class interface implementation code
        string FlatBufferFieldDefinitionCode();
        string FlatBufferPropertyImplementationCode();
        string FlatBufferEditPropertyCode(string variableName);
        string FlatBufferRemoveEditCode();

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
