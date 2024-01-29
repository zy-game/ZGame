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
                    ResourceReference reference = typeof(T).GetCustomAttribute<ResourceReference>();
                    if (reference == null)
                    {
                        _instance = CreateInstance<T>();
                    }
                    else
                    {
                        OnLoad(reference.path);
                    }

                    _instance?.OnAwake();
                }

                return _instance;
            }
        }

        public abstract void OnAwake();

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

        public static void OnLoad(string path)
        {
            if (path.StartsWith("Resources"))
            {
                path = path.Substring(path.IndexOf("/") + 1);
                path = path.Substring(0, path.LastIndexOf("."));
                _instance = Resources.Load<T>(path);
                return;
            }
#if UNITY_EDITOR
            if (BasicConfig.instance.resMode == ResourceMode.Editor || Application.isPlaying is false)
            {
                _instance = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
                if (_instance != null || Application.isPlaying)
                {
                    return;
                }

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
                UnityEditor.AssetDatabase.CreateAsset(_instance, path);
                UnityEditor.EditorUtility.SetDirty(_instance);
                UnityEditor.AssetDatabase.SaveAssets();
                UnityEditor.AssetDatabase.Refresh();
                return;
            }
#endif
            using (ResObject resObject = ResourceManager.instance.LoadAsset(path))
            {
                if (resObject is null || resObject.IsSuccess() is false)
                {
                    return;
                }

                _instance = resObject.GetAsset<T>();
            }
        }
    }
}