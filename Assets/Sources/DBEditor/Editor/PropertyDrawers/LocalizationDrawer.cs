﻿//using I2.Loc;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(LocalizationAttribute))]
public class LocalizationDrawer : PropertyDrawer
{
    private const float TEXT_HEIGHT = 16f;
	private const float SEPARATOR_HEIGHT = 2f;
	private const float ENDING_SEPARATOR_HEIGHT = 3f;

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
	    
	    return baseHeight + height + SEPARATOR_HEIGHT + ENDING_SEPARATOR_HEIGHT;
    }

    // Here you can define the GUI for your property drawer. Called by Unity.
    public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
    {
	    _locKeyValue = prop.stringValue;
	    _translatedValue = GetTranslation(_locKeyValue);
	    
	    DrawTextField(position, prop, label);
        DrawLocalizedField(position, prop);
    }

    void DrawTextField(Rect position, SerializedProperty prop, GUIContent label)
    {
        Rect textFieldPosition = position;
        textFieldPosition.height = TEXT_HEIGHT;
	    
	    EditorGUI.BeginChangeCheck();
        GUI.SetNextControlName("user");
	    if (EditorGUI.TextField(textFieldPosition, label, _locKeyValue).KeyPressed("user", KeyCode.Return, out _locKeyValue))
	    {
            _translatedValue = GetTranslation(_locKeyValue);
	    }
	    
	    if (EditorGUI.EndChangeCheck())
		    prop.stringValue = _locKeyValue;
    }

    void DrawLocalizedField(Rect position, SerializedProperty prop)
    {
        Rect localizedPosition = EditorGUI.IndentedRect(position);  
	    localizedPosition.y = localizedPosition.y + TEXT_HEIGHT;
	    localizedPosition.height = TEXT_HEIGHT;
	    
	    EditorGUI.BeginChangeCheck();
        if (_localizationAttribute.isMultiline)
        {
        	EditorGUI.LabelField(localizedPosition, "Localized:");	        	
	        localizedPosition.y = localizedPosition.y + TEXT_HEIGHT + SEPARATOR_HEIGHT;
	        localizedPosition.height = TEXT_HEIGHT * 3;
            _translatedValue = EditorGUI.TextArea(localizedPosition, _translatedValue);
            EditorStyles.textField.wordWrap = true;
        }
        else
	    {
	        localizedPosition.y = localizedPosition.y + SEPARATOR_HEIGHT;
            _translatedValue = EditorGUI.TextField(localizedPosition, "Localized:", _translatedValue);
	    }
	    
	    if (EditorGUI.EndChangeCheck())
		    SetTranslation(prop.stringValue, _translatedValue);
    }

    private string GetTranslation(string locKey)
    {
        if (string.IsNullOrEmpty(locKey))
            return "";

        return "soemthing"; // LocalizationManager.GetTranslation(locKey);
    }

    private void SetTranslation(string locKey, string translation)
	{
        if (string.IsNullOrEmpty(locKey))
            return;

        if (_localizationAttribute.isReadOnly)
            return;

        locKey = locKey.Trim();

        if (!string.IsNullOrEmpty(locKey))
        {
            /*
            var source = LocalizationManager.GetSourceContaining(locKey);
            var enIdx = source.GetLanguageIndexFromCode("en");
            var termData = source.GetTermData(locKey);
            var english = termData != null ? termData.Languages[enIdx] : "";
            var newEnglish = translation;

            if (english != newEnglish)
            {
                if (termData == null)
                {
                    termData = source.AddTerm(locKey);
                }

                termData.Languages[enIdx] = newEnglish;
                EditorUtility.SetDirty(source);
            }
            */
        }
    }
}