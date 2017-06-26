using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(MyPropertyType))]
public class MyPropertyDrawer : PropertyDrawer
{
    private float _height = 100;
    private float _indent = 30;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return _height;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive),
                              new GUIContent(label.text));

        var valueProperty = property.FindPropertyRelative("Value");

        float labelHeight = base.GetPropertyHeight(property, label);

        var region = new Rect(position.x + _indent, position.y + labelHeight, position.width - _indent - 10, position.height - labelHeight - 10);

        EditorGUI.BeginChangeCheck();
        var newValue = EditorGUI.TextArea(region, valueProperty.stringValue);
        if (EditorGUI.EndChangeCheck())
        {
            valueProperty.stringValue = newValue;
        }

        EditorGUI.EndProperty();
    }
}