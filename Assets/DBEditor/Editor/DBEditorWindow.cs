using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;

namespace DBEditor
{
	public class DBEditorWindow : EditorWindow
	{
		public DBEditorConfig config;

		private string _searchString;
		private bool _inspectorLock;
		private GUISkin _editorSkin;
		private TreeViewState _treeViewState;
		private DBEditorTreeView _dbEditorTreeView;
		private Object[] _selected;

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

			_editorSkin = ScriptableObject.Instantiate(EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector)) as GUISkin;
		}
		
		void OnGUI()
		{
			DrawToolBar();
			
			EditorGUILayout.BeginHorizontal();
			
			EditorGUILayout.BeginVertical(GUILayout.Width(270f));
			
			GUILayout.FlexibleSpace();

			_dbEditorTreeView.OnGUI(new Rect(10, 25, 250, position.height - 35));

			EditorGUILayout.EndVertical();

			EditorGUILayout.BeginVertical();
			
			if (!_inspectorLock)
				_selected = _dbEditorTreeView.GetSelectedObjects();
			
			if (_selected != null)
			{
				EditorGUILayout.BeginHorizontal(_editorSkin.FindStyle("ProjectBrowserTopBarBg"));
				GUILayout.FlexibleSpace();
				_inspectorLock = GUILayout.Toggle(_inspectorLock, "", _editorSkin.FindStyle("IN LockButton"));
				GUILayout.Space(5);
				EditorGUILayout.EndHorizontal();
				
				Editor editor = Editor.CreateEditor(_selected);
				editor.DrawHeader();
				editor.DrawDefaultInspector();
				//editor.OnInspectorGUI(); 
			}

			EditorGUILayout.EndVertical();
			
			EditorGUILayout.EndHorizontal();
		}
		
		void DrawToolBar()
		{
			GUILayout.BeginHorizontal(_editorSkin.FindStyle("Toolbar"));
			
			if (GUILayout.Button("Expand All", _editorSkin.FindStyle("toolbarbutton")))
			{
				_dbEditorTreeView.ExpandAll();
			}
			
			if (GUILayout.Button("Collapse All", _editorSkin.FindStyle("toolbarbutton")))
			{
				_dbEditorTreeView.CollapseAll();
			}	
			
			GUILayout.Button("Create New", EditorStyles.toolbarDropDown);
			
			var selected = _dbEditorTreeView.GetSelectedObjects();
			if (selected != null)
			{
				if (GUILayout.Button("Find in Project", _editorSkin.FindStyle("toolbarbutton")))
				{
					EditorGUIUtility.PingObject(selected[0]);
				}
				
				if (GUILayout.Button("Rename", _editorSkin.FindStyle("toolbarbutton")))
				{
					_dbEditorTreeView.StartRename();
				}
				
				if (GUILayout.Button("Duplicate", _editorSkin.FindStyle("toolbarbutton")))
				{
					_dbEditorTreeView.Duplicate();
				}
				
				if (GUILayout.Button("Delete", _editorSkin.FindStyle("toolbarbutton")))
				{
					_dbEditorTreeView.Delete();
				}
			}
			
			GUILayout.FlexibleSpace();
			
			_searchString = GUILayout.TextField(_searchString, _editorSkin.FindStyle("ToolbarSeachTextFieldPopup"), GUILayout.Width(140));
			var editorStyle = string.IsNullOrEmpty(_searchString) ? "ToolbarSeachCancelButtonEmpty" : "ToolbarSeachCancelButton";
			if (GUILayout.Button("", _editorSkin.FindStyle(editorStyle)))
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