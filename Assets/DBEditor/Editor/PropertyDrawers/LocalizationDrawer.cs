using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(LocalizationAttribute))]
public class LocalizationDrawer : PropertyDrawer
{
    // These constants describe the height of the help box and the text field.
    const float TextHeight = 16f;
	const float SeparatorHeight = 2f;

    // Provide easy access to the RegexAttribute for reading information from it.
    LocalizationAttribute localizationAttribute { get { return ((LocalizationAttribute)attribute); } }

    // Here you must define the height of your property drawer. Called by Unity.
    public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
    {
        float baseHeight = base.GetPropertyHeight(prop, label);
	    
	    var height = TextHeight;
	    if (localizationAttribute.isMultiline)
		    height = TextHeight * 4;
	    
	    return baseHeight + height + SeparatorHeight;
    }

    // Here you can define the GUI for your property drawer. Called by Unity.
    public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
    {
        // Draw the text field control GUI.
	    DrawTextField(position, prop, label);

        DrawLocalizedField(position, prop);
    }

	string value;
    void DrawTextField(Rect position, SerializedProperty prop, GUIContent label)
    {
        Rect textFieldPosition = position;
        textFieldPosition.height = TextHeight;
        
	    EditorGUI.BeginChangeCheck();
	    GUI.SetNextControlName("user");
	    if (EditorGUI.TextField(textFieldPosition, label, prop.stringValue).KeyPressed<string>("user", KeyCode.Return, out value))
		    Debug.Log("test");
	    
	    if (EditorGUI.EndChangeCheck())
	        prop.stringValue = value;
	    
	    // http://answers.unity3d.com/questions/41841/how-can-i-determine-when-user-presses-enter-in-an.html?_ga=2.61987756.1471278972.1499338231-1392953560.1496546406
	    
	    //if (Event.current.Equals(Event.KeyboardEvent("return")) && GUI.GetNameOfFocusedControl() == "user")
		//    Debug.Log("Login");
	    
	    //Event e = Event.current;
	    //if (e.type == EventType.keyDown && e.keyCode == KeyCode.Return)
		//    Debug.Log("test");
    }

    void DrawLocalizedField(Rect position, SerializedProperty prop)
    {
	    Rect localizedPosition = EditorGUI.IndentedRect(position);  
	    localizedPosition.y = localizedPosition.y + TextHeight;
	    localizedPosition.height = TextHeight;

        if (localizationAttribute.isMultiline)
        {
        	EditorGUI.LabelField(localizedPosition, "Localized:");
	        	
	        localizedPosition.y = localizedPosition.y + TextHeight + SeparatorHeight;
	        localizedPosition.height = TextHeight * 3;
	        EditorGUI.TextArea(localizedPosition, GetTranslation(prop.stringValue));
        }
        else
	    {
	        localizedPosition.y = localizedPosition.y + SeparatorHeight;
		    EditorGUI.TextField(localizedPosition, "Localized:", GetTranslation(prop.stringValue));
        }
    }

    private string GetTranslation(string locKey)
    {
        return Random.Range(1, 1000).ToString() + locKey;
    }
}