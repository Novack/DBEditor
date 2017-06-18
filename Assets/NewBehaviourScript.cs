using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu()]
public class NewBehaviourScript : ScriptableObject
{
	[Serializable]
	public class marisco
	{
		public string name;
		public int size;
	}
	
	[HideInInspector]
	public int none;
	public float lala;
	public List<int> lele;
	public int[] lilo;
	public string strele;
	[Header("gffgsfdcg")]
	public Color col;
	[Space(10)]
	public Vector3 vec;
	[Tooltip("vervever")]
	public marisco caracok;
	

}
