using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace DBEditor
{
	[CreateAssetMenu()]
	public class DBEditorConfig : ScriptableObject
	{		
		[Serializable]
		public class Config
		{
			[Tooltip("The path to the folder to contain newly created objects of this category. 'Assets/...'")]
			public string FileStoragePath = "Assets/...";
			[Tooltip("The ScriptableObject script class name.")]
			public string ClassName;
			[Tooltip("The parent category structure for this files. Use '/' to nest.")]
			public string TreeViewPath;
			[Tooltip("The ScriptableObject instances to include manually.")]
			public List<ScriptableObject> Files;
		}
		
		
		public int MaxCategoryId = 10000;
		
		public List<Config> Configs;
	}
}