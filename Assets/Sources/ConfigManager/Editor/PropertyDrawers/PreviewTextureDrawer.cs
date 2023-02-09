using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(PreviewTextureAttribute))]
public class PreviewTextureDrawer : PropertyDrawer
{
    private PreviewTextureAttribute _previewTextureAttribute { get { return (PreviewTextureAttribute)attribute; } }
    
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.serializedObject.isEditingMultipleObjects)
        {
            EditorGUI.PropertyField(position, property, label);
            return;
        }

        var objType = _previewTextureAttribute.fieldType == FieldType.Sprite ? typeof(Sprite) : typeof(Texture);
        property.objectReferenceValue = EditorGUI.ObjectField(position, label, property.objectReferenceValue, objType, false);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (property.serializedObject.isEditingMultipleObjects)
            return base.GetPropertyHeight(property, label);

        return base.GetPropertyHeight(property, label) + _previewTextureAttribute.size;
    }
}