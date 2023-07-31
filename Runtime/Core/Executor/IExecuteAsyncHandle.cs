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
    public interface IExecuteAsyncHandle : IExecute
    {
        /// <summary>
        /// 异步执行器执行进度
        /// </summary>
        float progress { get; }

        /// <summary>
        /// 获取异步对象
        /// </summary>
        /// <returns></returns>
        IEnumerator GetCoroutine();

        /// <summary>
        /// 订阅执行器完成回调
        /// </summary>
        /// <param name="subscribe">订阅器</param>
        void Subscribe(ISubscribe subscribe);

        /// <summary>
        /// 取消执行
        /// </summary>
        public void Cancel()
        {
            status = Status.Canceled;
        }

        /// <summary>
        /// 暂停执行
        /// </summary>
        public void Paused()
        {
            if (status is not Status.None)
            {
                Engine.Console.Error("The current Execute has not started executing");
                return;
            }

            if (status is Status.Paused)
            {
                Engine.Console.Error("The current execute has been paused");
                return;
            }

            if (status is Status.Canceled || status is Status.Failed || status is Status.Success)
            {
                Engine.Console.Error("Current execute completed");
                return;
            }

            status = Status.Paused;
        }

        /// <summary>
        /// 恢复执行
        /// </summary>
        public void Resume()
        {
            if (status is not Status.Paused)
            {
                Engine.Console.Error("The current execute is not in a paused state, and there is no need to reply to the execution");
                return;
            }

            if (status is Status.Execute)
            {
                Engine.Console.Error("The current execute is already in progress");
                return;
            }

            if (status is Status.Canceled || status is Status.Failed || status is Status.Success)
            {
                Engine.Console.Error("Current execute completed");
                return;
            }

            status = Status.Execute;
        }
    }
}