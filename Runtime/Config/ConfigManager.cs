using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ZGame.Config
{
    public class ConfigManager : GameFrameworkModule
    {
        private Dictionary<Type, ConfigHandler> cfgDict;

        public override void OnAwake(params object[] args)
        {
            cfgDict = new Dictionary<Type, ConfigHandler>();
        }

        private ConfigHandler GetMap<T>() where T : IConfigDatable
        {
            if (cfgDict.TryGetValue(typeof(T), out ConfigHandler cfg) is false)
            {
                cfgDict.Add(typeof(T), cfg = new ConfigHandler());
            }

            return cfg;
        }

        /// <summary>
        /// 获取配置数量
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public int Count<T>() where T : IConfigDatable
        {
            return GetMap<T>().Count<T>();
        }

        /// <summary>
        /// 判断配置项是否存在
        /// </summary>
        /// <param name="value"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool Contains<T>(T value) where T : IConfigDatable
        {
            return GetMap<T>().Contains(value);
        }

        /// <summary>
        /// 获取配置项
        /// </summary>
        /// <param name="value"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetConfig<T>(object value) where T : IConfigDatable
        {
            return GetMap<T>().GetConfig<T>(value);
        }

        /// <summary>
        /// 获取配置项
        /// </summary>
        /// <param name="match"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetConfig<T>(Predicate<T> match) where T : IConfigDatable
        {
            return GetMap<T>().GetConfig(match);
        }

        /// <summary>
        /// 获取所有配置项
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T[] All<T>() where T : IConfigDatable
        {
            return GetMap<T>().All<T>();
        }

        public T[] GetRange<T>(int start, int end) where T : IConfigDatable
        {
            return GetMap<T>().GetRange<T>(start, end);
        }

        /// <summary>
        /// 筛选配置项
        /// </summary>
        /// <param name="match"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T[] Where<T>(Predicate<T> match) where T : IConfigDatable
        {
            return GetMap<T>().Where(match);
        }

        /// <summary>
        /// 通过下标获取配置
        /// </summary>
        /// <param name="index"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetConfigWithIndex<T>(int index) where T : IConfigDatable
        {
            return GetMap<T>().GetRange<T>(index, 1).FirstOrDefault();
        }

        public override void Release()
        {
            foreach (var VARIABLE in cfgDict.Values)
            {
                GameFrameworkFactory.Release(VARIABLE);
            }

            cfgDict.Clear();
        }
    }
}