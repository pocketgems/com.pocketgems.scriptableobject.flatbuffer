#if ADDRESSABLE_PARAMS
using System;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AddressableAssets;
#endif

namespace PocketGems.Parameters.LocalCSV
{
    public static partial class CSVValueConverter
    {
        public static class AssetReference
        {
            public static string ToString(UnityEngine.AddressableAssets.AssetReference value)
            {
                if (value == null)
                    return "-";
                return $"{value.AssetGUID}-{value.SubObjectName}";
            }

            internal static (string, string) ParseString(string value, bool validateGuid)
            {
                if (string.IsNullOrWhiteSpace(value))
                    return ("", null);
                var index = value.IndexOf('-');
                if (index == -1)
                    throw new Exception($"Cannot find delimiter");
                string guid = value.Substring(0, index);
                string subObjectName = null;
                if (index != value.Length - 1)
                    subObjectName = value.Substring(index + 1);

#if UNITY_EDITOR
                if (validateGuid && !string.IsNullOrWhiteSpace(guid))
                {
                    if (string.IsNullOrWhiteSpace(AssetDatabase.GUIDToAssetPath(guid)))
                    {
                        throw new Exception($"Cannot find asset with guid {guid}.");
                    }

                    var settings = AddressableAssetSettingsDefaultObject.Settings;
                    var entry = settings.FindAssetEntry(guid);
                    if (entry == null)
                    {
                        throw new Exception($"Asset with {guid} is not an addressable asset.");
                    }
                }
#endif

                return (guid, subObjectName);
            }

            public static UnityEngine.AddressableAssets.AssetReference FromString(string value, bool validateGuid = false)
            {
                (string guid, string subObjectName) = ParseString(value, validateGuid);
                var reference = new UnityEngine.AddressableAssets.AssetReference(guid);
                if (subObjectName != null)
                    reference.SubObjectName = subObjectName;
                return reference;
            }
        }

        public static class AssetReferenceGameObject
        {
            public static string ToString(UnityEngine.AddressableAssets.AssetReferenceGameObject value)
            {
                return AssetReference.ToString(value);
            }

            public static UnityEngine.AddressableAssets.AssetReferenceGameObject FromString(string value,
                bool validateGuid = false)
            {
                (string guid, string subObjectName) = AssetReference.ParseString(value, validateGuid);
                var reference = new UnityEngine.AddressableAssets.AssetReferenceGameObject(guid);
                if (subObjectName != null)
                    reference.SubObjectName = subObjectName;
                return reference;
            }
        }

        public static class AssetReferenceSprite
        {
            public static string ToString(UnityEngine.AddressableAssets.AssetReferenceSprite value)
            {
                return AssetReference.ToString(value);
            }

            public static UnityEngine.AddressableAssets.AssetReferenceSprite FromString(string value, bool validateGuid = false)
            {
                (string guid, string subObjectName) = AssetReference.ParseString(value, validateGuid);
                var reference = new UnityEngine.AddressableAssets.AssetReferenceSprite(guid);
                if (subObjectName != null)
                    reference.SubObjectName = subObjectName;
                return reference;
            }
        }

        public static class AssetReferenceAtlasedSprite
        {
            public static string ToString(UnityEngine.AddressableAssets.AssetReferenceAtlasedSprite value)
            {
                return AssetReference.ToString(value);
            }

            public static UnityEngine.AddressableAssets.AssetReferenceAtlasedSprite FromString(string value,
                bool validateGuid = false)
            {
                (string guid, string subObjectName) = AssetReference.ParseString(value, validateGuid);
                var reference = new UnityEngine.AddressableAssets.AssetReferenceAtlasedSprite(guid);
                if (subObjectName != null)
                    reference.SubObjectName = subObjectName;
                return reference;
            }
        }

        public static class AssetReferenceArray
        {
            public static string ToString(UnityEngine.AddressableAssets.AssetReference[] value)
            {
                if (value == null)
                    return "";
                var guids = new string[value.Length];
                for (int i = 0; i < value.Length; i++)
                    guids[i] = AssetReference.ToString(value[i]);
                return string.Join(ListDelimiter.ToString(), guids);
            }

            public static UnityEngine.AddressableAssets.AssetReference[] FromString(string value, bool validateGuid = false)
            {
                // must return a non null value so we can detect overriding of properties by checking non null
                if (string.IsNullOrWhiteSpace(value))
                    return Array.Empty<UnityEngine.AddressableAssets.AssetReference>();
                var guids = value.Split(ListDelimiter);
                var assetRefs = new UnityEngine.AddressableAssets.AssetReference[guids.Length];
                for (int i = 0; i < guids.Length; i++)
                    assetRefs[i] = AssetReference.FromString(guids[i], validateGuid);
                return assetRefs;
            }
        }

        public static class AssetReferenceGameObjectArray
        {
            public static string ToString(UnityEngine.AddressableAssets.AssetReferenceGameObject[] value)
            {
                if (value == null)
                    return "";
                var guids = new string[value.Length];
                for (int i = 0; i < value.Length; i++)
                    guids[i] = AssetReferenceGameObject.ToString(value[i]);
                return string.Join(ListDelimiter.ToString(), guids);
            }

            public static UnityEngine.AddressableAssets.AssetReferenceGameObject[] FromString(string value,
                bool validateGuid = false)
            {
                // must return a non null value so we can detect overriding of properties by checking non null
                if (string.IsNullOrWhiteSpace(value))
                    return Array.Empty<UnityEngine.AddressableAssets.AssetReferenceGameObject>();
                var guids = value.Split(ListDelimiter);
                var assetRefs = new UnityEngine.AddressableAssets.AssetReferenceGameObject[guids.Length];
                for (int i = 0; i < guids.Length; i++)
                    assetRefs[i] = AssetReferenceGameObject.FromString(guids[i], validateGuid);
                return assetRefs;
            }
        }

        public static class AssetReferenceSpriteArray
        {
            public static string ToString(UnityEngine.AddressableAssets.AssetReferenceSprite[] value)
            {
                if (value == null)
                    return "";
                var guids = new string[value.Length];
                for (int i = 0; i < value.Length; i++)
                    guids[i] = AssetReferenceSprite.ToString(value[i]);
                return string.Join(ListDelimiter.ToString(), guids);
            }

            public static UnityEngine.AddressableAssets.AssetReferenceSprite[] FromString(string value, bool validateGuid = false)
            {
                // must return a non null value so we can detect overriding of properties by checking non null
                if (string.IsNullOrWhiteSpace(value))
                    return Array.Empty<UnityEngine.AddressableAssets.AssetReferenceSprite>();
                var guids = value.Split(ListDelimiter);
                var assetRefs = new UnityEngine.AddressableAssets.AssetReferenceSprite[guids.Length];
                for (int i = 0; i < guids.Length; i++)
                    assetRefs[i] = AssetReferenceSprite.FromString(guids[i], validateGuid);
                return assetRefs;
            }
        }

        public static class AssetReferenceAtlasedSpriteArray
        {
            public static string ToString(UnityEngine.AddressableAssets.AssetReferenceAtlasedSprite[] value)
            {
                if (value == null)
                    return "";
                var guids = new string[value.Length];
                for (int i = 0; i < value.Length; i++)
                    guids[i] = AssetReferenceAtlasedSprite.ToString(value[i]);
                return string.Join(ListDelimiter.ToString(), guids);
            }

            public static UnityEngine.AddressableAssets.AssetReferenceAtlasedSprite[] FromString(string value,
                bool validateGuid = false)
            {
                // must return a non null value so we can detect overriding of properties by checking non null
                if (string.IsNullOrWhiteSpace(value))
                    return Array.Empty<UnityEngine.AddressableAssets.AssetReferenceAtlasedSprite>();
                var guids = value.Split(ListDelimiter);
                var assetRefs = new UnityEngine.AddressableAssets.AssetReferenceAtlasedSprite[guids.Length];
                for (int i = 0; i < guids.Length; i++)
                    assetRefs[i] = AssetReferenceAtlasedSprite.FromString(guids[i], validateGuid);
                return assetRefs;
            }
        }
    }
}
#endif
