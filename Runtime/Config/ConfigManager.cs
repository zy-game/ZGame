using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ZGame.Config
{
    public class ConfigManager : GameFrameworkModule
    {
        private Dictionary<Type, IDisposable> cfgDict;

        public override void OnAwake()
        {
            cfgDict = new Dictionary<Type, IDisposable>();
        }

        private CfgMap<T> GetMap<T>() where T : IDatable
        {
            if (cfgDict.TryGetValue(typeof(T), out IDisposable cfg) is false)
            {
                cfgDict.Add(typeof(T), cfg = new CfgMap<T>());
            }

            return (CfgMap<T>)cfg;
        }

        /// <summary>
        /// 获取配置数量
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public int Count<T>() where T : IDatable
        {
            return GetMap<T>().Count();
        }

        /// <summary>
        /// 判断配置项是否存在
        /// </summary>
        /// <param name="value"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool Contains<T>(T value) where T : IDatable
        {
            return GetMap<T>().Contains(value);
        }

        /// <summary>
        /// 获取配置项
        /// </summary>
        /// <param name="value"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetConfig<T>(object value) where T : IDatable
        {
            return GetMap<T>().GetConfig(value);
        }

        /// <summary>
        /// 获取配置项
        /// </summary>
        /// <param name="match"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetConfig<T>(Predicate<T> match) where T : IDatable
        {
            return GetMap<T>().GetConfig(match);
        }

        /// <summary>
        /// 获取所有配置项
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T[] All<T>() where T : IDatable
        {
            return GetMap<T>().All();
        }

        public T[] GetRange<T>(int start, int end) where T : IDatable
        {
            return GetMap<T>().GetRange(start, end);
        }

        /// <summary>
        /// 筛选配置项
        /// </summary>
        /// <param name="match"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T[] Where<T>(Predicate<T> match) where T : IDatable
        {
            return GetMap<T>().Where(match);
        }

        /// <summary>
        /// 通过下标获取配置
        /// </summary>
        /// <param name="index"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetConfigWithIndex<T>(int index) where T : IDatable
        {
            return GetMap<T>().GetRange(index, 1).FirstOrDefault();
        }

        public override void Dispose()
        {
            foreach (var VARIABLE in cfgDict.Values)
            {
                VARIABLE.Dispose();
            }

            cfgDict.Clear();
            GC.SuppressFinalize(this);
        }
    }
}