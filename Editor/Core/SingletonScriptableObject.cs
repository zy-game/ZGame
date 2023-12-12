using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace ZGame.Editor
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
            EditorUtility.SetDirty(_instance);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void OnLoad()
        {
            ResourceReference reference = typeof(T).GetCustomAttribute<ResourceReference>();
            if (reference.path.StartsWith("Resources"))
            {
                _instance = Resources.Load<T>(reference.path.Substring(reference.path.IndexOf("/")));
                if (_instance != null)
                {
                    return;
                }

                OnCreate("Assets/" + reference.path + ".asset");
                return;
            }

            _instance = AssetDatabase.LoadAssetAtPath<T>(reference.path + ".asset");
            if (_instance != null)
            {
                return;
            }

            OnCreate(reference.path + ".asset");
        }

        private static void OnCreate(string path)
        {
            _instance = CreateInstance<T>();
            AssetDatabase.CreateAsset(_instance, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}