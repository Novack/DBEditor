using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DBEditor
{
	[CreateAssetMenu()]
	public class DBEditorConfig : ScriptableObject
	{
		public int MaxCategoryId = 100;
		public List<ScriptableObject> files;
	}
}