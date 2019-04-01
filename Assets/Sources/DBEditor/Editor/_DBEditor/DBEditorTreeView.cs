using UnityEditor.IMGUI.Controls;
using System.Collections.Generic;
using DBEditor;
using UnityEditor;
using UnityEngine;

class DBEditorTreeView : TreeView
{
	private DBEditorConfig config;
	private int _currentCatId;
    private Texture2D _folderIcon;
    private Texture2D _folderEmptyIcon;
    private Texture2D _scriptableObjectIcon;
    private Dictionary<int, int> _configIdxById;

    public DBEditorTreeView(TreeViewState treeViewState, DBEditorConfig configParam) : base(treeViewState)
	{
		config = configParam;
		showBorder = true;
        _folderIcon = EditorGUIUtility.FindTexture("Folder Icon");
        _folderEmptyIcon = EditorGUIUtility.FindTexture("FolderEmpty Icon");
        _scriptableObjectIcon = EditorGUIUtility.FindTexture("ScriptableObject Icon");

        Reload();
	}

    #region TreeView Overrides

    protected override TreeViewItem BuildRoot()
	{
		var root = new TreeViewItem { id = 0, depth = -1, displayName = "Root" };

        _currentCatId = 1;
        _configIdxById = new Dictionary<int, int>(); // store the config.Configs index for item meta data lookups (path, classname, etc).
        TreeViewItem treeCategory;
        TreeViewItem cachedCategory = root;
        for (int i = 0; i < config.Configs.Count; i++)
        {
            cachedCategory = root;
            var pathArray = (config.Configs[i].TreeViewPath).Split('/');
            for (int p = 0; p < pathArray.Length; p++)
            {
                if (cachedCategory.children != null)
                {
                    var existing = cachedCategory.children.Find(x => x.displayName == pathArray[p]);
                    if (existing != null)
                    {
                        cachedCategory = existing;
                        continue;
                    }
                }

                treeCategory = new TreeViewItem(_currentCatId);
                treeCategory.displayName = pathArray[p];
                treeCategory.icon = _folderIcon;
                _configIdxById.Add(_currentCatId, i); 
                _currentCatId++;

                cachedCategory.AddChild(treeCategory);
                cachedCategory = treeCategory;
            }

            if (config.Configs[i].AutoScan)
            {
                var tmpObj = ScriptableObject.CreateInstance(config.Configs[i].ClassName);
                if (tmpObj != null)
                {
                    var classType = tmpObj.GetType();
                    ScriptableObject.DestroyImmediate(tmpObj, true);
                    var arr = Resources.FindObjectsOfTypeAll(classType);
                    config.Configs[i].Files = new List<Object>(arr);
                }
            }

            bool sort = true;
            for (int j = 0; j < config.Configs[i].Files.Count; j++)
            {
                if (config.Configs[i].Files[j] == null)
                {
                    Debug.LogWarningFormat("Missing elements, alphabetical sorting not done at category {0}.", config.Configs[i].TreeViewPath);
                    sort = false;
                    break;
                }
            }

            if (sort)
                config.Configs[i].Files.Sort((x, y) => string.Compare(x.name, y.name));

            for (int j = 0; j < config.Configs[i].Files.Count; j++)
            {
                if (config.Configs[i].Files[j] == null)
                {
                    Debug.LogWarningFormat("Missing element expected in config item {0} ({1}), at index position {2}.", config.Configs[i].TreeViewPath, config.Configs[i].ClassName, j);
                    continue;
                }

                var id = config.Configs[i].Files[j].GetInstanceID();
                var item = new TreeViewItem(id);
                if (_configIdxById.ContainsKey(id))
                {
                    Debug.LogWarningFormat("Element {0} with id {1} already exist in the database. Ignoring add at category {2}.", config.Configs[i].Files[j].name, id, config.Configs[i].TreeViewPath);
                    continue;
                }
                _configIdxById.Add(id, i);
                //Debug.Log(config.files[i].GetInstanceID());
                item.displayName = config.Configs[i].Files[j].name;
                item.icon = _scriptableObjectIcon; // AssetPreview.GetMiniThumbnail(config.files[i]);
                cachedCategory.AddChild(item);
            }
        }		
		
		SetupDepthsFromParentsAndChildren(root);
			
	    return root;
	}
	
	protected override bool CanStartDrag(CanStartDragArgs args)
	{
		if (args.draggedItem != null)
		{
			if (Mathf.Abs(args.draggedItem.id) < config.MaxCategoryId)
				return false;
		}
		
		if (args.draggedItemIDs != null && args.draggedItemIDs.Count > 0)
		{
			for (int i = 0; i < args.draggedItemIDs.Count; i++)
			{
				if (Mathf.Abs(args.draggedItemIDs[i]) < config.MaxCategoryId)
					return false;
			}
		}
		
		return true;
	}
	
	protected override void SetupDragAndDrop(SetupDragAndDropArgs args)
	{
        var selection = GetSelectedObjects();
        if (selection == null)
            return;

        DragAndDrop.PrepareStartDrag();
		DragAndDrop.StartDrag("DBEditorDrag");
        DragAndDrop.objectReferences = selection;
	}
	
	protected override DragAndDropVisualMode HandleDragAndDrop(DragAndDropArgs args)
	{
		return DragAndDropVisualMode.Generic;
	}
	
	protected override bool CanRename(TreeViewItem item)
	{
		if (Mathf.Abs(item.id) < config.MaxCategoryId)
			return false;
		
		return true;
	}
	
	protected override void RenameEnded(RenameEndedArgs args)
	{
		if (!args.acceptedRename)
			return;
		
		string assetPath = AssetDatabase.GetAssetPath(args.itemID);
		AssetDatabase.RenameAsset(assetPath, args.newName);
		//AssetDatabase.SaveAssets();
		
		var item = FindItem(args.itemID, rootItem);
		item.displayName = args.newName;
	}
	
	protected override bool CanChangeExpandedState(TreeViewItem item)
	{
		if (Mathf.Abs(item.id) > config.MaxCategoryId)
			return false;

        item.icon = IsExpanded(item.id) ? _folderEmptyIcon : _folderIcon;
        return true;
	}
	
	#endregion
	
	#region Custom Methods
	
    public void CreateNewElement(object configIndex)
    {
        var idx = (int)configIndex;
        var className = config.Configs[idx].ClassName;

        var path = AssetDatabase.GenerateUniqueAssetPath(string.Format("{0}/{1}.asset", config.Configs[idx].FileStoragePath, className));
        var obj = ScriptableObject.CreateInstance(className);
        
        AssetDatabase.CreateAsset(obj, path);
        AssetDatabase.SaveAssets();        

        var id = obj.GetInstanceID();
        config.Configs[idx].Files.Add(obj);

        Reload();
        var selected = new List<int> { id };
        SetSelection(selected, TreeViewSelectionOptions.RevealAndFrame);
        Save();
        //Debug.LogFormat("Created new element of type {0}, id: {1}", className, id);
    }

    public Dictionary<int, string> GetElementTypes()
    {
        var elements = new Dictionary<int, string>();
        for (int i = 0; i < config.Configs.Count; i++)
        {
            if (!string.IsNullOrEmpty(config.Configs[i].ClassName))
                elements.Add(i, config.Configs[i].ClassName);
        }

        return elements;
    }

    public void StartRename()
	{
		if (state.selectedIDs.Count == 0)
			return;
		
		var item = FindItem(state.selectedIDs[0], rootItem);
		BeginRename(item);
	}
	
	public void Delete()
	{
		if (state.selectedIDs.Count == 0)
			return;


        // Only deleting first of the selection list.
        int id = state.selectedIDs[0];
		if (Mathf.Abs(id) < config.MaxCategoryId)
			return;
		
		string assetPath = AssetDatabase.GetAssetPath(id);
        bool check = EditorUtility.DisplayDialog("Detele Selected Asset?", string.Format("{0}\r\rYou cannot undo this action.", assetPath), "Delete", "Cancel");
        if (!check)
            return;

		var item = FindItem(id, rootItem);
		var parent = item.parent;

        var obj = EditorUtility.InstanceIDToObject(id) as ScriptableObject;
        var idx = _configIdxById[id];
        config.Configs[idx].Files.Remove(obj);
        Selection.activeGameObject = null;
        AssetDatabase.DeleteAsset(assetPath);
        Save();
        Reload();
		var selected = new List<int> { parent.id };
		//SetExpanded(selected);//parent.id, true);
		SetSelection(selected, TreeViewSelectionOptions.RevealAndFrame);
	}
	
    public void Save()
    {
        AssetDatabase.SaveAssets();
    }

	public void Duplicate()
	{
		if (state.selectedIDs.Count == 0)
			return;

        // Duplicate only the first if selected more than one:
        var id = state.selectedIDs[0];
        if (Mathf.Abs(id) < config.MaxCategoryId)
            return;

        string assetPath = AssetDatabase.GetAssetPath(id);
        var duplicatePath = AssetDatabase.GenerateUniqueAssetPath(assetPath);
        if (AssetDatabase.CopyAsset(assetPath, duplicatePath))
        {
            Object duplicateObject = AssetDatabase.LoadAssetAtPath(duplicatePath, typeof(Object));
            var idx = _configIdxById[id];
            config.Configs[idx].Files.Add(duplicateObject as ScriptableObject);

            Reload();
            var selected = new List<int> { duplicateObject.GetInstanceID() };
            SetSelection(selected, TreeViewSelectionOptions.RevealAndFrame);

            Save();
        }
        else
        {
            Debug.LogWarning("Error, file could not be duplicated.");
        }
	}

    public void FocusSelected()
    {
        if (state.selectedIDs.Count == 0)
            return;

        // Only use the first if selected more than one:
        var id = state.selectedIDs[0];
        var selected = new List<int> { id };
        SetSelection(selected, TreeViewSelectionOptions.RevealAndFrame);
    }

    public void SortAlphabetically()
    {
        if (state.selectedIDs.Count == 0)
			return;
		
		int id = state.selectedIDs[0];
        var idx = _configIdxById[id];
        config.Configs[idx].Files.Sort((x, y) => string.Compare(x.name, y.name));

        Reload();
        //SetSelection(state.selectedIDs, TreeViewSelectionOptions.RevealAndFrame); 
    }

    public Object[] GetSelectedObjects()
	{
		if (state.selectedIDs.Count == 0)
			return null;
		
		List<Object> selectedObjs = new List<Object>();
		for (int i = 0; i < state.selectedIDs.Count; i++)
		{
			if (Mathf.Abs(state.selectedIDs[i]) < config.MaxCategoryId)
				continue;
			
			Object obj = EditorUtility.InstanceIDToObject(state.selectedIDs[i]);
			selectedObjs.Add(obj);
		}
		
		if (selectedObjs.Count == 0)
			return null;
		
		return selectedObjs.ToArray();
	}
	
	#endregion
}