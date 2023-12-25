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
                if (_instance == null)
                {
                    OnLoad();
                }

                return _instance;
            }
        }

        protected abstract void OnAwake();

        protected virtual void OnSaved()
        {
        }


        public static void OnSave()
        {
            if (_instance == null)
            {
                return;
            }

            _instance.OnSaved();
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(_instance);
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
#endif
        }

        private static void OnLoad()
        {
            ResourceReference reference = typeof(T).GetCustomAttribute<ResourceReference>();
            if (reference == null)
            {
                return;
            }

            if (reference.path.StartsWith("Assets"))
            {
#if UNITY_EDITOR
                if (Application.isPlaying is false)
                {
                    _instance = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(reference.path);
                }
                else
                {
                    ResHandle handle = ResourceManager.instance.LoadAsset(reference.path);
                    if (handle is not null || handle.EnsureLoadSuccess())
                    {
                        _instance = handle.Get<T>(null);
                    }
                }
#endif
            }
            else
            {
                string path = reference.path.Substring(reference.path.IndexOf("/") + 1);
                _instance = Resources.Load<T>(path);
            }

            if (_instance != null)
            {
                return;
            }

            OnCreate(reference.path);
        }


        private static void OnCreate(string path)
        {
            if (path.StartsWith("Assets") is false)
            {
                path = "Assets/" + path;
            }

            string folder = Path.GetDirectoryName(path);
            if (Directory.Exists(folder) is false)
            {
                Directory.CreateDirectory(folder);
            }

            _instance = CreateInstance<T>();
#if UNITY_EDITOR
            UnityEditor.AssetDatabase.CreateAsset(_instance, path);
            UnityEditor.EditorUtility.SetDirty(_instance);
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
#endif
        }
    }
}