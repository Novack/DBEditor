using UnityEditor;
using UnityEngine;
using System.Reflection;

[CustomPropertyDrawer(typeof(TimeAttribute))]
public class TimeDrawer : PropertyDrawer
{
    private float[] _values = new float[4];

    public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
    {
        float val = prop.propertyType == SerializedPropertyType.Integer ? prop.intValue : prop.floatValue;
        int rawSeconds = Mathf.FloorToInt(val);

        _values[0] = rawSeconds / 3600;                 // Hours
        _values[1] = (rawSeconds % 3600) / 60;          // Minutes
        _values[2] = (rawSeconds % 3600) % 60;          // Seconds
        _values[3] = (int)((val - rawSeconds) * 1000);  // Miliseconds

        // Restoring the tooltip attribute.
        var tooltipAttribute = fieldInfo.GetCustomAttributes(typeof(TooltipAttribute), true);
        label.tooltip = tooltipAttribute != null && tooltipAttribute.Length > 0 ? ((TooltipAttribute)tooltipAttribute[0]).tooltip : "";

        var labels = new GUIContent[] { new GUIContent("H"), new GUIContent("M"), new GUIContent("S"), new GUIContent("m") };
        var pos = new Rect(EditorGUIUtility.labelWidth, position.y, 250f, EditorGUIUtility.singleLineHeight);
        EditorGUI.PrefixLabel(position, label);

        EditorGUI.BeginChangeCheck();

        EditorGUI.MultiFloatField(pos, labels, _values);

        if (EditorGUI.EndChangeCheck())
        {
            var t = _values[0] * 60 * 60 + _values[1] * 60 + _values[2] + (float)_values[3] / 1000;

            if (prop.propertyType == SerializedPropertyType.Integer)
                prop.intValue = (int)t;
            else
                prop.floatValue = t;
        }
    }
}