using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ZGame
{
    public class BaseConfig<T> : ScriptableObject where T : BaseConfig<T>
    {
        private static T _config;
        private static string _path;

        public static T instance
        {
            get
            {
                if (_config is null)
                {
                    _config = Load();
                    _config.OnAwake();
                }

                return _config;
            }
        }


        private static T Load()
        {
            T result = default;
            RefPath refPath = typeof(T).GetCustomAttribute<RefPath>();
            if (refPath is null)
            {
                _path = "Assets/Resources/" + typeof(T).Name + ".asset";
                result = Resources.Load<T>(typeof(T).Name);
            }
            else
            {
#if UNITY_EDITOR
                _path = refPath.path;
                result = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(refPath.path);
#endif
            }

            if (result is null)
            {
                Debug.Log("创建新的配置文件:" + typeof(T).Name);
                result = Activator.CreateInstance<T>();
#if UNITY_EDITOR
                GameFileSystemHelper.TryCreateDirectory(_path);
                UnityEditor.AssetDatabase.CreateAsset(result, _path);
                UnityEditor.AssetDatabase.Refresh();
#endif
            }


            return result;
        }

        public static void Save()
        {
            Debug.Log(_config.GetType().FullName);
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(_config);
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
#endif
        }

        public virtual void OnAwake()
        {
        }
    }
}