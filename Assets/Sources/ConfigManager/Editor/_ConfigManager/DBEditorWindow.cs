using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace ConfigManager
{
    public class DBEditorWindow : EditorWindow
    {
        private static bool _loaded;
        private static float _delay;
        private static double _lastTime;

        public CMConfig config;

        private const string _tabName = "Config Manager";

        private Editor _editor;
        private string _searchString;
        private bool _inspectorLock;
        private bool _inspectorWasLocked;
        private TreeViewState _treeViewState;
        private DBEditorTreeView _dbEditorTreeView;
        private Object[] _selected;
        private bool _sameTypeCached;
        private Object[] _selectedCached; // If the inspector is locked, we save the current selection apart.
        private GenericMenu _createMenu;
        private Vector2 _scrollPos;
        private float _treeViewLayoutWidth = 265f;
        private Rect _treeViewRect;
        private bool _dragging;
        private float _splitterWidth = 0;
        private float _labelWidth;
        private Texture2D _refreshIcon;
        private Texture2D _titleBarButtonIcon;
        private GUIStyle _tittleBarButtonStyle;

        private bool _stylesDefined;
        private GUIStyle _toolbarButtonStyle;
        private GUIStyle _projectBrowserTopBarBgStyle;
        private GUIStyle _inLockButtonStyle;
        private GUIStyle _toolbarStyle;
        private GUIStyle _toolbarDropDownStyle;
        private GUIStyle _toolbarSearchTextFieldPopupStyle;
        private GUIStyle _toolbarSearchCancelButtonStyle;
        private GUIStyle _toolbarSearchCancelButtonEmptyStyle;


        [MenuItem("Tools/Config Manager", priority = 5000)]
        static void Init()
        {
            if (!_loaded)
            {
                Debug.Log("Loading DB Editor Window. First load may take a few seconds...");
                _loaded = true;
                _delay = 0.1f;
                _lastTime = EditorApplication.timeSinceStartup;
                EditorApplication.update += OnEditorUpdate;
                return;
            }

            LoadWindow();
        }

        private static void OnEditorUpdate()
        {
            _delay -= (float)(EditorApplication.timeSinceStartup - _lastTime);
            if (_delay > 0)
            {
                _lastTime = EditorApplication.timeSinceStartup;
                return;
            }

            LoadWindow();
            EditorApplication.update -= OnEditorUpdate;
        }

        private void DefineStyles()
        {
            if (_stylesDefined)
                return;

            _toolbarButtonStyle = (GUIStyle)"toolbarbutton";
            _projectBrowserTopBarBgStyle = (GUIStyle)"ProjectBrowserTopBarBg";
            _inLockButtonStyle = (GUIStyle)"IN LockButton";
            _toolbarStyle = (GUIStyle)"toolbar";
            _toolbarDropDownStyle = (GUIStyle)"toolbarDropDown";
            _toolbarSearchTextFieldPopupStyle = (GUIStyle)"ToolbarSearchTextFieldPopup";
            _toolbarSearchCancelButtonStyle = (GUIStyle)"ToolbarSearchCancelButton";
            _toolbarSearchCancelButtonEmptyStyle = (GUIStyle)"ToolbarSearchCancelButtonEmpty";

            _stylesDefined = true;
        }

        private static void LoadWindow()
        {
            var window = EditorWindow.GetWindow<DBEditorWindow>();
            window.titleContent.text = _tabName;
            window.Show();
        }

        private void Awake()
        {
            _searchString = "";
            _treeViewRect = new Rect(10, 25, _treeViewLayoutWidth - 15, position.height - 35);

            _refreshIcon = EditorGUIUtility.FindTexture("d_TreeEditor.Refresh");
            _titleBarButtonIcon = EditorGUIUtility.FindTexture("d_winbtn_win_close");
        }

        private void OnEnable()
        {
            if (_treeViewState == null)
                _treeViewState = new TreeViewState();

            _dbEditorTreeView = new DBEditorTreeView(_treeViewState, config);
            _dbEditorTreeView.selectionChange += OnTreeViewSelectionChange;
            _labelWidth = config.LabelWidth;

            LoadCreateMenu();
        }

        private void LoadCreateMenu()
        {
            var possibleElements = _dbEditorTreeView.GetElementTypes();
            _createMenu = new GenericMenu();
            var en = possibleElements.GetEnumerator();
            while (en.MoveNext())
            {
                var item = en.Current;
                _createMenu.AddItem(new GUIContent(item.Value), false, _dbEditorTreeView.CreateNewElement, item.Key);
            }
        }

        private void OnTreeViewSelectionChange(Object[] selection, bool sameType)
        {
            _selectedCached = selection;
            _sameTypeCached = sameType;

            if (_inspectorLock)
                return;

            if (sameType)
            {
                _selected = selection;
                _editor = Editor.CreateEditor(selection);
            }
            else
            {
                _selected = null;
                _editor = null;
            }
        }

        private void OnGUI()
        {
            DefineStyles();
            DrawToolBar();

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginVertical(GUILayout.Width(_treeViewLayoutWidth));

            GUILayout.FlexibleSpace();

            _treeViewRect.height = position.height - 35;
            _treeViewRect.width = _treeViewLayoutWidth - 15;
            _dbEditorTreeView.OnGUI(_treeViewRect);
            _dbEditorTreeView.searchString = _searchString;

            EditorGUILayout.EndVertical();

            // Splitter
            GUILayout.Box(GUIContent.none, GUILayout.Width(_splitterWidth), GUILayout.ExpandHeight(true));
            HandleSplitView();
            GUILayout.Space(5);

            EditorGUILayout.BeginVertical();

            EditorGUIUtility.labelWidth = _labelWidth;

            if (_selected != null && _selected.Length > 0)
            {
                EditorGUILayout.BeginHorizontal(_projectBrowserTopBarBgStyle);
                GUILayout.FlexibleSpace();
                _inspectorLock = GUILayout.Toggle(_inspectorLock, "", _inLockButtonStyle);
                if (_inspectorLock != _inspectorWasLocked)
                {
                    OnTreeViewSelectionChange(_selectedCached, _sameTypeCached);
                    _inspectorWasLocked = _inspectorLock;

                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                    return;
                }

                GUILayout.Space(10);
                EditorGUILayout.EndHorizontal();

                _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
                //EditorGUILayout.InspectorTitlebar(true, _selected);
                _editor.DrawHeader();
                //editor.DrawDefaultInspector();
                _editor.OnInspectorGUI();

                GUILayout.Space(30);

                EditorGUILayout.EndScrollView();
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();

            HandleKeyboard();
        }

        /// <summary>
        /// Draw buttons on toolbar. Automatically called by unity.
        /// </summary>
        private void ShowButton(Rect position)
        {
            if (_tittleBarButtonStyle == null)
            {
                _tittleBarButtonStyle = new GUIStyle(GUI.skin.button)
                {
                    padding = new RectOffset()
                };
            }

            if (GUI.Button(position, _titleBarButtonIcon, _tittleBarButtonStyle))
            {
                this.Close();
            }
        }

        void DrawToolBar()
        {
            GUILayout.BeginHorizontal(_toolbarStyle);

            if (GUILayout.Button(_refreshIcon, _toolbarButtonStyle))
            {
                _dbEditorTreeView.Reload();
                LoadCreateMenu();
            }

            if (GUILayout.Button("Expand All", _toolbarButtonStyle))
            {
                _dbEditorTreeView.ExpandAll();
            }

            if (GUILayout.Button("Collapse All", _toolbarButtonStyle))
            {
                _dbEditorTreeView.CollapseAll();
            }

            /*
            GUILayout.Space(10);

            if (GUILayout.Button("AZ↓", _toolbarButtonStyle))
			{
				_dbEditorTreeView.SortAlphabetically();
			}
            */
            GUILayout.Space(10);

            if (GUILayout.Button("Create New", _toolbarDropDownStyle))
            {
                _createMenu.DropDown(new Rect(140f, -83f, 50f, 100f));
            }

            if (GUILayout.Button("Save All", _toolbarButtonStyle))
            {
                _dbEditorTreeView.SaveAll();
                this.ShowNotification(new GUIContent("Saving..."), 0.75f);
            }

            if (_selected != null && _selected.Length == 1)
            {
                GUILayout.Space(15);

                if (GUILayout.Button("Open", _toolbarButtonStyle))
                {
                    _dbEditorTreeView.OpenAsset();
                }

                if (GUILayout.Button("Rename", _toolbarButtonStyle))
                {
                    _dbEditorTreeView.StartRename();
                }

                if (GUILayout.Button("Duplicate", _toolbarButtonStyle))
                {
                    _dbEditorTreeView.Duplicate();
                }

                if (GUILayout.Button("Delete", _toolbarButtonStyle))
                {
                    _dbEditorTreeView.Delete();
                }

                GUILayout.Space(5);

                if (GUILayout.Button("Find in Project", _toolbarButtonStyle))
                {
                    EditorGUIUtility.PingObject(_selected[0]);
                }
            }

            GUILayout.FlexibleSpace();

            _searchString = GUILayout.TextField(_searchString, _toolbarSearchTextFieldPopupStyle, GUILayout.Width(200));
            var searchStyle = string.IsNullOrEmpty(_searchString) ? _toolbarSearchCancelButtonEmptyStyle : _toolbarSearchCancelButtonStyle;
            if (GUILayout.Button("", searchStyle))
            {
                _searchString = "";
                GUI.FocusControl(null);
                _dbEditorTreeView.FocusSelected();
            }

            GUILayout.Space(5);

            _dbEditorTreeView.deepSearchEnabled = GUILayout.Toggle(_dbEditorTreeView.deepSearchEnabled, "Deep Search", _toolbarButtonStyle);

            GUILayout.EndHorizontal();
        }

        void HandleSplitView()
        {
            Rect splitterRect = GUILayoutUtility.GetLastRect();
            EditorGUIUtility.AddCursorRect(splitterRect, MouseCursor.ResizeHorizontal);

            if (Event.current != null)
            {
                switch (Event.current.rawType)
                {
                    case EventType.MouseDown:
                        if (splitterRect.Contains(Event.current.mousePosition))
                            _dragging = true;
                        break;
                    case EventType.MouseDrag:
                        if (_dragging)
                        {
                            splitterRect.x += Event.current.delta.x;
                            _treeViewLayoutWidth += Event.current.delta.x;
                            Repaint();
                        }
                        break;
                    case EventType.MouseUp:
                        if (_dragging)
                            _dragging = false;
                        break;
                }
            }
        }

        private void HandleKeyboard()
        {
            Event e = Event.current;

            if (e.type == EventType.ValidateCommand && e.commandName == "Duplicate")
                _dbEditorTreeView.Duplicate();

            if (e.type != EventType.KeyDown)
                return;

            switch (e.keyCode)
            {
                case KeyCode.Delete:
                    _dbEditorTreeView.Delete();
                    break;
            }
        }

        protected void OnDestroy()
        {
            //EditorUtility.UnloadUnusedAssetsImmediate();
            //System.GC.Collect();
        }
    }
}