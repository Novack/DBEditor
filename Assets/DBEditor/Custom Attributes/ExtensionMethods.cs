using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ExtensionMethods 
{

    //public static bool KeyPressed<T>(this T s, string controlName, KeyCode key, out T fieldValue)
    public static bool KeyPressed(this string currentString, string controlName, KeyCode key, out string fieldValue)
    {
        fieldValue = currentString;
        if (GUI.GetNameOfFocusedControl() == controlName)
        {
            if ((Event.current.type == EventType.KeyUp) && (Event.current.keyCode == key))
            {
                return true;
            }
        }

        return false;
	}
	
}
