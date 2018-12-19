using UnityEngine;

public class ValidateMinMaxAttribute : PropertyAttribute
{
    public float min;
    public float max;

    public ValidateMinMaxAttribute(float min, float max) 
    { 
        this.min = min;
        this.max = max;
    }
}
