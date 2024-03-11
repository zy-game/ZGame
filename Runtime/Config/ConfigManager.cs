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

        public int Count<T>() where T : IDatable
        {
            if (cfgMap.TryGetValue(typeof(T), out List<IDatable> datableTempletes))
            {
                return datableTempletes.Count;
            }

            InitConfig<T>();
            return Count<T>();
        }

        public bool HasConfig<T>(object value) where T : IDatable
        {
            return GetConfig<T>(value) is not null;
        }

        public T GetConfig<T>(object value) where T : IDatable
        {
            if (cfgMap.TryGetValue(typeof(T), out List<IDatable> datableTempletes))
            {
                return (T)datableTempletes.Find(x => x.Equals(value));
            }

            InitConfig<T>();
            return GetConfig<T>(value);
        }

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