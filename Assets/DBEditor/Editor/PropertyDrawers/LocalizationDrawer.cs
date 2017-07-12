using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(LocalizationAttribute))]
public class LocalizationDrawer : PropertyDrawer
{
    private const float TEXT_HEIGHT = 16f;
    private const float SEPARATOR_HEIGHT = 2f;

    private LocalizationAttribute _localizationAttribute { get { return ((LocalizationAttribute)attribute); } }

    private string _locKeyValue = "";
    private string _translatedValue = "";

    // Here you must define the height of your property drawer. Called by Unity.
    public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
    {
        float baseHeight = base.GetPropertyHeight(prop, label);
	    
	    var height = TEXT_HEIGHT;
	    if (_localizationAttribute.isMultiline)
		    height = TEXT_HEIGHT * 4;
	    
	    return baseHeight + height + SEPARATOR_HEIGHT;
    }

    // Here you can define the GUI for your property drawer. Called by Unity.
    public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
    {
	    DrawTextField(position, prop, label);
        DrawLocalizedField(position, prop);
    }
        
    void DrawTextField(Rect position, SerializedProperty prop, GUIContent label)
    {
        Rect textFieldPosition = position;
        textFieldPosition.height = TEXT_HEIGHT;

        _locKeyValue = prop.stringValue;

        GUI.SetNextControlName("user");
        if (EditorGUI.TextField(textFieldPosition, label, _locKeyValue).KeyPressed("user", KeyCode.Return, out _locKeyValue))
        {
            prop.stringValue = _locKeyValue;
            _translatedValue = GetTranslation(_locKeyValue);
        }
    }

    void DrawLocalizedField(Rect position, SerializedProperty prop)
    {
        _translatedValue = GetTranslation(_locKeyValue);

        Rect localizedPosition = EditorGUI.IndentedRect(position);  
	    localizedPosition.y = localizedPosition.y + TEXT_HEIGHT;
	    localizedPosition.height = TEXT_HEIGHT;

        if (_localizationAttribute.isMultiline)
        {
        	EditorGUI.LabelField(localizedPosition, "Localized:");	        	
	        localizedPosition.y = localizedPosition.y + TEXT_HEIGHT + SEPARATOR_HEIGHT;
	        localizedPosition.height = TEXT_HEIGHT * 3;
            _translatedValue = EditorGUI.TextArea(localizedPosition, _translatedValue);            
        }
        else
	    {
	        localizedPosition.y = localizedPosition.y + SEPARATOR_HEIGHT;
            _translatedValue = EditorGUI.TextField(localizedPosition, "Localized:", _translatedValue);
        }
    }

    private string GetTranslation(string locKey)
    {
        return Random.Range(1, 1000).ToString() + locKey;
    }
}