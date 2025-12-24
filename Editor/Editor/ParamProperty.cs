using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace PocketGems.Parameters.Editor.Editor
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

        public SerializedObject SerializedScriptableObject => _serializedScriptableObject;

        /// <summary>
        /// Current position if the property is part of a list or array.  Null otherwise.
        /// </summary>
        public readonly int? ElementPosition;

        /// <summary>
        /// The parameter interface type for the Scriptable Object.
        /// </summary>
        public readonly Type InterfaceType;

        private readonly SerializedProperty _guidProperty;

        /*
         * Unity recommended we cache our SerializedObject to avoid GUI glitches.
         *
         * Unity's response:
         * Regarding the issue of Serializable fields becoming unselectable originates from your custom
         * property drawer. Our investigation revealed that parts of IMGUI, including the ReorderableList,
         * are stateful. This means they retain data about the SerializedObject and SerializedProperties.
         * In your property drawer, you are generating a new SerializedObject each time and disposing of it
         * immediately. This process invalidates the state, causing multiple drawings of the list and
         * disrupting input states.
         */
        private readonly SerializedObject _serializedScriptableObject;
        private static Dictionary<string, SerializedObject> s_serializedScriptableObjectByGuid = new ();

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
                var scriptableObject = ScriptableObject;
                if (scriptableObject == null)
                {
                    Error = $"Missing asset {GUID}";
                }
                else if (!InterfaceType.IsInstanceOfType(scriptableObject))
                {
                    Error = $"Asset no longer matches generic type {InterfaceType}";
                }
                else
                {
                    if (!s_serializedScriptableObjectByGuid.TryGetValue(GUID, out var serializedScriptableObject))
                    {
                        serializedScriptableObject = new SerializedObject(scriptableObject);
                        s_serializedScriptableObjectByGuid[GUID] = serializedScriptableObject;
                    }
                    else
                    {
                        // call to ensure the values are up to date with the target in case it was modified in another inspector/drawer
                        serializedScriptableObject.Update();
                    }
                    _serializedScriptableObject = serializedScriptableObject;
                }
            }
        }
    }
}
