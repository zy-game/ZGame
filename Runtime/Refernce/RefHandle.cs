using System;
using System.Collections.Concurrent;

namespace ZGame
{
    /// <summary>
    /// 引用对象句柄
    /// </summary>
    class RefHandle : IDisposable
    {
        private ConcurrentQueue<IReference> _queue;
        public Type owner { get; }

        public int count => _queue.Count;

        public RefHandle(Type owner)
        {
            this.owner = owner;
            _queue = new();
        }

        /// <summary>
        /// 创建引用对象
        /// </summary>
        /// <returns></returns>
        public IReference Spawner(params object[] args)
        {
            if (_queue.TryDequeue(out IReference result))
            {
                return result;
            }

            if (args is null || args.Length == 0)
            {
                return Activator.CreateInstance(owner) as IReference;
            }

            return Activator.CreateInstance(owner, args) as IReference;
        }

        /// <summary>
        /// 回收引用对象
        /// </summary>
        /// <param name="referenceObject"></param>
        public void Release(IReference referenceObject)
        {
            referenceObject.Release();
            GC.SuppressFinalize(referenceObject);
            _queue.Enqueue(referenceObject);
        }

        /// <summary>
        /// 释放引用对象句柄
        /// </summary>
        public void Dispose()
        {
            _queue.Clear();
        }
    }
}