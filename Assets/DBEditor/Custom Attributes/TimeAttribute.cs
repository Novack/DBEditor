using UnityEngine;

public class TimeAttribute : PropertyAttribute
{
    public readonly bool useMiliseconds;

    public TimeAttribute(bool useMiliseconds = false)
	{
        this.useMiliseconds = useMiliseconds;
    }
}
