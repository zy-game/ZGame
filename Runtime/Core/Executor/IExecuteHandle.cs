using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZEngine.VFS;

namespace ZEngine
{
    /// <summary>
    /// 异步执行器
    /// </summary>
    public interface IExecuteHandle : IReference
    {
        /// <summary>
        /// 异步执行器执行进度
        /// </summary>
        float progress { get; }

        /// <summary>
        /// 获取异步对象
        /// </summary>
        /// <returns></returns>
        IEnumerator Execute(params object[] paramsList);

        /// <summary>
        /// 订阅执行器完成回调
        /// </summary>
        /// <param name="subscribe">订阅器</param>
        void Subscribe(ISubscribe subscribe);
    }
}