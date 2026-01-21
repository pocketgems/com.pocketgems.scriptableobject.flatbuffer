using System.Reflection;
using PocketGems.Parameters.Common.Util.Editor;
using PocketGems.Parameters.LocalCSV;

namespace PocketGems.Parameters.Common.PropertyTypes.Editor
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
            $"public IReadOnlyList<{_typeKeyword}> {PropertyName}\n" +
            $"{{\n" +
            $"    get\n" +
            $"    {{\n" +
            $"        var collection = {FieldName};\n" +
            $"        return new ReadOnlyListContainer<{_typeKeyword}>(\n" +
            $"            () => collection?.Length ?? 0,\n" +
            $"            i => ({_typeKeyword})collection[i]);\n" +
            $"    }}\n" +
            $"}}";

        public override string CSVBridgeReadFromCSVCode(string variableName) =>
            $"data.{FieldName} = {nameof(CSVValueConverter)}.ArrayFuncMapper<{_serializedTypeKeyword}>.FromString({variableName}, " +
            $"s => ({_serializedTypeKeyword}){nameof(CSVValueConverter)}.{_typeKeyword}.FromString(s));";

        public override string CSVBridgeUpdateCSVRowCode(string variableName) =>
            $"{variableName} = {nameof(CSVValueConverter)}.ArrayFuncMapper<{_serializedTypeKeyword}>.ToString(data.{FieldName}, " +
            $"v => {nameof(CSVValueConverter)}.{_typeKeyword}.ToString(v));";
    }
}
