using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(CommentAttribute))]
public class CommentDrawer : DecoratorDrawer
{
    private CommentAttribute commentAttribute { get { return (CommentAttribute) attribute; } }

    public override float GetHeight()
    {
        var content = new GUIContent(commentAttribute.msg);
        var style = GUI.skin.GetStyle("helpbox");
        var height = style.CalcHeight(content, EditorGUIUtility.currentViewWidth);
        height += 16f; 
            
        return height;
    }

    public override void OnGUI(Rect position)
    {
        EditorGUI.HelpBox(position, commentAttribute.msg, MessageType.Info);
    }
}

