using System;
using System.Collections.Generic;

namespace ZGame.Data
{
    /// <summary>
    /// 运行时游戏数据
    /// </summary>
    public class DatableManager : GameManager
    {
        private List<IDatable> map = new();


        /// <summary>
        /// 是否存在数据
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Contains<T>() where T : IDatable
        {
            return Contains(typeof(T));
        }

        /// <summary>
        /// 是否存在数据
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool Contains(Type type)
        {
            if (typeof(IDatable).IsAssignableFrom(type) is false)
            {
                throw new NotImplementedException(nameof(IDatable));
            }

            return map.Exists(x => x.GetType() == type);
        }

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetDatable<T>() where T : IDatable
        {
            return (T)map.Find(x => x.GetType() == typeof(T));
        }

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public IDatable GetDatable(Type type)
        {
            if (typeof(IDatable).IsAssignableFrom(type) is false)
            {
                throw new NotImplementedException(nameof(IDatable));
            }

            return map.Find(x => x.GetType() == type);
        }

        /// <summary>
        /// 尝试获取指定的数据
        /// </summary>
        /// <param name="data"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool TryGetValue<T>(out T data) where T : IDatable
        {
            data = (T)map.Find(x => x.GetType() == typeof(T));
            return data != null;
        }

        public bool TryGetValue(Type type, out IDatable data)
        {
            if (typeof(IDatable).IsAssignableFrom(type) is false)
            {
                throw new NotImplementedException(nameof(IDatable));
            }

            data = map.Find(x => x.GetType() == type);
            return data != null;
        }

        /// <summary>
        /// 设置数据
        /// </summary>
        /// <param name="data"></param>
        /// <typeparam name="T"></typeparam>
        public void SetDatable(IDatable data)
        {
            if (Contains(data.GetType()))
            {
                map.RemoveAll(x => x.GetType() == data.GetType());
            }

            map.Add(data);
        }

        public void Clear()
        {
            foreach (var VARIABLE in map)
            {
                RefPooled.Free(VARIABLE);
            }

            map.Clear();
        }

        /// <summary>
        /// 清理指定数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void Clear<T>() where T : IDatable
        {
            Clear(typeof(T));
        }

        public void Clear(Type type)
        {
            if (typeof(IDatable).IsAssignableFrom(type) is false)
            {
                throw new NotImplementedException(nameof(IDatable));
            }

            IDatable data = GetDatable(type);
            if (data is null)
            {
                return;
            }

            data.Dispose();
            map.Remove(data);
        }

        public override void Release()
        {
            foreach (var VARIABLE in map)
            {
                VARIABLE.Dispose();
            }

            map.Clear();
            GC.SuppressFinalize(this);
        }
    }
}