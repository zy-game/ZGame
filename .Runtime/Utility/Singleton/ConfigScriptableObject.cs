﻿using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using ZEngine.Resource;
using Object = UnityEngine.Object;

namespace ZEngine
{
    public class ConfigScriptableObject<T> : ScriptableObject where T : ScriptableObject
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
            ConfigAttribute options = typeof(T).GetCustomAttribute<ConfigAttribute>();
            if (options is null)
            {
                throw new ArgumentNullException("options");
            }


            switch (options.localtion)
            {
                case Localtion.Internal:
                case Localtion.Project:
#if UNITY_EDITOR
                    if (options.path.IsNullOrEmpty())
                    {
                        options.path = $"UserSettings/{typeof(T).Name}.asset";
                    }

                    foreach (var VARIABLE in UnityEditorInternal.InternalEditorUtility.LoadSerializedFileAndForget(options.path))
                    {
                        if (VARIABLE is not null)
                        {
                            _instance = VARIABLE as T;
                        }
                    }

                    if (_instance is null)
                    {
                        _instance = CreateInstance<T>();
                        _instance.name = nameof(T);
                    }
#else
                _instance = Resources.Load<T>(Path.GetFileNameWithoutExtension(options.path));
#endif
                    break;
                case Localtion.Packaged:
                    if (options.path.EndsWith("asset"))
                    {
#if UNITY_EDITOR
                        _instance = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(options.path);
                        if (_instance == null)
                        {
                            UnityEditor.AssetDatabase.CreateAsset(_instance = CreateInstance<T>(), options.path);
                        }

                        return;
#endif
                        IRequestResourceObjectResult requestResourceObjectResult = ZGame.Resource.LoadAsset(options.path);
                        _instance = requestResourceObjectResult.GetObject<T>();
                    }
                    else if (options.path.EndsWith("json"))
                    {
                        if (File.Exists(options.path) is false)
                        {
                            using (File.Create(options.path)) ;
                        }
#if UNITY_EDITOR
                        string json = File.ReadAllText(options.path);
                        if (json.IsNullOrEmpty() is true)
                        {
                            json = "{}";
                        }

                        _instance = ZGame.Json.Parse<T>(json);
                        return;
#endif
                        IRequestResourceObjectResult result = ZGame.Resource.LoadAsset(options.path);
                        _instance = ZGame.Json.Parse<T>(result.GetObject<TextAsset>().text);
                    }

                    break;
            }
        }

        public void Saved()
        {
            ConfigAttribute options = typeof(T).GetCustomAttribute<ConfigAttribute>();
            if (options is null)
            {
                throw new NullReferenceException(nameof(ConfigAttribute));
            }

            if (_instance == null)
            {
                return;
            }

            switch (options.localtion)
            {
                case Localtion.Project:
                case Localtion.Internal:
#if UNITY_EDITOR
                    if (options.path.IsNullOrEmpty())
                    {
                        options.path = $"UserSettings/{typeof(T).Name}.asset";
                    }

                    UnityEditorInternal.InternalEditorUtility.SaveToSerializedFileAndForget(new Object[] { _instance }, options.path, true);

#endif
                    break;
                case Localtion.Packaged:
                    if (options.path.EndsWith("asset"))
                    {
#if UNITY_EDITOR
                        UnityEditor.EditorUtility.SetDirty(_instance);
                        break;
#endif
                    }

                    File.WriteAllText(options.path, ZGame.Json.ToJson(_instance));
                    break;
            }

#if UNITY_EDITOR
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
#endif
        }
    }
}