using UnityEditor.IMGUI.Controls;
using System.Collections.Generic;
using DBEditor;
using UnityEditor;
using UnityEngine;

class DBEditorTreeView : TreeView
{
	private DBEditorConfig config;
	private int _currentCatId;

	public DBEditorTreeView(TreeViewState treeViewState, DBEditorConfig configParam) : base(treeViewState)
	{
		config = configParam;
		showBorder = true;
		Reload();
	}
	
	protected override TreeViewItem BuildRoot()
	{
		var root = new TreeViewItem { id = 0, depth = -1, displayName = "Root" };
		
		_currentCatId = 1;
		
		var pepe = new TreeViewItem(_currentCatId);
		pepe.displayName = "pepito";
		pepe.icon = EditorGUIUtility.FindTexture("Folder Icon");
		_currentCatId++;

		root.AddChild(pepe);	
		
		for (int i = 0; i < config.files.Count; i++)
		{
			var item = new TreeViewItem(config.files[i].GetInstanceID());
			item.displayName = config.files[i].name;
			item.icon = EditorGUIUtility.FindTexture("ScriptableObject Icon");
			//item.icon = AssetPreview.GetMiniThumbnail(config.files[i]);
			pepe.AddChild(item);
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
		DragAndDrop.PrepareStartDrag();
		DragAndDrop.StartDrag("DBEditorDrag");
		DragAndDrop.objectReferences = GetSelectedObjects();
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
			item.icon = EditorGUIUtility.FindTexture("FolderEmpty Icon");
		else
			item.icon = EditorGUIUtility.FindTexture("Folder Icon");
		
		return true;
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
		config.files.Remove(obj);
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
		
		// Only duplicating first of the selection list.
		if (state.selectedIDs[0] < config.MaxCategoryId)
			return;
		
		string assetPath = AssetDatabase.GetAssetPath(state.selectedIDs[0]);
		var duplicatePath = assetPath.Replace(".asset", "_copy.asset");
		AssetDatabase.CopyAsset(assetPath, duplicatePath);
		Object duplicateObject = AssetDatabase.LoadAssetAtPath(duplicatePath, typeof(Object));
		config.files.Add(duplicateObject as ScriptableObject);
		Reload();
		var selected = new List<int> { duplicateObject.GetInstanceID() };
		SetSelection(selected, TreeViewSelectionOptions.RevealAndFrame);
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
}