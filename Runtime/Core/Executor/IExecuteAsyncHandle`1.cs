using System;
using System.Collections;
using System.Collections.Generic;
using ZEngine.VFS;

namespace ZEngine
{
    /// <summary>
    /// 异步执行器
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IExecuteAsyncHandle<T> : IExecuteHandle<T>, IExecuteAsyncHandle
    {
        /// <summary>
        /// 进度
        /// </summary>
        float progress { get; }

        /// <summary>
        /// 订阅执行器完成回调
        /// </summary>
        /// <param name="subscribe">订阅器</param>
        void Subscribe(ISubscribe<T> subscribe);
    }

    public abstract class ExecuteAsyncHandle<T> : ExecuteAsyncHandle, IExecuteAsyncHandle<T>
    {
        public void Subscribe(ISubscribe<T> subscribe)
        {
            subscribes.Add(subscribe);
        }

        public override void Completion()
        {
            for (int i = 0; i < subscribes.Count; i++)
            {
                try
                {
                    if (subscribes[i] is ISubscribe<T> write)
                    {
                        write.Execute(this);
                    }
                    else
                    {
                        subscribes[i].Execute(this);
                    }
                }
                catch (Exception e)
                {
                    Engine.Console.Error(e);
                }
            }
        }
    }
}