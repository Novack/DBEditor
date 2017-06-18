using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace DBEditor
{
	public class SelectionHistoryWindow : EditorWindow
	{
		public DBEditorConfig config;
		
		private SelectionHistory selectionHistory;
		private Vector2 scrollPosition;
		private string searchString = "";
		private bool someOption;
		private GUISkin editorSkin;

		[MenuItem ("Window/DBEditor")]
		static void Init()
		{
			// Get existing open window or if none, make a new one:
//			var window = ScriptableObject.CreateInstance<SelectionHistoryWindow>();
			var window = EditorWindow.GetWindow<SelectionHistoryWindow> ();

			window.titleContent.text = "DBEditor";
			window.Show();
		}
		
		public void Awake()
		{
			selectionHistory = new SelectionHistory();
			selectionHistory.LoadDB(config);
			
			editorSkin = ScriptableObject.Instantiate(EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector)) as GUISkin;
		}

		void UpdateSelection(int currentIndex)
		{
			selectionHistory.UpdateSelection(currentIndex);
			GUI.FocusControl(null);
		}

		void OnGUI()
		{
			DrawToolBar();
			
			EditorGUILayout.BeginHorizontal();
			
			EditorGUILayout.BeginVertical(GUILayout.Width(100f));
			GUILayout.Button("Pepa");
			GUILayout.Button("Pepa");
			GUILayout.Button("Pepa");
			EditorGUILayout.EndVertical();
			
			bool changedBefore = GUI.changed;

			//scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, false, true);
			//bool changedAfter = GUI.changed;

			//if (!changedBefore && changedAfter) {
			//	Debug.Log ("changed");
			//}
			
			EditorGUILayout.BeginVertical(GUILayout.Width(250));

			DrawHistory();
			
			EditorGUILayout.EndVertical();
			
			//EditorGUILayout.EndScrollView();
			
			EditorGUILayout.BeginVertical(/*GUILayout.Width(500f)*/);

			var selected = selectionHistory.CurrentSelection;// as GameObject;
			if (selected != null)
			{
				Editor editor = Editor.CreateEditor(selected);
				//editor.OnInspectorGUI();    
				editor.DrawDefaultInspector();
			}

			EditorGUILayout.EndVertical();
			
			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.Space();

			if (GUILayout.Button("Clear"))
			{
				selectionHistory.History.Clear();
				Repaint();
			}
		}
		
		void DrawToolBar()
		{
			GUILayout.BeginHorizontal(editorSkin.FindStyle("Toolbar"));
			
			someOption = GUILayout.Toggle(someOption, "Toggle Me", EditorStyles.toolbarButton);
			GUILayout.Button("DropDownBtn", EditorStyles.toolbarDropDown);
			
			if (selectionHistory.CurrentSelection != null)
			{
				if (GUILayout.Button("Find in Project", editorSkin.FindStyle("toolbarbutton")))
				{
					EditorGUIUtility.PingObject(selectionHistory.CurrentSelection);
				}
			}
			
			GUILayout.FlexibleSpace();
			
			searchString = GUILayout.TextField(searchString, editorSkin.FindStyle("ToolbarSeachTextFieldPopup"), GUILayout.Width(100));
			var editorStyle = string.IsNullOrEmpty(searchString) ? "ToolbarSeachCancelButtonEmpty" : "ToolbarSeachCancelButton";
			if (GUILayout.Button("", editorSkin.FindStyle(editorStyle)))
			{
				searchString = "";
				GUI.FocusControl(null);
			}
			
			GUILayout.EndHorizontal();
		}

		void DrawHistory()
		{
			var nonSelectedColor = GUI.contentColor;
			var history = selectionHistory.History;
			var buttonStyle = editorSkin.GetStyle("Label");

			for (int i = 0; i < history.Count; i++)
			{
				var historyElement = history [i];

				if (selectionHistory.IsSelected(i))
				{
					GUI.contentColor = new Color(0.2f, 170.0f / 255.0f, 1.0f, 1.0f);
				}
				else
				{
					GUI.contentColor = nonSelectedColor;
				}

				var rect = EditorGUILayout.BeginHorizontal();

				//if (historyElement == null) {
				//	GUILayout.Label ("Deleted", buttonStyle); 
				//} else {

					var icon = AssetPreview.GetMiniThumbnail (historyElement);

					GUIContent content = new GUIContent ();

					content.image = icon;
					content.text = historyElement.name;

					// chnanged to label to be able to handle events for drag
					GUILayout.Label (content, buttonStyle, GUILayout.Height(20)); 

					GUI.contentColor = nonSelectedColor;

				//	if (GUILayout.Button ("Ping", windowSkin.button)) {
				//		EditorGUIUtility.PingObject (historyElement);
				//	}

				//}
					
				EditorGUILayout.EndHorizontal ();

				ButtonLogic (i, rect, historyElement);
			}

			GUI.contentColor = nonSelectedColor;
		}

		void ButtonLogic(int currentIndex, Rect rect, Object currentObject)
		{
			if (Event.current == null)
				return;

			if (!rect.Contains(Event.current.mousePosition))
				return;
			
			if (currentObject == null)
				return;
			
//			Debug.Log(string.Format("event:{0}", currentEvent.ToString()));

			if (Event.current.type == EventType.MouseDrag)
			{
				DragAndDrop.PrepareStartDrag();
				DragAndDrop.StartDrag(currentObject.name);
				DragAndDrop.objectReferences = new Object[] { currentObject };

				Event.current.Use();
			}
			else if (Event.current.type == EventType.MouseUp)
			{
				if (Event.current.button == 0)
				{
					UpdateSelection(currentIndex);
				}

				Event.current.Use();
			}
		}
	}
}