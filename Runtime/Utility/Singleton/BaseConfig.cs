using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
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
            ResourceReference resourceReference = typeof(T).GetCustomAttribute<ResourceReference>();
            if (resourceReference is null)
            {
                _path = "Assets/Resources/" + typeof(T).Name + ".asset";
                result = Resources.Load<T>(typeof(T).Name);
            }
            else
            {
#if UNITY_EDITOR
                _path = resourceReference.path;
                result = AssetDatabase.LoadAssetAtPath<T>(resourceReference.path);
#endif
            }

            if (result is null)
            {
                Debug.Log("创建新的配置文件:" + typeof(T).Name);
                result = Activator.CreateInstance<T>();
                PathHelper.TryExistsDirectory(_path);
                AssetDatabase.CreateAsset(result, _path);
                AssetDatabase.Refresh();
            }


            return result;
        }

        public static void Save()
        {
            Debug.Log(_config.GetType().FullName);
            EditorUtility.SetDirty(_config);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public virtual void OnAwake()
        {
        }
    }
}