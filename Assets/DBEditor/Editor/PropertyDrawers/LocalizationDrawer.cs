using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(LocalizationAttribute))]
public class LocalizationDrawer : PropertyDrawer
{
    // These constants describe the height of the help box and the text field.
    const float NormalHeight = 30f;
    const float MultilineHeight = 50f;
    const float TextHeight = 16f;
    const float SpaceHeight = 16f;

    // Provide easy access to the RegexAttribute for reading information from it.
    LocalizationAttribute localizationAttribute { get { return ((LocalizationAttribute)attribute); } }

    // Here you must define the height of your property drawer. Called by Unity.
    public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
    {
        float baseHeight = base.GetPropertyHeight(prop, label);

        if (localizationAttribute.isMultiline)
            return baseHeight + MultilineHeight;
        else
            return baseHeight + NormalHeight;
    }

    // Here you can define the GUI for your property drawer. Called by Unity.
    public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
    {
        // Draw the text field control GUI.
        DrawTextField(position, prop, label);

        //DrawLocalizedField(position, prop);
    }

    void DrawTextField(Rect position, SerializedProperty prop, GUIContent label)
    {
        Rect textFieldPosition = position;
        textFieldPosition.height = TextHeight;
        
        EditorGUI.BeginChangeCheck();
        //string value = EditorGUILayout.TextField(label, prop.stringValue);
        string value = EditorGUI.TextField(textFieldPosition, prop.stringValue);
        if (EditorGUI.EndChangeCheck())
            prop.stringValue = value;
    }

    void DrawLocalizedField(Rect position, SerializedProperty prop)
    {
        Rect localizedPosition = EditorGUI.IndentedRect(position);        

        if (localizationAttribute.isMultiline)
        {
            //localizedPosition.y += TextHeight;
            EditorGUILayout.TextArea(GetTranslation(prop.stringValue));
            //EditorGUI.TextArea(localizedPosition, GetTranslation(prop.stringValue));            
        }
        else
        {
            //localizedPosition.y += TextHeight * 3;
            //EditorGUI.TextField(localizedPosition, "Localized:", GetTranslation(prop.stringValue));
            EditorGUILayout.TextField(GetTranslation(prop.stringValue));
        }
    }

    private string GetTranslation(string locKey)
    {
        return Random.Range(1, 1000).ToString() + locKey;
    }
}