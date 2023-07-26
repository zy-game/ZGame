using UnityEngine;


public class SingleScriptObject<T> : ScriptableObject where T : ScriptableObject
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