using System.Reflection;
using PocketGems.Parameters.LocalCSV;
using PocketGems.Parameters.Util;

namespace PocketGems.Parameters.PropertyTypes
{
    internal abstract class BaseTimeListPropertyType : NDimensionListPropertyType
    {
        private readonly string _serializedTypeKeyword;

        protected BaseTimeListPropertyType(PropertyInfo propertyInfo, string typeKeyword, string serializedTypeKeyword) : base(propertyInfo, typeKeyword, FlatBufferFieldType.Long)
        {
            _serializedTypeKeyword = serializedTypeKeyword;
        }

        public override string ScriptableObjectFieldDefinitionCode() =>
            $"public {_serializedTypeKeyword}[] {FieldName};";

        public override string ScriptableObjectPropertyImplementationCode() =>
            $"public IReadOnlyList<{_typeKeyword}> {PropertyName} => new ReadOnlyListContainer<{_typeKeyword}>(\n" +
            $"    () => {FieldName}?.Length ?? 0,\n" +
            $"    i => ({_typeKeyword}){FieldName}[i]);";

        public override string CSVBridgeReadFromCSVCode(string variableName) =>
            $"data.{FieldName} = {nameof(CSVValueConverter)}.ArrayFuncMapper<{_serializedTypeKeyword}>.FromString({variableName}, " +
            $"s => ({_serializedTypeKeyword}){nameof(CSVValueConverter)}.{_typeKeyword}.FromString(s));";

        public override string CSVBridgeUpdateCSVRowCode(string variableName) =>
            $"{variableName} = {nameof(CSVValueConverter)}.ArrayFuncMapper<{_serializedTypeKeyword}>.ToString(data.{FieldName}, " +
            $"v => {nameof(CSVValueConverter)}.{_typeKeyword}.ToString(v));";
    }
}
