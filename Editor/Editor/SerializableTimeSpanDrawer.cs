using System;
using PocketGems.Parameters.DataTypes;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace PocketGems.Parameters.Editor
{
    [ExcludeFromCoverage]
    [CustomPropertyDrawer(typeof(SerializableTimeSpan))]
    public class SerializableTimeSpanDrawer : PropertyDrawer
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

            float DrawComponent(string componentLabel, int componentDigits, int componentValue)
            {
                var componentFieldWidth = digitWidth * componentDigits;
                rect.width = componentFieldWidth;
                float floatComponentValue = EditorGUI.FloatField(rect, componentValue, floatFieldStyle);
                rect.x += componentFieldWidth + componentLabelPadding;

                var componentLabelWidth = EditorStyles.label.CalcSize(new GUIContent(componentLabel)).x;
                rect.width = componentLabelWidth;
                EditorGUI.LabelField(rect, componentLabel);
                rect.x += componentLabelWidth + componentLabelPadding;

                return floatComponentValue;
            }

            EditorGUI.BeginChangeCheck();

            var ticksProperty = property.FindPropertyRelative(nameof(SerializableTimeSpan.Ticks));
            var timeSpan = TimeSpan.FromTicks(ticksProperty.longValue);
            var newTimeSpan = TimeSpan.Zero;
            try
            {
                newTimeSpan += TimeSpan.FromDays(DrawComponent("d", 2, timeSpan.Days));
                newTimeSpan += TimeSpan.FromHours(DrawComponent("h", 2, timeSpan.Hours));
                newTimeSpan += TimeSpan.FromMinutes(DrawComponent("m", 2, timeSpan.Minutes));
                newTimeSpan += TimeSpan.FromSeconds(DrawComponent("s", 2, timeSpan.Seconds));
                newTimeSpan += TimeSpan.FromMilliseconds(DrawComponent("ms", 3, timeSpan.Milliseconds));
            }
            catch (Exception e)
            {
                // ignored
            }
            finally
            {
                if (EditorGUI.EndChangeCheck())
                {
                    ticksProperty.longValue = newTimeSpan.Ticks;
                }
            }

            EditorGUI.EndProperty();
        }
    }
}
