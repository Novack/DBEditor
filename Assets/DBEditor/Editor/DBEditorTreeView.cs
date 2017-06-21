using UnityEditor.IMGUI.Controls;
using System.Collections.Generic;
using DBEditor;
using UnityEditor;
using UnityEngine;

class DBEditorTreeView : TreeView
{
	private DBEditorConfig config;

	public DBEditorTreeView(TreeViewState treeViewState, DBEditorConfig configParam) : base(treeViewState)
	{
		config = configParam;
		showBorder = true;
		Reload();
	}
	
	protected override TreeViewItem BuildRoot()
	{
		var root = new TreeViewItem      { id = 0, depth = -1, displayName = "Root" };
		
		var pepe = new TreeViewItem(10);
		pepe.displayName = "pepito";
		pepe.icon = EditorGUIUtility.FindTexture("Folder Icon"); //("FolderEmpty Icon");

		root.AddChild(pepe);	
		
		for (int i = 0; i < config.files.Count; i++)
		{
			var item = new TreeViewItem(config.files[i].GetInstanceID());// 100+i);
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
			if (args.draggedItem.id < 100)
				return false;
		}
		
		if (args.draggedItemIDs != null && args.draggedItemIDs.Count > 0)
		{
			for (int i = 0; i < args.draggedItemIDs.Count; i++)
			{
				if (args.draggedItemIDs[i] < 100)
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
		if (item.id < 100)
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
	
	public void StartRename()
	{
		if (state.selectedIDs.Count == 0)
			return;
		
		var item = FindItem(state.selectedIDs[0], rootItem);
		BeginRename(item);
	}
	
	public Object[] GetSelectedObjects()
	{
		if (state.selectedIDs.Count == 0)
			return null;
		
		List<Object> selectedObjs = new List<Object>();
		for (int i = 0; i < state.selectedIDs.Count; i++)
		{
			if (state.selectedIDs[i] < 100)
				continue;
			
			Object obj = EditorUtility.InstanceIDToObject(state.selectedIDs[i]);
			selectedObjs.Add(obj);
		}
		
		if (selectedObjs.Count == 0)
			return null;
		
		return selectedObjs.ToArray();
	}
}