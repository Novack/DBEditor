﻿#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Text;
using System.Collections.Generic;
using System.Collections;
using UnityEditor;
using System.Reflection;

[CustomEditor(typeof (UnityEngine.Object), true)]
[CanEditMultipleObjects]
public class EditorButton : Editor
{
	class EditorButtonState {
		public bool opened;
		public System.Object[] parameters;
		public EditorButtonState(int numberOfParameters) {
			parameters = new System.Object[numberOfParameters];
		}
	}

	EditorButtonState[] editorButtonStates;

	delegate object ParameterDrawer(ParameterInfo parameter, object val);

	Dictionary<Type,ParameterDrawer> typeDrawer = new Dictionary<Type, ParameterDrawer> {

		{typeof(float),DrawFloatParameter},
		{typeof(int),DrawIntParameter},
		{typeof(string),DrawStringParameter},
		{typeof(bool),DrawBoolParameter},
		{typeof(Color),DrawColorParameter},
		{typeof(Vector3),DrawVector3Parameter},
		{typeof(Vector2),DrawVector2Parameter},
		{typeof(Quaternion),DrawQuaternionParameter},
        {typeof(Enum),DrawEnumParameter}
    };

	Dictionary<Type,string> typeDisplayName = new Dictionary<Type, string> {
	
		{typeof(float),"float"},
		{typeof(int),"int"},
		{typeof(string),"string"},
		{typeof(bool),"bool"},
		{typeof(Color),"Color"},
		{typeof(Vector3),"Vector3"},
		{typeof(Vector2),"Vector2"},
		{typeof(Quaternion),"Quaternion"},
        {typeof(Enum),"Enum"}
    };


	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		EditorGUILayout.Space();

		var methods = target.GetType()
			.GetMembers(BindingFlags.Instance | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public |
				BindingFlags.NonPublic)
			.Where(o => Attribute.IsDefined(o, typeof (EditorButtonAttribute)));

		int methodIndex = 0;

		if (editorButtonStates == null) {
			CreateEditorButtonStates (methods.Select(member => (MethodInfo)member).ToArray());
		}

		foreach (var memberInfo in methods)
		{
			var method = memberInfo as MethodInfo;
			DrawButtonforMethod (targets,method,GetEditorButtonState(method,methodIndex));
			methodIndex++;
		}
	}

	void CreateEditorButtonStates(MethodInfo[] methods) {
		editorButtonStates = new EditorButtonState[methods.Length];
		int methodIndex = 0;
		foreach (var methodInfo in methods) {
			editorButtonStates [methodIndex] = new EditorButtonState (methodInfo.GetParameters ().Length);
			methodIndex++;
		}
	}

	EditorButtonState GetEditorButtonState(MethodInfo method,int methodIndex) {
		return editorButtonStates [methodIndex];
	}

	void DrawButtonforMethod(object[] invokationTargets, MethodInfo methodInfo, EditorButtonState state) {

		var methodParameters = methodInfo.GetParameters();

		EditorGUILayout.BeginHorizontal ();
		if (methodParameters.Length > 0)
		{
			var foldoutRect = EditorGUILayout.GetControlRect(GUILayout.Width(10.0f));
			state.opened = EditorGUI.Foldout(foldoutRect, state.opened, "");
		}
		bool clicked = GUILayout.Button (/*MethodDisplayName(methodInfo)*/ SplitCamelCase(methodInfo.Name), GUILayout.ExpandWidth(true));
		EditorGUILayout.EndHorizontal ();
		EditorGUILayout.Space();

		if (state.opened) {
			EditorGUI.indentLevel++;
			int paramIndex = 0;
			foreach (ParameterInfo parameterInfo in methodParameters) {
				object currentVal = state.parameters [paramIndex];
				state.parameters[paramIndex] = DrawParameterInfo (parameterInfo,currentVal);
				paramIndex++;
			}
			EditorGUI.indentLevel--;
			EditorGUILayout.Space(20);
		}

		if (clicked) {

			foreach (var invokationTarget in invokationTargets)
			{
				if (invokationTarget is MonoBehaviour)
				{
					var target = invokationTarget as MonoBehaviour;
					object returnVal = methodInfo.Invoke(target, state.parameters);

					if (returnVal is IEnumerator)
					{
						target.StartCoroutine((IEnumerator)returnVal);
					}
					else if (returnVal != null)
					{
						Debug.Log(returnVal);
					}
				}
				else /*if (invokationTarget is ScriptableObject)*/
				{
					//var target = invokationTarget as ScriptableObject;
					//object returnVal = methodInfo.Invoke(target, state.parameters);
					object returnVal = methodInfo.Invoke(invokationTarget, state.parameters);
					
					if (returnVal != null)
					{
						Debug.Log(returnVal);
					}
				}
			}
		}
	}

	object GetDefaultValue(ParameterInfo parameter)
	{
		bool hasDefaultValue = !DBNull.Value.Equals (parameter.DefaultValue);

		if (hasDefaultValue)
			return parameter.DefaultValue;

		Type parameterType = parameter.ParameterType;
		if (parameterType.IsValueType)
			return Activator.CreateInstance(parameterType);

		return null;
	}

	object DrawParameterInfo(ParameterInfo parameterInfo, object currentValue) {

		object paramValue = null;

		EditorGUILayout.BeginHorizontal ();
		EditorGUILayout.LabelField (SplitCamelCase(parameterInfo.Name), GUILayout.MaxWidth(100));

		ParameterDrawer drawer = GetParameterDrawer (parameterInfo);
		if (currentValue == null)
			currentValue = GetDefaultValue (parameterInfo);
		paramValue = drawer.Invoke (parameterInfo, currentValue);

		EditorGUILayout.EndHorizontal ();

		return paramValue;
	}

	ParameterDrawer GetParameterDrawer(ParameterInfo parameter) {
		Type parameterType = parameter.ParameterType;

		if (typeof(UnityEngine.Object).IsAssignableFrom(parameterType))
			return DrawUnityEngineObjectParameter;
		
		ParameterDrawer drawer;
		if (typeDrawer.TryGetValue (parameterType, out drawer))
        {
			return drawer;
		}
        else if (typeDrawer.TryGetValue(parameterType.BaseType, out drawer))
        {
            return drawer;
        }

        return null;
	}

	static object DrawFloatParameter(ParameterInfo parameterInfo,object val) {
		//Since it is legal to define a float param with an integer default value (e.g void method(float p = 5);)
		//we must use Convert.ToSingle to prevent forbidden casts
		//because you can't cast an "int" object to float 
		//See for http://stackoverflow.com/questions/17516882/double-casting-required-to-convert-from-int-as-object-to-float more info

		return EditorGUILayout.FloatField (Convert.ToSingle(val));
	}

	static object DrawIntParameter(ParameterInfo parameterInfo,object val) {
		return EditorGUILayout.IntField ((int)val);
	}

	static object DrawBoolParameter(ParameterInfo parameterInfo,object val) {
		return EditorGUILayout.Toggle ((bool)val);
	}

	static object DrawStringParameter(ParameterInfo parameterInfo,object val) {
		return EditorGUILayout.TextField ((string)val);
	}

	static object DrawColorParameter(ParameterInfo parameterInfo,object val) {
		return EditorGUILayout.ColorField ((Color)val);
	}

	static object DrawUnityEngineObjectParameter(ParameterInfo parameterInfo,object val) {
		return EditorGUILayout.ObjectField ((UnityEngine.Object)val, parameterInfo.ParameterType, true);
	}

	static object DrawVector2Parameter(ParameterInfo parameterInfo,object val) {
		return EditorGUILayout.Vector2Field ("", (Vector2)val);
	}

	static object DrawVector3Parameter(ParameterInfo parameterInfo,object val) {
		return EditorGUILayout.Vector3Field ("", (Vector3)val);
	}

	static object DrawQuaternionParameter(ParameterInfo parameterInfo,object val) {
		return Quaternion.Euler(EditorGUILayout.Vector3Field ("", ((Quaternion)val).eulerAngles));
	}

    static object DrawEnumParameter(ParameterInfo parameterInfo, object val) {
		return EditorGUILayout.EnumPopup ("", ((Enum)val));
	}

	string MethodDisplayName(MethodInfo method) {
		StringBuilder sb = new StringBuilder ();
		sb.Append (method.Name +"(");
		var methodParams = method.GetParameters ();
		foreach (ParameterInfo parameter in methodParams) {
			sb.Append (MethodParameterDisplayName (parameter));
			sb.Append (",");
		}

		if(methodParams.Length > 0)
			sb.Remove (sb.Length - 1, 1);
		
		sb.Append (")");
		return sb.ToString ();
	}

	string MethodParameterDisplayName(ParameterInfo parameterInfo) {
		string parameterTypeDisplayName;
		if (!typeDisplayName.TryGetValue (parameterInfo.ParameterType,out parameterTypeDisplayName)) {
			parameterTypeDisplayName = parameterInfo.ParameterType.ToString ();
		}

		return parameterTypeDisplayName + " " + parameterInfo.Name; 
	}

	string MethodUID(MethodInfo method) {
		StringBuilder sb = new StringBuilder ();
		sb.Append (method.Name +"_");
		foreach (ParameterInfo parameter in method.GetParameters()) {
			sb.Append (parameter.ParameterType.ToString ());
			sb.Append ("_");
			sb.Append (parameter.Name);
		}
		sb.Append (")");
		return sb.ToString ();
	}

	private string SplitCamelCase(string input)
	{
		input = input.Replace(input[0], char.ToUpper(input[0]));
		return Regex.Replace(input, "([a-z](?=[A-Z])|[A-Z](?=[A-Z][a-z]))", "$1 ");
	}
}
#endif