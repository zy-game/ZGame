using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace ZGame.Config
{
    class CfgMap<T> : IDisposable where T : IDatable
    {
        private List<T> cfgList;

        public void InitConfig()
        {
            Type cfgType = typeof(T);
            MethodInfo method = cfgType.GetMethod("InitConfig", BindingFlags.Static | BindingFlags.NonPublic);
            if (method is null)
            {
                throw new NotImplementedException("InitConfig");
            }

            cfgList = new List<T>();
            IList temp = (IList)method.Invoke(null, new object[0]);
            for (int i = 0; i < temp.Count; i++)
            {
                cfgList.Add((T)temp[i]);
            }
        }

        public int Count()
        {
            if (cfgList is null)
            {
                InitConfig();
            }

            return cfgList.Count;
        }

        public bool Contains(T value)
        {
            if (cfgList is null)
            {
                InitConfig();
            }

            return cfgList.Contains(value);
        }

        public T GetConfig(object key)
        {
            if (cfgList is null)
            {
                InitConfig();
            }

            return cfgList.Find(x => x.Equals(key));
        }

        public T GetConfig(Predicate<T> match)
        {
            if (cfgList is null)
            {
                InitConfig();
            }

            return cfgList.Find(match);
        }

        public T[] All()
        {
            if (cfgList is null)
            {
                InitConfig();
            }

            return cfgList.ToArray();
        }

        public T[] GetRange(int index, int count)
        {
            if (cfgList is null)
            {
                InitConfig();
            }

            return cfgList.GetRange(index, count).ToArray();
        }

        public T[] Where(Predicate<T> match)
        {
            if (cfgList is null)
            {
                InitConfig();
            }

            return cfgList.FindAll(match).ToArray();
        }

        public void Dispose()
        {
            cfgList.ForEach(x => x.Dispose());
            cfgList.Clear();
            GC.SuppressFinalize(this);
        }
    }
}