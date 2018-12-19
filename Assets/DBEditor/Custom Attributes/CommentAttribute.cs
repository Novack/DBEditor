using UnityEngine;

public class CommentAttribute : PropertyAttribute
{
    public readonly string msg;

    public CommentAttribute(string msg) 
    { 
        this.msg = msg;
    }
}
