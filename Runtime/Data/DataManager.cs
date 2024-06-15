using System;
using System.Collections.Generic;
using System.Linq;

namespace ZGame.Data
{
    /// <summary>
    /// 数据缓存管理器
    /// </summary>
    public class DataManager : GameManager
    {
        private List<IDatable> _datables;

        public override void OnAwake(params object[] args)
        {
            _datables = new List<IDatable>();
        }

        /// <summary>
        /// 获取所有指定类型的数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T[] GetDatas<T>() where T : IDatable
        {
            if (_datables == null || _datables.Count == 0)
                return Array.Empty<T>();

            List<T> result = new List<T>();
            foreach (var item in _datables)
            {
                if (item is T)
                {
                    result.Add((T)item);
                }
            }

            return result.ToArray();
        }

        /// <summary>
        /// 获取指定ID的数据
        /// </summary>
        /// <param name="id"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetData<T>(uint id) where T : IDatable
        {
            return (T)_datables.Find(x => x.id == id);
        }

        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="data"></param>
        public void AddData(IDatable data)
        {
            _datables.Add(data);
        }

        /// <summary>
        /// 移除数据
        /// </summary>
        /// <param name="data"></param>
        public void RemoveData(IDatable data)
        {
            _datables.Remove(data);
            RefPooled.Free(data);
        }

        /// <summary>
        /// 移除数据
        /// </summary>
        /// <param name="id"></param>
        public void RemoveData(uint id)
        {
            IDatable data = _datables.Find(x => x.id == id);
            if (data is null)
            {
                return;
            }

            RemoveData(data);
        }

        public void Clear()
        {
            _datables.ForEach(x => RefPooled.Free(x));
            _datables.Clear();
        }
    }
}