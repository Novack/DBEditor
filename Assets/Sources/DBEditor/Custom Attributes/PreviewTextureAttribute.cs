using UnityEngine;

public enum FieldType
{
    Texture = 0,
    Sprite = 1,
}

public class PreviewTextureAttribute : PropertyAttribute
{
    public float size = -1;
    public FieldType fieldType;


    public PreviewTextureAttribute(int size, FieldType fieldType = FieldType.Sprite)
    {
        this.size = size;
        this.fieldType = fieldType;
    }
}
