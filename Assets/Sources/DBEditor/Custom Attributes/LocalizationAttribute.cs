using UnityEngine;


public class LocalizationAttribute : PropertyAttribute
{
    public readonly bool isMultiline;
    public readonly bool isReadOnly;

    public LocalizationAttribute(bool multiline, bool readOnly = false)
    {
        isMultiline = multiline;
        isReadOnly = readOnly;
    }
}