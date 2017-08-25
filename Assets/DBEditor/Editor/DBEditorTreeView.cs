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
    public Dictionary<int, int> _configIdxById;

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

            for (int j = 0; j < config.Configs[i].Files.Count; j++)
            {
                if (config.Configs[i].Files[j] == null)
                {
                    UnityEngine.Debug.LogWarningFormat("Missing element expected in config item {0} ({1}), at index position {2}.", i, config.Configs[i].ClassName, j);
                    continue;
                }
                var id = config.Configs[i].Files[j].GetInstanceID();
                var item = new TreeViewItem(id);
                if (_configIdxById.ContainsKey(id))
                {
                    UnityEngine.Debug.LogWarningFormat("Element {0} with id {1} already exist in the database. Ignoring add at category {2}.", config.Configs[i].Files[j].name, id, config.Configs[i].TreeViewPath);
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
			if (args.draggedItem.id < config.MaxCategoryId)
				return false;
		}
		
		if (args.draggedItemIDs != null && args.draggedItemIDs.Count > 0)
		{
			for (int i = 0; i < args.draggedItemIDs.Count; i++)
			{
				if (args.draggedItemIDs[i] < config.MaxCategoryId)
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
		if (item.id < config.MaxCategoryId)
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
		if (item.id > config.MaxCategoryId)
			return false;

        if (IsExpanded(item.id))
            item.icon = _folderEmptyIcon;
        else
            item.icon = _folderIcon;
		
		return true;
	}
	
	#endregion
	
	#region Custom Methods
	
    public void CreateNewElement(object configIndex)
    {
        UnityEngine.Debug.LogFormat("Create new element of type {0}", config.Configs[(int)configIndex].ClassName);
    }

    public List<string> GetElementTypes()
    {
        var elements = new List<string>();
        for (int i = 0; i < config.Configs.Count; i++)
        {
            elements.Add(config.Configs[i].ClassName);
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
		if (id < config.MaxCategoryId)
			return;
		
		var item = FindItem(id, rootItem);
		var parent = item.parent;

        var obj = EditorUtility.InstanceIDToObject(id) as ScriptableObject;
        var idx = _configIdxById[id];
        config.Configs[idx].Files.Remove(obj);
		string assetPath = AssetDatabase.GetAssetPath(id);
		AssetDatabase.DeleteAsset(assetPath);
		
		Reload();
		var selected = new List<int> { parent.id };
		//SetExpanded(selected);//parent.id, true);
		SetSelection(selected, TreeViewSelectionOptions.RevealAndFrame);
	}
	
	public void Duplicate()
	{
		if (state.selectedIDs.Count == 0)
			return;
		
		if (state.selectedIDs[0] < config.MaxCategoryId)
			return;

        // Duplicate only the first if sleected more than one:
        var id = state.selectedIDs[0];
        if (id < config.MaxCategoryId)
            return;

        string assetPath = AssetDatabase.GetAssetPath(id);
        // TODO: check for duplicate names.
        var duplicatePath = assetPath.Replace(".asset", "_copy.asset");
        if (AssetDatabase.CopyAsset(assetPath, duplicatePath))
        {
            Object duplicateObject = AssetDatabase.LoadAssetAtPath(duplicatePath, typeof(Object));
            var idx = _configIdxById[id];
            config.Configs[idx].Files.Add(duplicateObject as ScriptableObject);

            Reload();
            var selected = new List<int> { duplicateObject.GetInstanceID() };
            SetSelection(selected, TreeViewSelectionOptions.RevealAndFrame);
        }
        else
        {
            UnityEngine.Debug.LogWarning("Error, file could not be duplicated.");
        }
	}
	
	public Object[] GetSelectedObjects()
	{
		if (state.selectedIDs.Count == 0)
			return null;
		
		List<Object> selectedObjs = new List<Object>();
		for (int i = 0; i < state.selectedIDs.Count; i++)
		{
			if (state.selectedIDs[i] < config.MaxCategoryId)
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