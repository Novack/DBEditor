using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;

namespace DBEditor
{
	[InitializeOnLoad]
	public static class SelectionHistoryInitialized
	{
		static SelectionHistoryInitialized()
		{
			SelectionHistoryWindow.RegisterSelectionListener();
		}
	}

	public class SelectionHistoryWindow : EditorWindow
	{
		static readonly SelectionHistory selectionHistory = new SelectionHistory();

		// Add menu named "My Window" to the Window menu
		[MenuItem ("Window/Gemserk/Selection History %#h")]
		static void Init()
		{
			// Get existing open window or if none, make a new one:
//			var window = ScriptableObject.CreateInstance<SelectionHistoryWindow>();
			var window = EditorWindow.GetWindow<SelectionHistoryWindow> ();

			window.titleContent.text = "History";
			window.Show();
		}

		static void SelectionRecorder ()
		{
			if (Selection.activeObject != null)
			{
				selectionHistory.UpdateSelection (Selection.activeObject);
			} 
		}

		public static void RegisterSelectionListener()
		{
			Selection.selectionChanged += SelectionRecorder;
		}

		void OnEnable()
		{
			automaticRemoveDeleted = true;
			
			Selection.selectionChanged += OnSelectionChanged;
		}
		
		void OnSelectionChanged()
		{
			if (selectionHistory.IsSelected(selectionHistory.GetHistoryCount() - 1))
			{
				scrollPosition.y = float.MaxValue;
			}
			
			Repaint();
		}

		void UpdateSelection(int currentIndex)
		{
			/*Selection.activeObject = */selectionHistory.UpdateSelection(currentIndex);
			GUI.FocusControl(null);
		}

		Vector2 scrollPosition;

		bool automaticRemoveDeleted;
		bool allowDuplicatedEntries;

		void OnGUI()
		{
			DrawSearchBar();
			
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
			
			EditorGUILayout.BeginVertical(GUILayout.Width(500f));

			var selected = selectionHistory.GetSelection();// as GameObject;
			if (selected != null)
			{
				Editor editor = Editor.CreateEditor(selected);
				//editor.OnInspectorGUI();    
				editor.DrawDefaultInspector();
			}

			EditorGUILayout.EndVertical();
			
			EditorGUILayout.EndHorizontal();

			if (GUILayout.Button("Clear")) {
				selectionHistory.Clear();
				Repaint();
			}
		}
		
		string searchString = "";
		bool someOption;
		GUISkin editorSkin;
		void DrawSearchBar()
		{
			if (editorSkin == null)
				editorSkin = ScriptableObject.Instantiate(EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector)) as GUISkin;
			
			GUILayout.BeginHorizontal(editorSkin.FindStyle("Toolbar"));
			someOption = GUILayout.Toggle(someOption, "Toggle Me", EditorStyles.toolbarButton);
			GUILayout.Button("Pepe", EditorStyles.toolbarDropDown);
			GUILayout.FlexibleSpace();
			
			searchString = GUILayout.TextField(searchString, editorSkin.FindStyle("ToolbarSeachTextFieldPopup"), GUILayout.Width(100));
			if (string.IsNullOrEmpty(searchString))
			{
				GUILayout.Button("", editorSkin.FindStyle("ToolbarSeachCancelButtonEmpty"));
			}
			else
			{
				if (GUILayout.Button("", editorSkin.FindStyle("ToolbarSeachCancelButton")))
				{
	    			// Remove focus if cleared
					searchString = "";
					GUI.FocusControl(null);
				}
			}
			//searchString = EditorGUILayout.TextField(searchString, EditorStyles.toolbarTextField);
			GUILayout.EndHorizontal();
		}

		void DrawHistory()
		{
			var nonSelectedColor = GUI.contentColor;

			var history = selectionHistory.History;

			var buttonStyle = EditorStyles.label; //windowSkin.GetStyle("SelectionButton");

			for (int i = 0; i < history.Count; i++) {
				var historyElement = history [i];

				if (selectionHistory.IsSelected(i)) {
					GUI.contentColor = new Color(0.2f, 170.0f / 255.0f, 1.0f, 1.0f);
				} else {
					GUI.contentColor = nonSelectedColor;
				}

				var rect = EditorGUILayout.BeginHorizontal ();

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
			var currentEvent = Event.current;

			if (currentEvent == null)
				return;

			if (!rect.Contains (currentEvent.mousePosition))
				return;
			
//			Debug.Log (string.Format("event:{0}", currentEvent.ToString()));

			var eventType = currentEvent.type;

			if (eventType == EventType.MouseDrag) {

				if (currentObject != null) {
					DragAndDrop.PrepareStartDrag ();

					DragAndDrop.StartDrag (currentObject.name);

					DragAndDrop.objectReferences = new Object[] { currentObject };

//					if (ProjectWindowUtil.IsFolder(currentObject.GetInstanceID())) {

					// fixed to use IsPersistent to work with all assets with paths.
					if (EditorUtility.IsPersistent(currentObject)) {

						// added DragAndDrop.path in case we are dragging a folder.

						DragAndDrop.paths = new string[] {
							AssetDatabase.GetAssetPath(currentObject)
						};

						// previous test with setting generic data by looking at
						// decompiled Unity code.

						// DragAndDrop.SetGenericData ("IsFolder", "isFolder");
					}
				}

				Event.current.Use ();

			} else if (eventType == EventType.MouseUp) {

				if (currentObject != null) {
					if (Event.current.button == 0) {
						UpdateSelection (currentIndex);
					} else {
						EditorGUIUtility.PingObject (currentObject);
					}
				}

				Event.current.Use ();
			}

		}

	}
}