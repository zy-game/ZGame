using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ZGame
{
    /// <summary>
    /// 数据表，一个键值对应多个值
    /// </summary>
    /// <typeparam name="TKey">键</typeparam>
    /// <typeparam name="TValue">值</typeparam>
    public sealed class Mapping<TKey, TValue> : IReference, IEnumerable<KeyValuePair<TKey, IEnumerable<TValue>>> where TValue : IReference
    {
        private Dictionary<TKey, List<TValue>> mapping = new Dictionary<TKey, List<TValue>>();
        public int Count => mapping.Count;

        public void Release()
        {
            foreach (var list in mapping.Values)
            {
                foreach (var value in list)
                {
                    RefPooled.Free(value);
                }
            }

            mapping.Clear();
        }

        /// <summary>
        /// 添加一个值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Add(TKey key, TValue value)
        {
            if (mapping.TryGetValue(key, out List<TValue> list) is false)
            {
                mapping.Add(key, list = new List<TValue>());
            }

            list.Add(value);
        }

        /// <summary>
        /// 添加多个值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="values"></param>
        public void Add(TKey key, IEnumerable<TValue> values)
        {
            if (mapping.TryGetValue(key, out List<TValue> list) is false)
            {
                mapping.Add(key, list = new List<TValue>());
            }

            list.AddRange(values);
        }

        /// <summary>
        /// 尝试获取值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public bool TryGetValue(TKey key, out IEnumerable<TValue> values)
        {
            if (mapping.TryGetValue(key, out List<TValue> list))
            {
                values = list;
                return true;
            }
            else
            {
                values = null;
                return false;
            }
        }

        /// <summary>
        /// 移除所有值
        /// </summary>
        /// <param name="key"></param>
        public void RemoveAll(TKey key)
        {
            if (mapping.TryGetValue(key, out List<TValue> list))
            {
                list.Clear();
            }
        }

        /// <summary>
        /// 移除所有值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="comparer"></param>
        public void RemoveAll(TKey key, Predicate<TValue> comparer)
        {
            if (mapping.TryGetValue(key, out List<TValue> list))
            {
                list.RemoveAll(comparer);
            }
        }

        public IEnumerator<KeyValuePair<TKey, IEnumerable<TValue>>> GetEnumerator()
        {
            return new Enumerator(mapping);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        class Enumerator : IEnumerator<KeyValuePair<TKey, IEnumerable<TValue>>>
        {
            private int index = -1;
            private Dictionary<TKey, List<TValue>> mapping;

            public Enumerator(Dictionary<TKey, List<TValue>> mapping)
            {
                this.mapping = mapping;
            }

            public bool MoveNext()
            {
                if (mapping is null)
                {
                    return false;
                }

                index++;
                return index < mapping.Count;
            }

            private KeyValuePair<TKey, IEnumerable<TValue>> GetValue()
            {
                if (index < 0 || index >= mapping.Count)
                {
                    throw new IndexOutOfRangeException();
                }

                KeyValuePair<TKey, List<TValue>> pair = mapping.ElementAt(index);
                return new KeyValuePair<TKey, IEnumerable<TValue>>(pair.Key, pair.Value);
            }

            public void Reset()
            {
                index = 0;
            }

            public KeyValuePair<TKey, IEnumerable<TValue>> Current => GetValue();

            object IEnumerator.Current => Current;

            public void Dispose()
            {
                mapping = null;
            }
        }
    }
}