using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ZGame.Config
{
    class ConfigHandler : IReference
    {
        private List<IConfigDatable> cfgList;

        public void InitConfig<T>()
        {
            Type cfgType = typeof(T);
            MethodInfo method = cfgType.GetMethod("InitConfig", BindingFlags.Static | BindingFlags.NonPublic);
            if (method is null)
            {
                throw new NotImplementedException("InitConfig");
            }

            cfgList = new List<IConfigDatable>();
            IList temp = (IList)method.Invoke(null, new object[0]);
            for (int i = 0; i < temp.Count; i++)
            {
                cfgList.Add((IConfigDatable)temp[i]);
            }
        }

        public int Count<T>() where T : IConfigDatable
        {
            if (cfgList is null)
            {
                InitConfig<T>();
            }

            return cfgList.Count;
        }

        public bool Contains<T>(T value) where T : IConfigDatable
        {
            if (cfgList is null)
            {
                InitConfig<T>();
            }

            return cfgList.Contains(value);
        }

        public T GetConfig<T>(object key) where T : IConfigDatable
        {
            if (cfgList is null)
            {
                InitConfig<T>();
            }

            return (T)cfgList.Find(x => x.Equals(key));
        }

        public T GetConfig<T>(Predicate<T> match) where T : IConfigDatable
        {
            if (cfgList is null)
            {
                InitConfig<T>();
            }

            return (T)cfgList.Find(x => match((T)x));
        }

        public T[] All<T>() where T : IConfigDatable
        {
            if (cfgList is null)
            {
                InitConfig<T>();
            }

            return cfgList.Cast<T>().ToArray();
        }

        public T[] GetRange<T>(int index, int count) where T : IConfigDatable
        {
            if (cfgList is null)
            {
                InitConfig<T>();
            }

            return cfgList.GetRange(index, count).Cast<T>().ToArray();
        }

        public T[] Where<T>(Predicate<T> match) where T : IConfigDatable
        {
            if (cfgList is null)
            {
                InitConfig<T>();
            }

            List<T> result = new List<T>();
            foreach (var VARIABLE in cfgList)
            {
                if (match((T)VARIABLE))
                {
                    result.Add((T)VARIABLE);
                }
            }

            return result.ToArray();
        }

        public void Release()
        {
            cfgList.ForEach(x => x.Dispose());
            cfgList.Clear();
        }
    }
}