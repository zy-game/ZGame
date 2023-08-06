using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using ZEngine.Resource;
using Object = UnityEngine.Object;


public class SingleScript<T> : ScriptableObject where T : ScriptableObject
{
    private static T _instance;

    public static T instance
    {
        get
        {
            if (_instance is null)
            {
                CreateScriptObject();
            }

            return _instance;
        }
    }

    private static void CreateScriptObject()
    {
        ConfigOptions options = typeof(T).GetCustomAttribute<ConfigOptions>();
        if (options is null)
        {
            throw new ArgumentNullException("options");
        }


        switch (options.localtion)
        {
            case ConfigOptions.Localtion.Internal:
            case ConfigOptions.Localtion.Project:
#if UNITY_EDITOR
                if (options.path.IsNullOrEmpty())
                {
                    options.path = $"ProjectSettings/{typeof(T).Name}.asset";
                }

                _instance = UnityEditorInternal.InternalEditorUtility.LoadSerializedFileAndForget(options.path).FirstOrDefault() as T;
                if (_instance is null)
                {
                    _instance = CreateInstance<T>();
                    _instance.name = nameof(T);
                }
#else
                _instance = Resources.Load<T>(Path.GetFileNameWithoutExtension(options.path));
#endif
                break;
            case ConfigOptions.Localtion.Packaged:
                if (options.path.EndsWith("asset"))
                {
                    IAssetRequestExecute<T> execute = Engine.Resource.LoadAsset<T>(options.path);
                    _instance = execute.result;
                }
                else if (options.path.EndsWith("json"))
                {
                    IAssetRequestExecute<TextAsset> execute = Engine.Resource.LoadAsset<TextAsset>(options.path);
                    _instance = Engine.Json.Parse<T>(execute.result.text);
                }

                break;
        }
    }

    public void Saved()
    {
        ConfigOptions options = typeof(T).GetCustomAttribute<ConfigOptions>();
        if (options is null)
        {
            throw new NullReferenceException(nameof(ConfigOptions));
        }

        if (_instance is null)
        {
            return;
        }

        switch (options.localtion)
        {
            case ConfigOptions.Localtion.Project:
            case ConfigOptions.Localtion.Internal:
#if UNITY_EDITOR
                if (options.path.IsNullOrEmpty())
                {
                    options.path = $"ProjectSettings/{typeof(T).Name}.asset";
                }

                UnityEditorInternal.InternalEditorUtility.SaveToSerializedFileAndForget(new Object[1] { _instance }, options.path, true);
#endif
                break;
            case ConfigOptions.Localtion.Packaged:
                if (options.path.EndsWith("asset"))
                {
#if UNITY_EDITOR
                    UnityEditor.EditorUtility.SetDirty(_instance);
                    break;
#endif
                }

                File.WriteAllText(options.path, Engine.Json.ToJson(_instance));
                break;
        }
    }
}