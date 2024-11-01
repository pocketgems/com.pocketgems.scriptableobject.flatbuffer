using System;
using System.Collections.Generic;
using System.Reflection;
using PocketGems.Parameters.Common.Util.Editor;

namespace PocketGems.Parameters.Common.PropertyTypes.Editor
{
    internal class EnumListPropertyType : StandardListPropertyType
    {
        public EnumListPropertyType(PropertyInfo propertyInfo, Type genericType) :
            base(propertyInfo, genericType.Name, FlatBufferFieldType.Long)
        {
        }

        public static bool IsListEnumType(PropertyInfo propertyInfo, out Type genericType)
        {
            var propertyType = propertyInfo.PropertyType;
            if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(IReadOnlyList<>))
            {
                var innerPropertyType = propertyType.GetGenericArguments()[0];
                if (innerPropertyType.IsEnum)
                {
                    genericType = innerPropertyType;
                    return true;
                }
            }

            genericType = null;
            return false;
        }

        public override string FlatBufferPropertyImplementationCode()
        {
            return $"public IReadOnlyList<{_typeKeyword}> {PropertyName}\n" +
                   $"{{\n" +
                   $"    get\n" +
                   $"    {{\n" +
                   $"        if ({OverrideFieldName} != null)\n" +
                   $"            return {OverrideFieldName};\n" +
                   $"        return new ReadOnlyListContainer<{_typeKeyword}>(\n" +
                   $"            () => _fb.{FlatBufferStructPropertyName}Length,\n" +
                   $"            i => ({_typeKeyword})_fb.{FlatBufferStructPropertyName}(i));\n" +
                   $"    }}\n" +
                   $"}}";
        }

        public override string FlatBufferBuilderPrepareCode(string tableName)
        {
            return $"VectorOffset vector{FlatBufferStructPropertyName} = default;\n" +
                   $"if (data.{FieldName}?.Length > 0)\n" +
                   $"{{\n" +
                   $"    var longVector = data.{FieldName}.Select(x => (long)x).ToArray();\n" +
                   $"    vector{FlatBufferStructPropertyName} = {tableName}.Create{FlatBufferStructPropertyName}Vector(_builder, longVector);\n" +
                   $"}}";
        }
    }
}
