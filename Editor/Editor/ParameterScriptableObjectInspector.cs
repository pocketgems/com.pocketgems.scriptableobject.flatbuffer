using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using PocketGems.Parameters.Common.Editor;
using PocketGems.Parameters.Common.Util.Editor;
using PocketGems.Parameters.Interface;
using PocketGems.Parameters.Validation;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace PocketGems.Parameters.Editor.Editor
{
    [ExcludeFromCoverage]
    [CustomEditor(typeof(ParameterScriptableObject), true), CanEditMultipleObjects]
    public class ParameterScriptableObjectInspector : UnityEditor.Editor
    {
        private Dictionary<string, List<ValidationError>> _propertyToError = new Dictionary<string, List<ValidationError>>();

        protected void DrawParameterToolGUI()
        {
            var skin = GUI.skin.box;
            EditorGUILayout.BeginVertical(skin);

            EditorGUILayout.LabelField("Pocket Gems Parameter", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();

            var className = target.GetType().Name;
            var baseName = NamingUtil.BaseNameFromScriptableObjectClassName(className);
            var csvFileName = NamingUtil.CSVFileNameFromBaseName(baseName, true);

            var buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.fixedWidth = 180;
            EditorGUILayout.LabelField($"Sync'd with:", csvFileName);
            if (GUILayout.Button("Open CSV", buttonStyle))
            {
                var assetFolderName = "Assets";
                var projectPath = Application.dataPath;
                if (projectPath.EndsWith(assetFolderName))
                    projectPath = projectPath.Substring(0, projectPath.Length - assetFolderName.Length);

                var relativeCSVPath = Path.Combine(EditorParameterConstants.CSV.Dir, csvFileName);
                // windows support: Application.dataPath returns forward slashes
                relativeCSVPath = relativeCSVPath.Replace('\\', '/');

                var absoluteCSVPath = Path.Combine(projectPath, relativeCSVPath);
                if (File.Exists(absoluteCSVPath))
                    Application.OpenURL($"file://{absoluteCSVPath}");
                else
                    ParameterDebug.LogError($"File doesn't exist: {absoluteCSVPath}");
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            bool isDirty = EditorUtility.IsPersistent(target) && EditorUtility.IsDirty(target);

            EditorGUILayout.LabelField("Unsaved Changes:", isDirty ? "True" : "False");

            if (GUILayout.Button("Save & Hot Load if Playing", buttonStyle))
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();
            EditorGUILayout.Space();
        }

        /// <summary>
        /// Allow for subclasses to draw custom visualizers for the property
        /// </summary>
        /// <param name="property">property to render</param>
        /// <param name="errorMessage">error message to show if any</param>
        protected virtual void DrawProperty(SerializedProperty property, string errorMessage = null)
        {
            var propLabel = new GUIContent(property.displayName);
            EditorGUILayout.PropertyField(property, propLabel, true);
            if (errorMessage != null)
                EditorGUILayout.HelpBox(errorMessage, MessageType.Error);
        }

        public override void OnInspectorGUI()
        {
            if (!EditorUtility.IsPersistent(target))
            {
                DrawDefaultInspector();
                return;
            }

            DrawParameterToolGUI();

            ValidationError[] validationErrors = null;
            try
            {
                validationErrors = ((ParameterScriptableObject)target).ValidationErrors();
            }
            catch (Exception e)
            {
                // catch and report errors so that users aren't completely stuck on modifying the object
                Debug.LogError(e);
            }

            // construct error mappings property name -> errors
            _propertyToError.Clear();
            for (int i = 0; i < validationErrors?.Length; i++)
            {
                var error = validationErrors?[i];
                var propertyName = error.InfoProperty;
                if (string.IsNullOrEmpty(error.InfoProperty))
                    propertyName = "_unknown";
                if (!_propertyToError.TryGetValue(propertyName, out List<ValidationError> errors))
                {
                    errors = new List<ValidationError>();
                    _propertyToError[propertyName] = errors;
                }
                errors.Add(error);
            }

            // collect properties & errors
            List<(SerializedProperty, string)> properties = new List<(SerializedProperty, string)>();
            var serializedProp = serializedObject.GetIterator();
            bool children = true;
            while (serializedProp.NextVisible(children))
            {
                children = false;
                // don't draw class file
                if (serializedProp.name == "m_Script") continue;
                var interfacePropertyGetterName = serializedProp.displayName.Replace(" ", "");
                string errorMessage = null;
                if (_propertyToError.ContainsKey(interfacePropertyGetterName))
                {
                    StringBuilder s = new StringBuilder();
                    s.Append($"{interfacePropertyGetterName} Validation Error(s):");
                    var propertyErrors = _propertyToError[interfacePropertyGetterName];
                    _propertyToError.Remove(interfacePropertyGetterName);
                    for (int i = 0; i < propertyErrors.Count; i++)
                    {
                        var error = propertyErrors[i];
                        s.AppendLine();
                        s.Append("  ");
                        if (!string.IsNullOrEmpty(error.StructKeyPath))
                        {
                            // remove the first info property and display the rest if the path goes deeper
                            bool hasPath = false;
                            if (error.StructKeyPath.Length > error.InfoProperty.Length)
                            {
                                s.Append(error.StructKeyPath.Substring(error.InfoProperty.Length + 1));
                                hasPath = true;
                            }
                            if (!string.IsNullOrEmpty(error.StructProperty))
                            {
                                if (hasPath)
                                    s.Append(".");
                                s.Append(error.StructProperty);
                            }
                            s.Append(": ");
                        }
                        s.Append($"{error.Message}");
                    }

                    errorMessage = s.ToString();
                }
                properties.Add((serializedProp.Copy(), errorMessage));
            }

            // display missing maps (shouldn't ever happen but just in case...)
            if (_propertyToError.Count != 0)
            {
                StringBuilder s = new StringBuilder("Unexpected Errors:");
                foreach (var kvp in _propertyToError)
                {
                    var errors = kvp.Value;
                    for (int i = 0; i < errors.Count; i++)
                    {
                        s.AppendLine();
                        if (!string.IsNullOrEmpty(errors[i].InfoProperty))
                            s.Append($"{errors[i].InfoProperty}: ");
                        s.Append(errors[i].Message);
                    }
                }
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox(s.ToString(), MessageType.Error);
                EditorGUILayout.Space();
            }

            // draw properties & errors
            for (int i = 0; i < properties.Count; i++)
            {
                serializedProp = properties[i].Item1;
                var errorMessage = properties[i].Item2;

                // don't draw class file
                if (serializedProp.name == "m_Script") continue;
                DrawProperty(serializedProp, errorMessage);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
