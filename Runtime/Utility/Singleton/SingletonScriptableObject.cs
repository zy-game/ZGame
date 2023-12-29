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
                    _instance.OnAwake();
                }

                return _instance;
            }
        }

        public abstract void OnAwake();

        protected virtual void OnSaved()
        {
#if UNITY_EDITOR
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

        private static void OnLoad()
        {
            ResourceReference reference = typeof(T).GetCustomAttribute<ResourceReference>();
            if (reference == null)
            {
                _instance = CreateInstance<T>();
                return;
            }

            if (reference.path.StartsWith("Resources"))
            {
                string path = reference.path.Substring(reference.path.IndexOf("/") + 1);
                path = path.Substring(0, path.LastIndexOf("."));
                _instance = Resources.Load<T>(path);
            }
            else
            {
#if UNITY_EDITOR
                _instance = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(reference.path);
#endif
                if (_instance == null && Application.isPlaying)
                {
                    ResHandle handle = ResourceManager.instance.LoadAsset(reference.path);
                    if (handle is not null || handle.EnsureLoadSuccess())
                    {
                        _instance = handle.Get<T>(null);
                    }
                }
            }

            if (_instance != null)
            {
                return;
            }

            if (reference.path.StartsWith("Assets") is false)
            {
                reference.path = "Assets/" + reference.path;
            }

            string folder = Path.GetDirectoryName(reference.path);
            if (Directory.Exists(folder) is false)
            {
                Directory.CreateDirectory(folder);
            }

            _instance = CreateInstance<T>();
#if UNITY_EDITOR
            UnityEditor.AssetDatabase.CreateAsset(_instance, reference.path);
            UnityEditor.EditorUtility.SetDirty(_instance);
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
#endif
        }
    }
}