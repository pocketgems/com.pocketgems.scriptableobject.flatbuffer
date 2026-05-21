using System;
using System.Collections.Generic;
using System.Reflection;
using PocketGems.Parameters.Common.Editor;
using PocketGems.Parameters.Common.Util.Editor;
using PocketGems.Parameters.Interface;
using PocketGems.Parameters.Interface.Attributes;
using UnityEngine.TestTools;

namespace PocketGems.Parameters.Common.PropertyTypes.Editor
{
    internal abstract class BasePropertyType : IPropertyType
    {
        protected bool HasLocalizationKeyAttribute { get; }
        protected bool HasLocalizedScriptAttribute { get; }
        protected virtual bool CanSupportLocalization => false;

        protected BasePropertyType(PropertyInfo propertyInfo)
        {
            PropertyInfo = propertyInfo;

            foreach (var data in propertyInfo.CustomAttributes)
            {
                Type attrType = data.AttributeType;
                if (attrType == typeof(ParameterLocalizationKeyAttribute))
                    HasLocalizationKeyAttribute = true;
                else if (attrType == typeof(ParameterLocalizableScriptAttribute))
                    HasLocalizedScriptAttribute = true;
            }
        }

        public PropertyInfo PropertyInfo { get; }


        public bool Validate(string interfaceName, out IReadOnlyList<string> errors)
        {
            List<string> internalErrors = null;
            void Error(string error)
            {
                if (internalErrors == null)
                    internalErrors = new();
                internalErrors.Add(error);
            }

            var propertyName = PropertyInfo.Name;
            if (EditorParameterConstants.Interface.PropertyNameRegex.Matches(propertyName).Count != 1)
                Error($"Property [{propertyName}] in interface [{interfaceName}] must follow naming pattern {EditorParameterConstants.Interface.PropertyNameRegexString}.");
            if (EditorParameterConstants.Interface.InvalidReservedPropertyNames.Contains(propertyName.ToLower()))
                Error($"Property name [{propertyName}] is invalid & reserved.  It cannot be used in interface [{interfaceName}].");
            if ((ParameterReferencePropertyType.IsReferenceType(PropertyInfo, out var genericType) && genericType == typeof(IBaseInfo)) ||
                (ParameterReferenceListPropertyType.IsListReferenceType(PropertyInfo, out genericType) && genericType == typeof(IBaseInfo)))
                Error($"Cannot define {propertyName} as {nameof(ParameterReference)} with {nameof(IBaseInfo)} in {interfaceName}.");
            if ((ParameterStructReferencePropertyType.IsReferenceType(PropertyInfo, out genericType) && genericType == typeof(IBaseStruct)) ||
                (ParameterStructReferenceListPropertyType.IsListReferenceType(PropertyInfo, out genericType) && genericType == typeof(IBaseStruct)))
                Error($"Cannot define {propertyName} as {nameof(ParameterStructReference)} with {nameof(IBaseStruct)} in {interfaceName}.");
            if (HasLocalizationKeyAttribute && HasLocalizedScriptAttribute)
                Error($"Cannot add both [{nameof(ParameterLocalizationKeyAttribute)}] and [{nameof(ParameterLocalizableScriptAttribute)}] on {propertyName} in {interfaceName}.");
            if (HasLocalizationKeyAttribute && !CanSupportLocalization)
                Error($"The property {propertyName} of type {PropertyTypeName} in {interfaceName} cannot support localization attribute [{nameof(ParameterLocalizationKeyAttribute)}].");
            if (HasLocalizedScriptAttribute && !CanSupportLocalization)
                Error($"The property {propertyName} of type {PropertyTypeName} in {interfaceName} cannot support localization attribute [{nameof(ParameterLocalizableScriptAttribute)}].");

            errors = internalErrors;
            return errors == null;
        }

        public virtual IReadOnlyList<string> ScriptableObjectFieldAttributesCode()
        {
            List<string> attributes = null;
            foreach (var customAttribute in PropertyInfo.GetCustomAttributes())
            {
                if (customAttribute is IAttachScriptableObjectAttribute scriptableObjectAttribute)
                {
                    if (attributes == null)
                        attributes = new List<string>();
                    attributes.Add(scriptableObjectAttribute.ScriptableObjectFieldAttributesCode);
                }
            }
            return attributes;
        }
        public abstract string ScriptableObjectFieldDefinitionCode();
        public abstract string ScriptableObjectPropertyImplementationCode();
        public virtual string ScriptableObjectCollectLocalizationStringsCode(
            string localizationKeysArgumentName,
            string localizedScriptArgumentName) => null;

        public virtual string FlatBufferFieldDefinitionCode() => null;
        public abstract string FlatBufferPropertyImplementationCode(int propertyIndex);
        public abstract string FlatBufferEditPropertyCode(int propertyIndex, int maxPropertyIndex, string variableName);
        public virtual string FlatBufferRevertEditPropertyCode(int propertyIndex) =>
            $"return TryRemoveOverride({propertyIndex}, out error);";

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
        protected string FieldName => $"_{PropertyInfo.Name.LowercaseFirstChar()}";
        protected string FlatBufferStructPropertyName => PropertyInfo.Name.UppercaseFirstChar();
    }
}
