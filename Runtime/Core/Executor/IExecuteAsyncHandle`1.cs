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
    public interface IExecuteAsyncHandle<T> : IExecute<T>, IExecuteAsyncHandle
    {
        /// <summary>
        /// 订阅执行器完成回调
        /// </summary>
        /// <param name="subscribe">订阅器</param>
        public void Subscribe(ISubscribe<T> subscribe)
        {
            Subscribe((ISubscribe)subscribe);
        }
    }
}