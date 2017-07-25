using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(TimeAttribute))]
public class TimeDrawer : PropertyDrawer
{
    private const float TEXT_HEIGHT = 16f;
	private const float SEPARATOR_HEIGHT = 2f;
	private const float ENDING_SEPARATOR_HEIGHT = 3f;
	private const float FIELD_WIDTH = 40f;
	private const float SEPARATION_WIDTH = 9f;

	private TimeAttribute _timeDrawer { get { return ((TimeAttribute)attribute); } }

    // Here you must define the height of your property drawer. Called by Unity.
    public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
    {
        float baseHeight = base.GetPropertyHeight(prop, label);
	    
	    var height = TEXT_HEIGHT;

	    return baseHeight + height + SEPARATOR_HEIGHT + ENDING_SEPARATOR_HEIGHT;
    }

    // Here you can define the GUI for your property drawer. Called by Unity.
    public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
    {
	    

	    
	    DrawField(position, prop, label);
    }

    void DrawField(Rect position, SerializedProperty prop, GUIContent label)
	{
		var timerLabel = label;
		timerLabel.tooltip = "(HH:MM:SS:ms)";
		
		var separator = new GUIContent();
		separator.text = ":";
		
		int rawSeconds = Mathf.FloorToInt(prop.floatValue);
		
		int miliSeconds = (int)((prop.floatValue - rawSeconds) * 1000);
		int seconds = rawSeconds;
		int minutes = rawSeconds / 60;
		int hours = rawSeconds / 3600;
		
		minutes -= hours * 60; 
		seconds -= minutes * 60;
		seconds -= hours * 3600;
		
		Rect fieldPosition = EditorGUI.IndentedRect(position);
		
		EditorGUI.PrefixLabel(fieldPosition, timerLabel);
		
		fieldPosition.x = 200f;
		fieldPosition.width = FIELD_WIDTH;
        fieldPosition.height = TEXT_HEIGHT;
	    
	    EditorGUI.BeginChangeCheck();

		hours = EditorGUI.IntField(fieldPosition, hours);
		fieldPosition.x += FIELD_WIDTH;
		EditorGUI.LabelField(fieldPosition, separator);
		fieldPosition.x += SEPARATION_WIDTH;
	    minutes = EditorGUI.IntField(fieldPosition, minutes);
		fieldPosition.x += FIELD_WIDTH;
		EditorGUI.PrefixLabel(fieldPosition, separator);
		fieldPosition.x += SEPARATION_WIDTH;
	    seconds = EditorGUI.IntField(fieldPosition, seconds);
		fieldPosition.x += FIELD_WIDTH;
		EditorGUI.PrefixLabel(fieldPosition, separator);
		fieldPosition.x += SEPARATION_WIDTH;
	    miliSeconds = EditorGUI.IntField(fieldPosition, miliSeconds);
	    
	    if (EditorGUI.EndChangeCheck())
	    {
		    prop.floatValue = hours * 60 * 60 + minutes * 60 + seconds + (float)miliSeconds / 1000;
		}
    }
}