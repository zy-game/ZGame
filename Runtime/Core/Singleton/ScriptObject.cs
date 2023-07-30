using UnityEngine;


public class ScriptObject<T> : ScriptableObject where T : ScriptableObject
{
    private static T _instance;
    public static T instance => Initialized();

    static T Initialized()
    {
        if (_instance is not null)
        {
            return _instance;
        }

        return default;
    }

    public void Saved()
    {
    }
}

public static class ScriptableObjectExtension
{
    public static void Saveed<T>(this ScriptObject<T> single) where T : ScriptableObject
    {
        single.Saved();
    }
}