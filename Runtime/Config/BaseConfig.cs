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

        public static T instance
        {
            get
            {
                if (_config is null)
                {
                    _config = Load();
                }

                return _config;
            }
        }


        private static T Load()
        {
#if UNITY_EDITOR
            string[] files = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            if (files is null || files.Length == 0)
            {
                return default;
            }

            return AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(files[0]));
#endif
            T[] o = Resources.FindObjectsOfTypeAll<T>();
            if (o is null || o.Length == 0)
            {
                return default;
            }

            return Instantiate(o[0]);
        }

        public virtual void OnAwake()
        {
        }
    }
}