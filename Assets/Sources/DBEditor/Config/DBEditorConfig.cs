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
			[Tooltip("The parent category structure for this files. Use '/' to nest.")]
			public string TreeViewPath;
			[Tooltip("The path to the folder to contain newly created objects of this category. 'Assets/...'")]
			public string FileStoragePath = "Assets/...";
			[Tooltip("The ScriptableObject script class name.")]
			public string ClassName;
            [Tooltip("This option will auto load all assets of the ClassName to the files list below.")]
            public bool AutoScan;
			[Tooltip("The ScriptableObject instances to include manually.")]
			public List<UnityEngine.Object> Files;
		}
		
		
		public int MaxCategoryId = 10000;
        [Tooltip("The width in pixels reserved for labels in the DB Editor.")]
        public float LabelWidth = 220f;

        [Space(15)]
		public List<Config> Configs;
	}
}