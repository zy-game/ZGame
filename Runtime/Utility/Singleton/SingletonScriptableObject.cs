using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using ZGame.Resource;

namespace ZGame
{
    public abstract class SingletonScriptableObject<T> : ScriptableObject where T : SingletonScriptableObject<T>
    {
        //懒汉单例模式
        private static T _instance;

        public static T instance
        {
            get
            {
                if (_instance != null)
                {
                    return _instance;
                }

                Load();
                if (_instance == null)
                {
                    Create();
                }

                _instance?.OnAwake();
                return _instance;
            }
        }

        private static void Create()
        {
            ResourceReference resourceReference = typeof(T).GetCustomAttribute<ResourceReference>();
            if (resourceReference is null)
            {
                return;
            }
#if UNITY_EDITOR
            if (Application.isPlaying)
            {
                return;
            }

            _instance = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(resourceReference.path);
            if (_instance != null || Application.isPlaying)
            {
                return;
            }

            if (resourceReference.path.StartsWith("Assets") is false)
            {
                resourceReference.path = "Assets/" + resourceReference.path;
            }

            string folder = Path.GetDirectoryName(resourceReference.path);
            if (Directory.Exists(folder) is false)
            {
                Directory.CreateDirectory(folder);
            }

            _instance = CreateInstance<T>();
            UnityEditor.AssetDatabase.CreateAsset(_instance, resourceReference.path);
            UnityEditor.EditorUtility.SetDirty(_instance);
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
#endif
        }

        private static void Load()
        {
            ResourceReference resourceReference = typeof(T).GetCustomAttribute<ResourceReference>();
            if (resourceReference is null)
            {
                return;
            }

            if (resourceReference.path.StartsWith("Resources"))
            {
                string path = resourceReference.path.Substring(resourceReference.path.IndexOf("/") + 1);
                path = path.Substring(0, path.LastIndexOf("."));
                _instance = Resources.Load<T>(path);
                return;
            }
#if UNITY_EDITOR
            if (Application.isPlaying is false)
            {
                _instance = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(resourceReference.path);
                return;
            }
#endif
            using (ResObject resObject = WorkApi.Resource.LoadAsset(resourceReference.path))
            {
                if (resObject is null || resObject.IsSuccess() is false)
                {
                    return;
                }

                _instance = resObject.GetAsset<T>();
            }
        }

        public virtual void OnAwake()
        {
        }

        protected virtual void OnSaved()
        {
#if UNITY_EDITOR
            Debug.Log($"Save {typeof(T)}");
            UnityEditor.EditorUtility.SetDirty(_instance);
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
#endif
        }

        public static void OnSave()
        {
            if (_instance == null)
            {
                return;
            }

            _instance.OnSaved();
        }
    }
}