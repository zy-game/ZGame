using System;
using System.Reflection;
using UnityEngine;
using ZGame.Resource;

namespace ZGame.Config
{
    /// <summary>
    /// 单例配置表
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class Singleton<T> : ConfigBase where T : Singleton<T>
    {
        private static Lazy<T> _instance = new Lazy<T>(LoadConfig);

        public static T instance => _instance.Value;

        private static T LoadConfig()
        {
            Type type = typeof(T);
#if UNITY_EDITOR
            string[] guidList = UnityEditor.AssetDatabase.FindAssets($"t:{type.Name}");
            if (guidList is null || guidList.Length == 0)
            {
                return default;
            }

            return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(UnityEditor.AssetDatabase.GUIDToAssetPath(guidList[0]));
#endif

            RefPath refPath = type.GetCustomAttribute<RefPath>();
            if (refPath == null)
            {
                Debug.LogError($"{type.Name} 没有配置RefPath");
                return null;
            }

            ResObject resObject = AppCore.Resource.LoadAsset<T>(refPath.path);
            if (resObject is null || resObject.IsSuccess() is false)
            {
                return default;
            }

            return resObject.GetAsset<T>();
        }
    }
}