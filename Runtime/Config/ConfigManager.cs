using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ZGame.Config
{
    public class ConfigManager : GameManager
    {
        private Dictionary<Type, ConfigBase> cfgDict;

        public override void OnAwake(params object[] args)
        {
            cfgDict = new Dictionary<Type, ConfigBase>();
        }

        /// <summary>
        /// 加载配置表
        /// </summary>
        /// <param name="path"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T LoadConfig<T>(string path) where T : ConfigBase
        {
            if (cfgDict.TryGetValue(typeof(T), out var cfg))
            {
                return (T)cfg;
            }

            var resObject = AppCore.Resource.LoadAsset<T>(path);
            if (resObject is null || resObject.IsSuccess() is false)
            {
                return default;
            }

            cfg = resObject.GetAsset<T>();
            if (cfg is LanguageConfig languageConfig)
            {
                AppCore.Language.SetData(languageConfig);
            }

            return (T)cfg;
        }

        /// <summary>
        /// 加载配置表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T LoadConfig<T>() where T : ConfigBase
        {
            if (cfgDict.TryGetValue(typeof(T), out var cfg))
            {
                return (T)cfg;
            }

            var refPath = typeof(T).GetCustomAttribute<RefPath>();
            if (refPath is null)
            {
                AppCore.Logger.LogError("RefPath is null");
                return default;
            }

            return LoadConfig<T>(refPath.path);
        }

        /// <summary>
        /// 获取指定的配置
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetConfig<T>() where T : ConfigBase
        {
            if (cfgDict.TryGetValue(typeof(T), out var cfg))
            {
                return (T)cfg;
            }

            return LoadConfig<T>();
        }

        /// <summary>
        /// 获取配置数量
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public int Count<T>() where T : ConfigBase
        {
            if (cfgDict.TryGetValue(typeof(T), out var cfg))
            {
                return cfg.Config.Count;
            }

            return 0;
        }

        public void Clear()
        {
            foreach (var VARIABLE in cfgDict.Keys)
            {
                RefPath refPath = VARIABLE.GetCustomAttribute<RefPath>();
                if (refPath is null)
                {
                    continue;
                }

                AppCore.Resource.Unload(refPath.path);
            }

            cfgDict.Clear();
        }

        public void Remove<T>() where T : ConfigBase
        {
            Remove((typeof(T)));
        }

        public void Remove(Type type)
        {
            if (typeof(ConfigBase).IsAssignableFrom(type) is false)
            {
                return;
            }

            cfgDict.Remove(type);
            RefPath refPath = type.GetCustomAttribute<RefPath>();
            if (refPath is null)
            {
                return;
            }

            AppCore.Resource.Unload(refPath.path);
        }
    }
}