using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZEngine.VFS;

namespace ZEngine
{
    /// <summary>
    /// 异步执行器
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IExecuteHandle<T> : IExecuteHandle
    {
        /// <summary>
        /// 订阅执行器完成回调
        /// </summary>
        /// <param name="subscribe">订阅器</param>
        public void Subscribe(ISubscribeHandle<T> subscribe)
        {
            Subscribe((ISubscribeHandle)subscribe);
        }
    }

    /// <summary>
    /// 异步执行器
    /// </summary>
    public interface IExecuteHandle : IReference
    {
        /// <summary>
        /// 执行器状态
        /// </summary>
        Status status { get; }

        /// <summary>
        /// 获取异步对象
        /// </summary>
        /// <returns></returns>
        void Execute(params object[] paramsList);

        /// <summary>
        /// 订阅执行器完成回调
        /// </summary>
        /// <param name="subscribe">订阅器</param>
        void Subscribe(ISubscribeHandle subscribe);

        /// <summary>
        /// 等待完成
        /// </summary>
        /// <returns></returns>
        IEnumerator ExecuteComplete();
    }
}