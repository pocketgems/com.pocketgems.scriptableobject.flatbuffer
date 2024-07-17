using System.Collections.Generic;
using System.Reflection;
using PocketGems.Parameters.Common.Util.Editor;
using PocketGems.Parameters.Interface;
using UnityEngine.TestTools;

namespace PocketGems.Parameters.Common.PropertyTypes.Editor
{
    internal abstract class BasePropertyType : IPropertyType
    {
        protected BasePropertyType(PropertyInfo propertyInfo)
        {
            PropertyInfo = propertyInfo;
        }

        public PropertyInfo PropertyInfo { get; }

        public virtual IReadOnlyList<string> ScriptableObjectFieldAttributesCode()
        {
            List<string> attributes = null;
            foreach (var customAttribute in PropertyInfo.GetCustomAttributes())
            {
                if (customAttribute is AttachFieldAttributeAttribute attachFieldAttribute)
                {
                    if (attributes == null)
                        attributes = new List<string>();
                    attributes.Add(attachFieldAttribute.AttributeText);
                }
            }
            return attributes;
        }
        public abstract string ScriptableObjectFieldDefinitionCode();
        public abstract string ScriptableObjectPropertyImplementationCode();

        public abstract string FlatBufferFieldDefinitionCode();
        public abstract string FlatBufferPropertyImplementationCode();
        public abstract string FlatBufferEditPropertyCode(string variableName);
        public abstract string FlatBufferRemoveEditCode();

        public abstract string FlatBufferBuilderPrepareCode(string tableName);
        public abstract string FlatBufferBuilderCode(string tableName);

        public virtual string CSVBridgeColumnNameText => PropertyName;
        public virtual string CSVBridgeColumnTypeText => PropertyTypeName;
        public abstract string CSVBridgeReadFromCSVCode(string variableName);
        public abstract string CSVBridgeUpdateCSVRowCode(string variableName);

        [ExcludeFromCoverage]
        void IPropertyType.DefineFlatBufferSchema(SchemaBuilder schemaBuilder, string tableName)
        {
        }

        // helper methods
        protected string PropertyName => PropertyInfo.Name;
        protected string PropertyTypeName => PropertyInfo.PropertyType.Name;
        protected string OverrideFieldName => $"_override{PropertyInfo.Name}";
        protected string FieldName => $"_{PropertyInfo.Name.LowercaseFirstChar()}";
        protected string FlatBufferStructPropertyName => PropertyInfo.Name.UppercaseFirstChar();
    }
}
