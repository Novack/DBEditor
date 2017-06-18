using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DBEditor
{
	[CreateAssetMenu()]
	public class DBEditorConfig : ScriptableObject
	{
		public List<ScriptableObject> files;
	}
}