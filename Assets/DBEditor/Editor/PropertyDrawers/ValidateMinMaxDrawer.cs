using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ValidateMinMaxAttribute))]
public class ValidateMinMaxDrawer : PropertyDrawer
{
    private ValidateMinMaxAttribute _validateRangeAttribute { get { return (ValidateMinMaxAttribute) attribute; } }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var color = GUI.contentColor;

        if (property.floatValue < _validateRangeAttribute.min || property.floatValue > _validateRangeAttribute.max)
            GUI.contentColor = Color.red;

        EditorGUI.PropertyField(position, property);

        GUI.contentColor = color;
    }
}

