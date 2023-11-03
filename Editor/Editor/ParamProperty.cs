using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace PocketGems.Parameters.Editor
{
    /// <summary>
    /// Convenience wrapper to fetch and modify data for a SerializedProperty representing a
    /// ParameterReference<>
    /// </summary>
    [ExcludeFromCoverage]
    internal class ParamProperty
    {
        /// <summary>
        /// The original Serialized Property.
        /// </summary>
        public readonly SerializedProperty Property;

        /// <summary>
        /// String with the error for the current serialized values, null otherwise.
        /// </summary>
        public string Error { get; set; }

        /// <summary>
        /// Serialized GUID.  Empty string or null otherwise.
        /// </summary>
        public string GUID
        {
            get => _guidProperty.stringValue;
            set => _guidProperty.stringValue = value;
        }

        /// <summary>
        /// Serialized Scriptable Object.  Null if the object no longer exists or was set to null.
        /// </summary>
        public ScriptableObject ScriptableObject
        {
            get
            {
                if (string.IsNullOrWhiteSpace(GUID))
                    return null;
                var assetPath = AssetDatabase.GUIDToAssetPath(GUID);
                if (assetPath == null)
                    return null;
                return AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);
            }
        }

        /// <summary>
        /// Current position if the property is part of a list or array.  Null otherwise.
        /// </summary>
        public readonly int? ElementPosition;

        /// <summary>
        /// The parameter interface type for the Scriptable Object.
        /// </summary>
        public readonly Type InterfaceType;

        private readonly SerializedProperty _guidProperty;

        public ParamProperty(FieldInfo fieldInfo, SerializedProperty property)
        {
            Property = property;
            _guidProperty = property.FindPropertyRelative("guid");

            // check if the field is a list or array of references or direct reference
            Type fieldType = fieldInfo.FieldType;
            // update the label for list/arrays
            if (Property.propertyPath.EndsWith("]"))
            {
                // get the position
                var split = Property.propertyPath.Split('[', ']');
                int pos = int.Parse(split[split.Length - 2]);
                ElementPosition = pos;
                if (fieldType.IsArray)
                    InterfaceType = fieldType.GetElementType().GetGenericArguments()[0];
                else
                    InterfaceType = fieldType.GetGenericArguments()[0].GetGenericArguments()[0];
            }
            else
            {
                InterfaceType = fieldType.GetGenericArguments()[0];
            }

            if (!string.IsNullOrWhiteSpace(GUID))
            {
                if (ScriptableObject == null)
                {
                    Error = $"Missing asset {GUID}";
                }
                else if (!InterfaceType.IsInstanceOfType(ScriptableObject))
                {
                    Error = $"Asset no longer matches generic type {InterfaceType}";
                }
            }
        }
    }
}
