using System.Collections.Generic;
using UnityEngine;

namespace DBEditor
{
	public class SelectionHistory
	{
		public List<Object> History;
		public Object CurrentSelection;
		
		public void LoadDB(DBEditorConfig config)
		{
			History = new List<Object>(100);
			for (int i = 0; i < config.files.Count; i++)
			{
				History.Add(config.files[i]);
			}
		}
		
		public bool IsSelected(int index)
		{
			return History[index] == CurrentSelection;
		}

		public Object UpdateSelection(int currentIndex)
		{
			CurrentSelection = History[currentIndex];
			return CurrentSelection;
		}
	}
}
