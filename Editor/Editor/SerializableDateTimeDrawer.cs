using System;
using PocketGems.Parameters.DataTypes;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace PocketGems.Parameters.Editor
{
    [ExcludeFromCoverage]
    [CustomPropertyDrawer(typeof(SerializableDateTime))]
    public class SerializableDateTimeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.LabelField(position, label);

            var rect = new Rect(position);
            var labelWidth = EditorGUIUtility.labelWidth + 20.0f;
            rect.x = labelWidth;

            const float componentLabelPadding = 4.0f;
            var floatFieldStyle = new GUIStyle(GUI.skin.textField);
            float digitWidth = EditorStyles.label.CalcSize(new GUIContent("0")).x * 1.5f;
            floatFieldStyle.alignment = TextAnchor.MiddleRight;

            int DrawComponent(string componentLabel, int componentDigits, int componentValue)
            {
                var componentFieldWidth = digitWidth * componentDigits;
                rect.width = componentFieldWidth;
                int intComponentValue = EditorGUI.IntField(rect, componentValue, floatFieldStyle);
                rect.x += componentFieldWidth + componentLabelPadding;

                var componentLabelWidth = EditorStyles.label.CalcSize(new GUIContent(componentLabel)).x;
                rect.width = componentLabelWidth;
                EditorGUI.LabelField(rect, componentLabel);
                rect.x += componentLabelWidth + componentLabelPadding;

                return intComponentValue;
            }

            EditorGUI.BeginChangeCheck();

            var ticksProperty = property.FindPropertyRelative(nameof(SerializableDateTime.Ticks));
            var dateTime = new DateTime(ticksProperty.longValue);
            try
            {
                var year = DrawComponent("y", 4, dateTime.Year);
                var month = DrawComponent("m", 2, dateTime.Month);
                var day = DrawComponent("d", 2, dateTime.Day);
                var hour = DrawComponent("h", 2, dateTime.Hour);
                var minute = DrawComponent("m", 2, dateTime.Minute);
                var second = DrawComponent("s", 2, dateTime.Second);
                dateTime = new DateTime(year, month, day, hour, minute, second);
            }
            catch (Exception e)
            {
                // ignored
            }
            finally
            {
                if (EditorGUI.EndChangeCheck())
                {
                    ticksProperty.longValue = dateTime.Ticks;
                }
            }

            EditorGUI.EndProperty();
        }
    }
}
