using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ExtensionMethods 
{
	
	public static bool KeyPressed<T>(this T s, string controlName,KeyCode key, out T fieldValue)
	{
		fieldValue = s;
		if(GUI.GetNameOfFocusedControl() == controlName)
		{
			if ((Event.current.type == EventType.KeyUp) && (Event.current.keyCode == key))
				return true;
			
			return false;
		}
		else
		{
			return false;
		}
		
	}
	
}
