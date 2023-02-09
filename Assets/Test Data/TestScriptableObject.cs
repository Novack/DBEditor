using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

//[CreateAssetMenu()]
public class TestScriptableObject : ScriptableObject
{
	[Serializable]
	public class marisco
	{
		public string name;
		public int size;
	}
	
	public TestScriptableObject testScriptableObject;
	public List<TestScriptableObject> testScriptableObjects;
		
	[HideInInspector]
	public int none;
    public float standardFloat;
    [Range(0f, 1f)]
    public float lala;
	public List<int> lele;
	public int[] lilo;
	public string strele;
	[TextArea]
	public string MyTextArea;
	[Header("gffgsfdcg")]
	public Color col;
	[Space(10)]
	public Vector3 vec;
	[Tooltip("vervever")]
	public marisco caracok;
    [Space(10)]
    [Tooltip("Tests for custom Property Drawers")]
    public MyPropertyType Field1;
	public List<MyPropertyType> mypropList;
    [Space(10)]
    [Tooltip("Tests for Custom Inspectors")]
    public Transform trans;
    [Space(10)]
    [Tooltip("Tests for Custom Property Attributes")]
    //[Regex(@"^(?:\d{1,3}\.){3}\d{1,3}$", "Invalid IP address!\nExample: '127.0.0.1'")]
    public string serverAddress = "192.168.0.1";
    [Multiline()]
	public string multilineString;
	[Space(10)]
	public string normalString;
	[Header("Localization")]
	public int somethingElse;
	[Time]
    [Tooltip("Testing tooltip...")]
	public float myTimeTest;


}
