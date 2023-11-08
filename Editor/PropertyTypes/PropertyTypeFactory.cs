using System;
using System.Collections.Generic;
using System.Reflection;
using PocketGems.Parameters.Interface;
using PocketGems.Parameters.Models;
using PocketGems.Parameters.Types;
using PocketGems.Parameters.Util;
using UnityEngine;
#if ADDRESSABLE_PARAMS
using UnityEngine.AddressableAssets;
#endif

namespace PocketGems.Parameters.PropertyTypes
{
    /// <summary>
    ///  Factory to return create and return property types
    /// </summary>
    internal static class PropertyTypeFactory
    {
        /// <summary>
        /// Create and return an IPropertyType.
        /// </summary>
        /// <param name="parameterInterface">the parameter interface that the property belongs to</param>
        /// <param name="propertyInfo">input property info from an interface</param>
        /// <param name="error">error string, else null if no errors</param>
        /// <returns>IPropertyType if successful, null otherwise</returns>
        public static IPropertyType Create(IParameterInterface parameterInterface, PropertyInfo propertyInfo, out string error)
        {
            error = null;
            var propertyType = propertyInfo.PropertyType;

            // strings
            if (typeof(IBaseInfo).IsAssignableFrom(parameterInterface.Type) &&
                propertyInfo.Name == NamingUtil.IdentifierPropertyName)
                return new IdentifierPropertyType(propertyInfo);
            if (propertyType == typeof(string))
                return new StringPropertyType(propertyInfo);
            if (propertyType == typeof(IReadOnlyList<string>))
                return new StringListPropertyType(propertyInfo);

            // localized strings
            if (propertyType == typeof(LocalizedString))
                return new LocalizedStringPropertyType(propertyInfo);
            if (propertyType == typeof(IReadOnlyList<LocalizedString>))
                return new LocalizedStringListPropertyType(propertyInfo);

            // enum
            if (propertyType.IsEnum)
                return new EnumPropertyType(propertyInfo);
            if (EnumListPropertyType.IsListEnumType(propertyInfo, out Type genericType))
                return new EnumListPropertyType(propertyInfo, genericType);

            // scalars
            if (propertyType == typeof(bool))
                return new StandardPropertyType(propertyInfo, "bool", FlatBufferFieldType.Bool);
            if (propertyType == typeof(short))
                return new StandardPropertyType(propertyInfo, "short", FlatBufferFieldType.Short);
            if (propertyType == typeof(int))
                return new StandardPropertyType(propertyInfo, "int", FlatBufferFieldType.Int);
            if (propertyType == typeof(long))
                return new StandardPropertyType(propertyInfo, "long", FlatBufferFieldType.Long);
            if (propertyType == typeof(float))
                return new StandardPropertyType(propertyInfo, "float", FlatBufferFieldType.Float);
            if (propertyType == typeof(ushort))
                return new StandardPropertyType(propertyInfo, "ushort", FlatBufferFieldType.UShort);
            if (propertyType == typeof(uint))
                return new StandardPropertyType(propertyInfo, "uint", FlatBufferFieldType.UInt);
            if (propertyType == typeof(ulong))
                return new StandardPropertyType(propertyInfo, "ulong", FlatBufferFieldType.ULong);

            // scalar lists
            if (propertyType == typeof(IReadOnlyList<bool>))
                return new StandardListPropertyType(propertyInfo, "bool", FlatBufferFieldType.Bool);
            if (propertyType == typeof(IReadOnlyList<short>))
                return new StandardListPropertyType(propertyInfo, "short", FlatBufferFieldType.Short);
            if (propertyType == typeof(IReadOnlyList<int>))
                return new StandardListPropertyType(propertyInfo, "int", FlatBufferFieldType.Int);
            if (propertyType == typeof(IReadOnlyList<long>))
                return new StandardListPropertyType(propertyInfo, "long", FlatBufferFieldType.Long);
            if (propertyType == typeof(IReadOnlyList<float>))
                return new StandardListPropertyType(propertyInfo, "float", FlatBufferFieldType.Float);
            if (propertyType == typeof(IReadOnlyList<ushort>))
                return new StandardListPropertyType(propertyInfo, "ushort", FlatBufferFieldType.UShort);
            if (propertyType == typeof(IReadOnlyList<uint>))
                return new StandardListPropertyType(propertyInfo, "uint", FlatBufferFieldType.UInt);
            if (propertyType == typeof(IReadOnlyList<ulong>))
                return new StandardListPropertyType(propertyInfo, "ulong", FlatBufferFieldType.ULong);

            // parameter reference types
            if (ParameterReferencePropertyType.IsReferenceType(propertyInfo, out genericType))
                return new ParameterReferencePropertyType(propertyInfo, genericType);
            if (ParameterReferenceListPropertyType.IsListReferenceType(propertyInfo, out genericType))
                return new ParameterReferenceListPropertyType(propertyInfo, genericType);

            // parameter struct reference types
            if (ParameterStructReferencePropertyType.IsReferenceType(propertyInfo, out genericType))
                return new ParameterStructReferencePropertyType(propertyInfo, genericType);
            if (ParameterStructReferenceListPropertyType.IsListReferenceType(propertyInfo, out genericType))
                return new ParameterStructReferenceListPropertyType(propertyInfo, genericType);

            // unity types
            if (propertyType == typeof(Color))
                return new ColorPropertyType(propertyInfo);
            if (propertyType == typeof(IReadOnlyList<Color>))
                return new ColorListPropertyType(propertyInfo);
            if (propertyType == typeof(Vector3Int))
                return new Vector3IntPropertyType(propertyInfo);
            if (propertyType == typeof(IReadOnlyList<Vector3Int>))
                return new Vector3IntListPropertyType(propertyInfo);
            if (propertyType == typeof(Vector3))
                return new Vector3PropertyType(propertyInfo);
            if (propertyType == typeof(IReadOnlyList<Vector3>))
                return new Vector3ListPropertyType(propertyInfo);
            if (propertyType == typeof(Vector2Int))
                return new Vector2IntPropertyType(propertyInfo);
            if (propertyType == typeof(IReadOnlyList<Vector2Int>))
                return new Vector2IntListPropertyType(propertyInfo);
            if (propertyType == typeof(Vector2))
                return new Vector2PropertyType(propertyInfo);
            if (propertyType == typeof(IReadOnlyList<Vector2>))
                return new Vector2ListPropertyType(propertyInfo);

#if ADDRESSABLE_PARAMS
            // addressable references
            if (propertyType == typeof(AssetReference))
                return new AssetReferencePropertyType(propertyInfo);
            if (propertyType == typeof(IReadOnlyList<AssetReference>))
                return new AssetReferenceListPropertyType(propertyInfo);
            if (propertyType == typeof(AssetReferenceSprite))
                return new AssetReferenceSpritePropertyType(propertyInfo);
            if (propertyType == typeof(IReadOnlyList<AssetReferenceSprite>))
                return new AssetReferenceListSpritePropertyType(propertyInfo);
            if (propertyType == typeof(AssetReferenceAtlasedSprite))
                return new AssetReferenceAtlasedSpritePropertyType(propertyInfo);
            if (propertyType == typeof(IReadOnlyList<AssetReferenceAtlasedSprite>))
                return new AtlasedSpriteAssetReferenceListPropertyType(propertyInfo);
#endif

            error = $"property {propertyType} is not supported";
            return null;
        }
    }
}
