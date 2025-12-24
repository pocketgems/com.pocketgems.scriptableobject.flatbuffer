using System;
using System.Globalization;
using System.Linq;
using System.Text;
using PocketGems.Parameters.Interface;
using UnityEngine.TestTools;
#if UNITY_EDITOR
using System.IO;
using UnityEngine;
using UnityEditor;
#endif

namespace PocketGems.Parameters.LocalCSV
{
    public static partial class CSVValueConverter
    {
        private const char ValueDelimiter = ',';
        private const char ListDelimiter = '|';

        public static class Identifier
        {
            public static string FromString(string value)
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new Exception("Identifier must have a non empty value");
                return value.Trim();
            }
        }

        public static class Color
        {
            public static string ToString(UnityEngine.Color value)
            {
                return $"{(int)(value.r * 255)}{ValueDelimiter}{(int)(value.g * 255)}{ValueDelimiter}{(int)(value.b * 255)}{ValueDelimiter}{(int)(value.a * 255)}";
            }

            public static UnityEngine.Color FromString(string value)
            {
                if (string.IsNullOrWhiteSpace(value))
                    return new UnityEngine.Color(0, 0, 0, 0);
                var splitValues = value.Split(ValueDelimiter);
                return new UnityEngine.Color(
                    int.Parse(splitValues[0]) / 255f,
                    int.Parse(splitValues[1]) / 255f,
                    int.Parse(splitValues[2]) / 255f,
                    int.Parse(splitValues[3]) / 255f);
            }
        }

        public static class Vector2
        {
            public static string ToString(UnityEngine.Vector2 value) => $"{value.x}{ValueDelimiter}{value.y}";

            public static UnityEngine.Vector2 FromString(string value)
            {
                if (string.IsNullOrWhiteSpace(value))
                    return new UnityEngine.Vector2();
                var splitValues = value.Split(ValueDelimiter);
                return new UnityEngine.Vector2(
                    float.Parse(splitValues[0]),
                    float.Parse(splitValues[1]));
            }
        }

        public static class Vector2Int
        {
            public static string ToString(UnityEngine.Vector2Int value) => $"{value.x}{ValueDelimiter}{value.y}";

            public static UnityEngine.Vector2Int FromString(string value)
            {
                if (string.IsNullOrWhiteSpace(value))
                    return new UnityEngine.Vector2Int();
                var splitValues = value.Split(ValueDelimiter);
                return new UnityEngine.Vector2Int(
                    int.Parse(splitValues[0]),
                    int.Parse(splitValues[1]));
            }
        }

        public static class Vector3
        {
            public static string ToString(UnityEngine.Vector3 value) =>
                $"{value.x}{ValueDelimiter}{value.y}{ValueDelimiter}{value.z}";

            public static UnityEngine.Vector3 FromString(string value)
            {
                if (string.IsNullOrWhiteSpace(value))
                    return new UnityEngine.Vector3();
                var splitValues = value.Split(ValueDelimiter);
                return new UnityEngine.Vector3(
                    float.Parse(splitValues[0]),
                    float.Parse(splitValues[1]),
                    float.Parse(splitValues[2]));
            }
        }

        public static class Vector3Int
        {
            public static string ToString(UnityEngine.Vector3Int value) =>
                $"{value.x}{ValueDelimiter}{value.y}{ValueDelimiter}{value.z}";

            public static UnityEngine.Vector3Int FromString(string value)
            {
                if (string.IsNullOrWhiteSpace(value))
                    return new UnityEngine.Vector3Int();
                var splitValues = value.Split(ValueDelimiter);
                return new UnityEngine.Vector3Int(
                    int.Parse(splitValues[0]),
                    int.Parse(splitValues[1]),
                    int.Parse(splitValues[2]));
            }
        }

        public static class DateTime
        {
            public static string ToString(System.DateTime value) => value.ToString("yyyy.MM.dd.HH.mm.ss");

            public static System.DateTime FromString(string value)
            {
                if (string.IsNullOrWhiteSpace(value))
                    return new System.DateTime();
                return System.DateTime.ParseExact(value, "yyyy.M.d.H.m.s", CultureInfo.InvariantCulture);
            }
        }

        public static class TimeSpan
        {
            public static string ToString(System.TimeSpan value)
            {
                return string.Format("{0}.{1:D2}.{2:D2}.{3:D2}.{4:D3}",
                    value.Days,
                    value.Hours,
                    value.Minutes,
                    value.Seconds,
                    value.Milliseconds);
            }

            public static System.TimeSpan FromString(string value)
            {
                if (string.IsNullOrWhiteSpace(value))
                    return new System.TimeSpan();
                string[] parts = value.Split('.');
                if (parts.Length == 5)
                {
                    int days = int.Parse(parts[0]);
                    int hours = int.Parse(parts[1]);
                    int minutes = int.Parse(parts[2]);
                    int seconds = int.Parse(parts[3]);
                    int milliseconds = int.Parse(parts[4]);
                    return new System.TimeSpan(days, hours, minutes, seconds, milliseconds);
                }

                throw new ArgumentException("Timespan not the correct format day.hour.minute.second.ms");
            }
        }

        public static class Bool
        {
            public static string ToString(bool value)
            {
                return value ? "1" : "0";
            }

            public static bool FromString(string value)
            {
                if (string.IsNullOrWhiteSpace(value))
                    return false;
                var trimmedValue = value.Trim();
                if (trimmedValue.Length == 0 ||
                    trimmedValue == "FALSE" ||
                    trimmedValue == "false" ||
                    trimmedValue == "False" ||
                    trimmedValue == "0")
                    return false;
                if (trimmedValue == "TRUE" ||
                    trimmedValue == "true" ||
                    trimmedValue == "True" ||
                    trimmedValue == "1")
                    return true;
                throw new ArgumentException($"Bool value cannot be {value}");
            }
        }

        public static class Numeric<T>
        {
            public static string ToString(T value) => value.ToString();

            public static T FromString(string value)
            {
                if (typeof(T).IsEnum)
                    return (T)System.Enum.Parse(typeof(T), value);
                if (string.IsNullOrWhiteSpace(value))
                    return default;

                object o = null;
                if (typeof(T) == typeof(short))
                    o = short.Parse(value);
                else if (typeof(T) == typeof(int))
                    o = int.Parse(value);
                else if (typeof(T) == typeof(long))
                    o = long.Parse(value);
                else if (typeof(T) == typeof(float))
                    o = float.Parse(value);
                else
                    throw new Exception($"Unsupported type {typeof(T)}");
                return (T)o;
            }
        }

        public static class LocalizedString
        {
            public static string ToString(string value) => value;
            public static string FromString(string value) => value;
        }

        [ExcludeFromCoverage]
        public static class ParameterReference
        {
#if UNITY_EDITOR
            public static bool EnableCache
            {
                get => ScriptableObjectLookupCache.Enabled;
                set => ScriptableObjectLookupCache.Enabled = value;
            }

            public static string ToString<T>(ParameterReference<T> value)
                where T : class, IBaseInfo
            {
                if (value == null)
                    return "";
                var guid = value.AssignedGUID;
                if (!string.IsNullOrWhiteSpace(guid))
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    if (path != null)
                        return Path.GetFileNameWithoutExtension(path);
                }
                return guid;
            }
#endif

            public static ParameterReference<T> FromString<T>(IParameterManager parameterManager, string value)
                where T : class, IBaseInfo
            {
                if (string.IsNullOrWhiteSpace(value))
                    return new ParameterReference<T>(parameterManager);
#if UNITY_EDITOR
                if (ScriptableObjectLookupCache.Enabled)
                {
                    var data = ScriptableObjectLookupCache.LookUp(value);
                    for (int i = 0; i < data?.Count; i++)
                    {
                        var path = data[i].Item2;
                        var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                        if (asset != null && asset.name == value && asset is T)
                        {
                            return new ParameterReference<T>(parameterManager, data[i].Item1);
                        }
                    }
                }
                else
                {
                    var searchText = $"{value} t:{nameof(ParameterScriptableObject)}";
                    var guids = AssetDatabase.FindAssets(searchText);
                    for (int i = 0; i < guids.Length; i++)
                    {
                        var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                        var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                        if (asset != null && asset.name == value && asset is T)
                        {
                            return new ParameterReference<T>(parameterManager, guids[i]);
                        }
                    }
                }

                throw new Exception($"Cannot find asset with name {value} of type {typeof(T)}");
#else
                return new ParameterReference<T>(value, true);
#endif
            }
        }

        public static class BoolArray
        {
            public static string ToString(bool[] value)
            {
                if (value == null)
                    return "";
                StringBuilder s = new StringBuilder();
                for (int i = 0; i < value.Length; i++)
                {
                    if (i > 0)
                        s.Append(ListDelimiter);
                    s.Append(Bool.ToString(value[i]));
                }
                return s.ToString();
            }

            public static bool[] FromString(string value)
            {
                // must return a non null value so we can detect overriding of properties by checking non null
                if (string.IsNullOrWhiteSpace(value))
                    return Array.Empty<bool>();
                var values = value.Split(ListDelimiter);
                var bools = new bool[values.Length];
                for (int i = 0; i < values.Length; i++)
                    bools[i] = Bool.FromString(values[i]);
                return bools;
            }
        }

        public static class NumericArray<T>
        {
            public static string ToString(T[] value) =>
                ArrayFuncMapper<T>.ToString(value, Numeric<T>.ToString);

            public static T[] FromString(string value) =>
                ArrayFuncMapper<T>.FromString(value, Numeric<T>.FromString);
        }

        public static class StringArray
        {
            public static string ToString(string[] value)
            {
                if (value == null)
                    return "";
                return string.Join(ListDelimiter.ToString(), value);
            }

            public static string[] FromString(string value)
            {
                // must return a non null value so we can detect overriding of properties by checking non null
                if (string.IsNullOrEmpty(value))
                    return Array.Empty<string>();
                return value.Split(ListDelimiter);
            }
        }

        [ExcludeFromCoverage]
        public static class ParameterReferenceArray
        {
#if UNITY_EDITOR
            public static string ToString<T>(ParameterReference<T>[] value)
                where T : class, IBaseInfo
            {
                if (value == null)
                    return "";
                var strings = new string[value.Length];
                for (int i = 0; i < value.Length; i++)
                    strings[i] = CSVValueConverter.ParameterReference.ToString(value[i]);
                return string.Join(ListDelimiter.ToString(), strings);
            }
#endif

            public static ParameterReference<T>[] FromString<T>(IParameterManager parameterManager, string value)
                    where T : class, IBaseInfo
            {
                // must return a non null value so we can detect overriding of properties by checking non null
                if (string.IsNullOrWhiteSpace(value))
                    return Array.Empty<ParameterReference<T>>();
                var strings = value.Split(ListDelimiter);
                var assetRefs = new ParameterReference<T>[strings.Length];
                for (int i = 0; i < strings.Length; i++)
                    assetRefs[i] = ParameterReference.FromString<T>(parameterManager, strings[i]);
                return assetRefs;
            }
        }

        public static class ArrayFuncMapper<T>
        {
            public static string ToString(T[] value, Func<T, string> converter)
            {
                if (value == null)
                    return "";
                return string.Join(ListDelimiter, value.Select(converter));
            }

            public static T[] FromString(string value, Func<string, T> converter)
            {
                if (string.IsNullOrWhiteSpace(value))
                    return Array.Empty<T>();
                var splitValues = value.Split(ListDelimiter);
                var values = new T[splitValues.Length];
                for (int i = 0; i < splitValues.Length; i++)
                    values[i] = converter(splitValues[i]);
                return values;
            }
        }
    }
}
