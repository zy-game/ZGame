using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using ZGame.Module;

namespace ZGame.Config
{
    public class ConfigManager : IModule
    {
        private Dictionary<Type, List<IDatable>> cfgMap;

        public void OnAwake()
        {
            cfgMap = new Dictionary<Type, List<IDatable>>();
        }

        private void InitConfig<T>()
        {
            Type cfgType = typeof(T);
            MethodInfo method = cfgType.GetMethod("InitConfig", BindingFlags.Static | BindingFlags.NonPublic);
            if (method is null)
            {
                throw new NotImplementedException("InitConfig");
            }

            IList cfgList = (IList)method.Invoke(null, new object[0]);
            List<IDatable> templetes = new List<IDatable>(cfgList.Count);
            for (int i = 0; i < cfgList.Count; i++)
            {
                templetes.Add((IDatable)cfgList[i]);
            }

            cfgMap.Add(typeof(T), templetes);
        }

        /// <summary>
        /// 获取配置数量
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public int Count<T>() where T : IDatable
        {
            if (cfgMap.TryGetValue(typeof(T), out List<IDatable> datableTempletes))
            {
                return datableTempletes.Count;
            }

            InitConfig<T>();
            return Count<T>();
        }

        /// <summary>
        /// 判断配置项是否存在
        /// </summary>
        /// <param name="value"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool Contains<T>(object value) where T : IDatable
        {
            return GetConfig<T>(value) is not null;
        }

        /// <summary>
        /// 获取配置项
        /// </summary>
        /// <param name="value"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetConfig<T>(object value) where T : IDatable
        {
            if (cfgMap.TryGetValue(typeof(T), out List<IDatable> datableTempletes))
            {
                return (T)datableTempletes.Find(x => x.Equals(value));
            }

            InitConfig<T>();
            return GetConfig<T>(value);
        }

        /// <summary>
        /// 获取配置项
        /// </summary>
        /// <param name="match"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetConfig<T>(Predicate<T> match) where T : IDatable
        {
            if (cfgMap.TryGetValue(typeof(T), out List<IDatable> datables))
            {
                return (T)datables.Find(x => match((T)x));
            }

            InitConfig<T>();
            return GetConfig<T>(match);
        }

        /// <summary>
        /// 获取所有配置项
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public List<T> All<T>() where T : IDatable
        {
            List<T> templeteList = new List<T>();
            if (cfgMap.TryGetValue(typeof(T), out List<IDatable> datableTempletes))
            {
                foreach (var VARIABLE in datableTempletes)
                {
                    templeteList.Add((T)VARIABLE);
                }

                return templeteList;
            }

            InitConfig<T>();
            return All<T>();
        }

        /// <summary>
        /// 筛选配置项
        /// </summary>
        /// <param name="match"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public List<T> Where<T>(Predicate<T> match) where T : IDatable
        {
            List<T> templeteList = new List<T>();
            if (cfgMap.TryGetValue(typeof(T), out List<IDatable> datableTempletes) is false)
            {
                foreach (var VARIABLE in datableTempletes)
                {
                    if (match((T)VARIABLE))
                    {
                        templeteList.Add((T)VARIABLE);
                    }
                }

                return templeteList;
            }

            InitConfig<T>();
            return Where<T>(match);
        }

        /// <summary>
        /// 通过下标获取配置
        /// </summary>
        /// <param name="index"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetConfigWithIndex<T>(int index) where T : IDatable
        {
            if (cfgMap.TryGetValue(typeof(T), out List<IDatable> datableTempletes))
            {
                return (T)datableTempletes[index];
            }

            InitConfig<T>();
            return GetConfigWithIndex<T>(index);
        }

        public void Dispose()
        {
            foreach (var VARIABLE in cfgMap.Values)
            {
                VARIABLE.ForEach(x => x.Dispose());
                VARIABLE.Clear();
            }

            cfgMap.Clear();
            GC.SuppressFinalize(this);
        }
    }
}