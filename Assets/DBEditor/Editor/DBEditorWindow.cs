using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace DBEditor
{
	public class DBEditorWindow : EditorWindow
	{
		public DBEditorConfig config;

		private string _searchString;
		private bool _inspectorLock;
		private TreeViewState _treeViewState;
		private DBEditorTreeView _dbEditorTreeView;
		private Object[] _selected;
        private GenericMenu _createMenu;

        [MenuItem ("Window/DBEditor")]
		static void Init()
		{
			var window = EditorWindow.GetWindow<DBEditorWindow> ();
			window.titleContent.text = "DBEditor";
			window.Show();
		}
		
		public void Awake()
		{
			_searchString = "";
			
			if (_treeViewState == null)
				_treeViewState = new TreeViewState ();
			
			_dbEditorTreeView = new DBEditorTreeView(_treeViewState, config);

            var possibleElements = _dbEditorTreeView.GetElementTypes();
            _createMenu = new GenericMenu();
            for (int i = 0; i < possibleElements.Count; i++)
            {
                _createMenu.AddItem(new GUIContent(possibleElements[i]), false, _dbEditorTreeView.CreateNewElement, i);
            }
        }
		
		//private GUISkin _editorSkin;
		private GUIStyle GetStyle(string name)
		{
			//if (_editorSkin == null)
			//	_editorSkin = ScriptableObject.Instantiate(EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector)) as GUISkin;
			//GUIStyle style =  _editorSkin.FindStyle(name);
			GUIStyle style = (GUIStyle)name;
			
			return style;
		}

        Vector2 _scrollPos;
		void OnGUI()
		{
			DrawToolBar();
			
			EditorGUILayout.BeginHorizontal();
			
			EditorGUILayout.BeginVertical(GUILayout.Width(270f));
			
			GUILayout.FlexibleSpace();

			_dbEditorTreeView.OnGUI(new Rect(10, 25, 250, position.height - 35));
			_dbEditorTreeView.searchString = _searchString;

			EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();
			
			if (!_inspectorLock)
				_selected = _dbEditorTreeView.GetSelectedObjects();			

			if (_selected != null)
			{
				EditorGUILayout.BeginHorizontal(GetStyle("ProjectBrowserTopBarBg"));
                GUILayout.FlexibleSpace();
				_inspectorLock = GUILayout.Toggle(_inspectorLock, "", GetStyle("IN LockButton"));
				GUILayout.Space(10);
				EditorGUILayout.EndHorizontal();
				
                _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

				Editor editor = Editor.CreateEditor(_selected);
                //EditorGUILayout.InspectorTitlebar(true, _selected);
                editor.DrawHeader();
				editor.DrawDefaultInspector();
                //editor.OnInspectorGUI(); 
                GUILayout.Space(30);

                EditorGUILayout.EndScrollView();
			}

			EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
		}
		
		void DrawToolBar()
		{
			GUILayout.BeginHorizontal(GetStyle("toolbar"));
			
			if (GUILayout.Button("Expand All", GetStyle("toolbarbutton")))
			{
				_dbEditorTreeView.ExpandAll();
			}
			
			if (GUILayout.Button("Collapse All", GetStyle("toolbarbutton")))
			{
				_dbEditorTreeView.CollapseAll();
			}			
            
			if (GUILayout.Button("Create New", GetStyle("toolbarDropDown")))
            {
                _createMenu.DropDown(new Rect(140f, -83f, 50f, 100f));
            }

            var selected = _dbEditorTreeView.GetSelectedObjects();
			if (selected != null && selected.Length == 1)
			{
				if (GUILayout.Button("Find in Project", GetStyle("toolbarbutton")))
				{
					EditorGUIUtility.PingObject(selected[0]);
				}
				
				if (GUILayout.Button("Rename", GetStyle("toolbarbutton")))
				{
					_dbEditorTreeView.StartRename();
				}
				
				if (GUILayout.Button("Duplicate", GetStyle("toolbarbutton")))
				{
					_dbEditorTreeView.Duplicate();
				}
				
				if (GUILayout.Button("Delete", GetStyle("toolbarbutton")))
				{
					_dbEditorTreeView.Delete();
				}
			}
			
			GUILayout.FlexibleSpace();
			
			_searchString = GUILayout.TextField(_searchString, GetStyle("ToolbarSeachTextFieldPopup"), GUILayout.Width(200));
			var editorStyle = string.IsNullOrEmpty(_searchString) ? "ToolbarSeachCancelButtonEmpty" : "ToolbarSeachCancelButton";
			if (GUILayout.Button("", GetStyle(editorStyle)))
			{
				_searchString = "";
				GUI.FocusControl(null);
			}
			
			GUILayout.EndHorizontal();
		}
		
		protected void OnDestroy()
		{
			//EditorUtility.UnloadUnusedAssetsImmediate();
			//System.GC.Collect();
		}
	}
}