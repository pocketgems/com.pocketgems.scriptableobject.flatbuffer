using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using PocketGems.Parameters.Common.Editor;
using PocketGems.Parameters.Common.Util.Editor;
using PocketGems.Parameters.Interface;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;
using ParameterInfo = PocketGems.Parameters.Common.Models.Editor.ParameterInfo;
using Type = System.Type;

namespace PocketGems.Parameters.Editor.Editor
{
    /// <summary>
    /// The editor drawer for ParameterReferences<> so that parameter Scriptable Objects can be dragged directly onto
    /// an Object Field.
    ///
    /// Behind the scenes, only a guid is serialized and the Scriptable Object itself is not referenced directly
    /// on devices.
    /// </summary>
    [ExcludeFromCoverage]
    [CustomPropertyDrawer(typeof(ParameterReference<>), true)]
    public class ParameterReferenceDrawer : PropertyDrawer
    {
        /*
         * Only allow the fold out to be at the top most depth. (MaxDepth 1)
         * Unity starts getting real buggy visually when nesting too many GUI layers
         * (low ROI to handle all depths & corner cases).
         *
         * Note: It was set to 1 originally but increased to 10 for now.
         */
        private const int MaxDepth = 10;
        private static int s_depthCounter = 0;

        /*
         * This is needed so we do not attempt to work around a Unity issue.
         *
         * When attempting to draw one Property Field of an array with the same property name as the inner array, Unity
         * crashes. From briefly looking at their source code, the GUI reordered list seems to index based on property
         * name and other values which seem to be conflicting when trying to draw one within another.
         *
         * Example:
         *
         * ScriptableObjectA
         *      ScriptableObjectB[] _rewards
         *
         * ScriptableObjectB
         *      int[] _rewards
         *
         * In this example, attempting to render ScriptableObjectA will cause issues.  It'll try to render _rewards
         * with in inner list also called _rewards.  The recursive calls to to EditorGUI.PropertyField and
         * EditorGUI.GetPropertyHeight cause issues.
         *
         * This was identified in LTS2020.  In LTS2022, it appears to work without crashing.  If you find yourself
         * crashing, re-enable this workaround by flipping this bool.
         */
        private const bool ShouldWorkAroundRecursivePropertyNameBug = false;
        private static readonly Stack<string> s_arrayPropertyNames = new();
        private static Object s_firstTargetObject;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var paramProperty = new ParamProperty(fieldInfo, property);

            bool objectChanged = DrawGUI(position, property, paramProperty, label, out ParameterScriptableObject newObject);

            if (!objectChanged)
                return;

            if (newObject == null)
            {
                paramProperty.GUID = null;
                return;
            }

            if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(newObject, out string guid, out long localId))
            {
                if (paramProperty.InterfaceType.IsInstanceOfType(newObject))
                {
                    paramProperty.GUID = guid;
                }
                else
                {
                    ParameterDebug.LogError($"Can only assign Parameter Scriptable Objects of type {paramProperty.InterfaceType}");
                }
            }
            else
            {
                ParameterDebug.LogError($"Unable to fetch GUID for {newObject}");
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            s_depthCounter++;
            var paramProperty = new ParamProperty(fieldInfo, property);
            if (paramProperty.ElementPosition.HasValue)
                s_arrayPropertyNames.Push(property.propertyPath.Split('.')[0]);

            // first single line for the label and object field
            float height = EditorGUIUtility.singleLineHeight;

            // allocate space for a error Help Box if needed
            if (paramProperty.Error != null)
            {
                // add spacing between first line & help box
                height += EditorGUIUtility.standardVerticalSpacing;
                // help box height
                height += HelpBoxHeight(paramProperty);
            }

            height += InnerInspectorHeight(paramProperty, property);

            if (paramProperty.ElementPosition.HasValue)
                s_arrayPropertyNames.Pop();
            s_depthCounter--;
            return height;
        }

        private float HelpBoxHeight(ParamProperty property)
        {
            /*
             * This is just an approximation.

             * There doesn't seem to be a great way to determine this from the GetPropertyHeight function and
             * there isn't enough information about where this view will be nested.
             */

            // start with the current full view width
            var approximateWidth = EditorGUIUtility.currentViewWidth;
            // utilize the depth and
            approximateWidth -= property.Property.depth * 15f * 2;
            // additional buffer
            approximateWidth -= 60;

            return EditorStyles.helpBox.CalcHeight(new GUIContent(property.Error), approximateWidth);
        }

        private float InnerInspectorHeight(ParamProperty paramProperty, SerializedProperty property)
        {
            if (s_depthCounter > MaxDepth)
                return 0;
            if (!ParameterPrefs.ExpandableDrawer || paramProperty.ScriptableObject == null || !property.isExpanded)
                return 0;

            float height = 0;
            var serializedObject = paramProperty.SerializedScriptableObject;
            var prop = serializedObject.GetIterator();
            bool children = true;
            while (prop.NextVisible(children))
            {
                children = false;
                // don't draw class file
                if (prop.name == "m_Script") continue;
                if (ShouldWorkAroundRecursivePropertyNameBug &&
                    prop.isArray && s_arrayPropertyNames.Contains(prop.propertyPath))
                    height += EditorGUIUtility.singleLineHeight;
                else
                    height += EditorGUI.GetPropertyHeight(prop, new GUIContent(prop.displayName), true);
                height += EditorGUIUtility.standardVerticalSpacing;
            }
            return height;
        }

        private bool DrawGUI(Rect position, SerializedProperty property, ParamProperty paramProperty, GUIContent label, out ParameterScriptableObject
            newObject)
        {
            /*
             * The s_firstTargetObject is used to keep track of the upper most target that has been traversed.
             *
             * Ideally, all of our DrawGUI code is called to completion on every draw so our
             * s_depthCounter and s_arrayPropertyNames are correctly tracking.
             *
             * However this isn't the case if an element in the array is changed.  It causes the code to stop executing
             * (possibly with an internal exception & catch?) and re-render from the top level.  This throws off
             * s_depthCounter and s_arrayPropertyNames.
             *
             * If we detect we're re-rendering the first item again, we reset appropriately.
             */
            if (s_firstTargetObject == null)
            {
                s_firstTargetObject = property.serializedObject.targetObject;
            }
            else if (s_firstTargetObject == property.serializedObject.targetObject)
            {
                s_depthCounter = 0;
                s_arrayPropertyNames.Clear();
            }

            s_depthCounter++;
            if (paramProperty.ElementPosition.HasValue)
                s_arrayPropertyNames.Push(property.propertyPath.Split('.')[0]);

            // update the text if field is an element in a list or array
            if (paramProperty.ElementPosition.HasValue)
                label.text = $"Element {paramProperty.ElementPosition.Value}";

            // draw the label and object field on the first line
            position.height = EditorGUIUtility.singleLineHeight;
            // utilize the resulting position for the object field & new button
            var objectFieldPosition = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            const int newButtonWidth = 40;
            var buttonRect = new Rect(objectFieldPosition.x + objectFieldPosition.width - newButtonWidth,
                objectFieldPosition.y, newButtonWidth, objectFieldPosition.height);
            objectFieldPosition.width -= newButtonWidth;
            EditorGUI.BeginChangeCheck();
            newObject = null;
            // object field
            var scriptableObject = paramProperty.ScriptableObject;
            var fieldObject = EditorGUI.ObjectField(objectFieldPosition, scriptableObject, GetImplementingType(paramProperty.InterfaceType), false);
            if (EditorGUI.EndChangeCheck())
            {
                // this means the user interacted and set the value on the field
                // this helps determine if the user deliberately set an object or nulled the field.
                newObject = fieldObject as ParameterScriptableObject;

                // return early so that the ParamProperty can be updated with the correct scriptable object
                return true;
            }

            if (GUI.Button(buttonRect, "New"))
            {
                var parameterInterface = new ParameterInfo(paramProperty.InterfaceType);

                var rootDirectory = ParameterConstants.ScriptableObject.Dir;
                if (!Directory.Exists(rootDirectory)) Directory.CreateDirectory(rootDirectory);
                var baseFolderName = parameterInterface.BaseName;
                if (baseFolderName.EndsWith("Info"))
                    baseFolderName = baseFolderName.Substring(0, baseFolderName.Length - 4);
                var subDirectory = Path.Combine(rootDirectory, baseFolderName);
                if (!Directory.Exists(subDirectory)) Directory.CreateDirectory(subDirectory);

                // determine new filename
                string filename = null;
                int counter = 0;
                do
                {
                    var suffix = counter > 0 ? $" {counter}" : "";
                    counter++;
                    var tempFilename = $"{parameterInterface.BaseName}{suffix}.asset";
                    var filePath = Path.Combine(subDirectory, tempFilename);
                    if (!File.Exists(filePath))
                        filename = tempFilename;
                } while (filename == null);
                string savePath = EditorUtility.SaveFilePanelInProject("Save New Asset",
                    filename, "asset", "Save New Asset", subDirectory);
                if (!string.IsNullOrEmpty(savePath))
                {
                    var createdObject = ScriptableObject.CreateInstance(parameterInterface.ScriptableObjectType());
                    AssetDatabase.CreateAsset(createdObject, savePath);
                    newObject = createdObject as ParameterScriptableObject;

                    // return early so that the ParamProperty can be updated with the correct scriptable object
                    return true;
                }
            }

            bool canExpandDrawer = ParameterPrefs.ExpandableDrawer && s_depthCounter <= MaxDepth &&
                                   scriptableObject != null;

            // fold out
            // Only allow the fold out to show for the top most parameter reference in the tree, any children below it
            // cannot be expanded.
            // Unity starts getting real buggy in those situations (low ROI to investigate why).
            if (canExpandDrawer)
            {
                var foldoutRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth,
                    EditorGUIUtility.singleLineHeight);
                property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, new GUIContent(), true);
            }
            position.y += position.height + EditorGUIUtility.standardVerticalSpacing;

            // display error if any
            if (paramProperty.Error != null)
            {
                var helpBoxHeight = HelpBoxHeight(paramProperty);
                position.height = helpBoxHeight;
                EditorGUI.HelpBox(position, paramProperty.Error, MessageType.Error);
                position.y += helpBoxHeight;
            }
            else if (canExpandDrawer && property.isExpanded)
            {
                var prop = paramProperty.SerializedScriptableObject.GetIterator();
                EditorGUI.indentLevel++;
                bool children = true;
                while (prop.NextVisible(children))
                {
                    children = false;
                    // don't draw class file
                    if (prop.name == "m_Script") continue;
                    float height;
                    var propLabel = new GUIContent(prop.displayName);
                    if (ShouldWorkAroundRecursivePropertyNameBug &&
                        prop.isArray && s_arrayPropertyNames.Contains(prop.propertyPath))
                    {
                        height = EditorGUIUtility.singleLineHeight;
                        var style = new GUIStyle(GUI.skin.label);
                        style.fontStyle = FontStyle.Bold;
                        style.normal.textColor = Color.red;
                        position.height = height;
                        var content = new GUIContent("CANNOT DISPLAY", "A Unity bug prevents this from being displayed without crashing. " +
                                                                       "(see code for more details)");
                        EditorGUI.LabelField(position, propLabel, content, style);
                    }
                    else
                    {
                        height = EditorGUI.GetPropertyHeight(prop, propLabel, true);
                        position.height = height;
                        DrawExpandedField(paramProperty.SerializedScriptableObject, position, prop, propLabel);
                    }
                    position.y += height + EditorGUIUtility.standardVerticalSpacing;
                }
                EditorGUI.indentLevel--;

                /*
                 * Checking for GUI.changed is more reliable than checking the result of
                 * serializedObject.ApplyModifiedProperties().
                 *
                 * If the serialized property is an array, modifications in it doesn't seem to trigger
                 * a true result from calls to serializedObject.ApplyModifiedProperties
                 */
                if (GUI.changed)
                {
                    paramProperty.SerializedScriptableObject.ApplyModifiedProperties();
                    InspectorAutoSave.DispatchDelayedSave();
                }
            }

            if (paramProperty.ElementPosition.HasValue)
                s_arrayPropertyNames.Pop();
            s_depthCounter--;
            if (s_depthCounter == 0)
            {
                s_firstTargetObject = null;
            }

            return false;
        }

        /// <summary>
        /// Allows subclasses to override various expanded properties for custom fields.
        /// </summary>
        /// <param name="serializedObject">Main object that the field is drawing for.</param>
        /// <param name="position">Position to draw within.</param>
        /// <param name="property">Property to draw GUI for.</param>
        /// <param name="label">Label that can be used.</param>
        protected virtual void DrawExpandedField(SerializedObject serializedObject, Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.PropertyField(position, property, label, true);
        }

        /// <summary>
        /// Helper method to find the implementing class of a param interface derived from IBaseInfo
        /// This allows restricting the editor object field to only the parameter objects of the relevant type
        /// </summary>
        /// <param name="interfaceType">Interface type of the parameter</param>
        private Type GetImplementingType(Type interfaceType)
        {
            Assembly assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == EditorParameterConstants.CodeGeneration.EditorAssemblyName);

            Type[] types = assembly.GetTypes();
            int implementingTypes = 0;
            Type implementingType = null;
            foreach (var type in types)
            {
                if (interfaceType.IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract &&
                    typeof(ParameterScriptableObject).IsAssignableFrom(type))
                {
                    implementingType = type;
                    implementingTypes++;
                }
            }
            if (implementingTypes == 1)
            {
                return implementingType;
            }
            /*
             * The objectField GUI element this gets used in can only take one Type,
             * so if there are more than one Scriptable Object classes implementing the interface
             * we chose to default to showing all ParamterScriptableObjects for this v1 version
             *
             * In the future, we'll build a custom inspector/drawer that handles this case
             * and shows the precise list of scriptable objects i.e. objects of all types that
             * implement the interface
             */
            else if (implementingTypes > 1)
            {
                return typeof(ParameterScriptableObject);
            }
            else
            {
                ParameterDebug.LogError($"Can't find implementations of type {interfaceType}");
                return null;
            }
        }
    }
}
