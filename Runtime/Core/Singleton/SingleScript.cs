using UnityEngine;


public class SingleScript<T> : ScriptableObject where T : ScriptableObject
{
    private static T _instance;
    public static T instance => Initialized();

    static T Initialized()
    {
        if (_instance is null)
        {
            _instance = CreateScriptObject();
        }

        return _instance;
    }

    private static T CreateScriptObject()
    {
        
        return default;
    }

    public void Saved()
    {
    }
}