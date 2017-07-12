using UnityEngine;

public class LocalizationAttribute : PropertyAttribute
{
    public readonly bool isMultiline;

    public LocalizationAttribute(bool multiline)
    {
        isMultiline = multiline;
    }
}
