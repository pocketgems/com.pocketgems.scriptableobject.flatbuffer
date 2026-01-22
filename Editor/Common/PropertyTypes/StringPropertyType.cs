using System.Reflection;
using PocketGems.Parameters.Common.Util.Editor;

namespace PocketGems.Parameters.Common.PropertyTypes.Editor
{
    internal class StringPropertyType : StandardPropertyType
    {
        protected override bool CanSupportLocalization => true;

        public StringPropertyType(PropertyInfo propertyInfo) : base(propertyInfo, "string", FlatBufferFieldType.String) { }

        /*
         * Return empty string if the field is null. This gives more predictability for the data returned from
         * parameters getters.
         *
         * It is an improvement over Unity's default behavior where newly added string fields default to null until
         * the object is re-serialized (which will either set the string to empty or a value if set).
         */
        public override string ScriptableObjectPropertyImplementationCode() =>
            $"public {_typeKeyword} {PropertyName} => {FieldName} ?? \"\";";

        public override string ScriptableObjectCollectLocalizationStringsCode(string localizationKeysArgumentName, string localizedScriptArgumentName)
        {
            if (HasLocalizationKeyAttribute || HasLocalizedScriptAttribute)
            {
                return $"if (!string.IsNullOrWhiteSpace({FieldName}))\n" +
                       $"    {(HasLocalizationKeyAttribute ? localizationKeysArgumentName : localizedScriptArgumentName)}.Add({FieldName});";
            }

            return null;
        }

        public override string FlatBufferPropertyImplementationCode(int propertyIndex)
        {
            if (HasLocalizationKeyAttribute)
                return $"public {_typeKeyword} {PropertyName} => {nameof(ParameterLocalizationHandler)}.{nameof(ParameterLocalizationHandler.GetLocalizationKeyTranslation)}(TryGetOverride<{_typeKeyword}>({propertyIndex}, out var val) ? val : _fb.{FlatBufferStructPropertyName});";

            if (HasLocalizedScriptAttribute)
                return $"public {_typeKeyword} {PropertyName} => {nameof(ParameterLocalizationHandler)}.{nameof(ParameterLocalizationHandler.GetLocalizableScriptTranslation)}(TryGetOverride<{_typeKeyword}>({propertyIndex}, out var val) ? val : _fb.{FlatBufferStructPropertyName});";

            return base.FlatBufferPropertyImplementationCode(propertyIndex);
        }

        public override string FlatBufferEditPropertyCode(int propertyIndex, int maxPropertyIndex, string variableName) =>
            $"return TrySetOverride<{_typeKeyword}>({propertyIndex}, {variableName}, {maxPropertyIndex}, out error);";

        public override string FlatBufferBuilderPrepareCode(string tableName) =>
            $"var sharedString{FlatBufferStructPropertyName} = _builder.CreateSharedString(data.{PropertyName});";

        public override string FlatBufferBuilderCode(string tableName) =>
            $"{tableName}.Add{FlatBufferStructPropertyName}(_builder, sharedString{FlatBufferStructPropertyName});";

        public override string CSVBridgeReadFromCSVCode(string variableName) =>
            $"data.{FieldName} = {variableName};";

        public override string CSVBridgeUpdateCSVRowCode(string variableName) =>
            $"{variableName} = data.{FieldName};";
    }
}
