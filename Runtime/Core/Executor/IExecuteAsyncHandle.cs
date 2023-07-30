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
    public interface IExecuteAsyncHandle : IExecuteHandle
    {
        /// <summary>
        /// 异步执行器执行进度
        /// </summary>
        float progress { get; }

        /// <summary>
        /// 暂停执行
        /// </summary>
        void Paused();

        /// <summary>
        /// 恢复执行
        /// </summary>
        void Resume();

        /// <summary>
        /// 取消执行
        /// </summary>
        void Cancel();

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
    }

    public abstract class ExecuteAsyncHandle : IExecuteAsyncHandle
    {
        public float progress { get; protected set; }
        public ExecuteStatus status { get; set; }
        private IEnumerator _enumerator;
        protected List<ISubscribe> subscribes = new List<ISubscribe>();

        public IEnumerator GetCoroutine()
        {
            if (_enumerator is null)
            {
                _enumerator = GenericExeuteCoroutine();
            }

            return _enumerator;
        }

        public void Subscribe(ISubscribe subscribe)
        {
            subscribes.Add(subscribe);
        }

        public bool EnsureExecuteSuccessfuly()
        {
            return status == ExecuteStatus.Success;
        }


        public void Cancel()
        {
            status = ExecuteStatus.Canceled;
        }

        public virtual void Release()
        {
            status = ExecuteStatus.None;
            subscribes.ForEach(Engine.Class.Release);
            subscribes.Clear();
            progress = 0;
        }

        public virtual void Completion()
        {
            for (int i = 0; i < subscribes.Count; i++)
            {
                try
                {
                    if (subscribes[i] is ISubscribe<IWriteFileAsyncExecuteHandle> write)
                    {
                        write.Execute(this);
                    }
                    else
                    {
                    }

                    subscribes[i].Execute(this);
                }
                catch (Exception e)
                {
                    Engine.Console.Error(e);
                }
            }
        }

        public void Paused()
        {
            if (status is not ExecuteStatus.None)
            {
                Engine.Console.Error("The current Execute has not started executing");
                return;
            }

            if (status is ExecuteStatus.Paused)
            {
                Engine.Console.Error("The current execute has been paused");
                return;
            }

            if (status is ExecuteStatus.Canceled || status is ExecuteStatus.Failed || status is ExecuteStatus.Success)
            {
                Engine.Console.Error("Current execute completed");
                return;
            }

            status = ExecuteStatus.Paused;
        }

        public void Resume()
        {
            if (status is not ExecuteStatus.Paused)
            {
                Engine.Console.Error("The current execute is not in a paused state, and there is no need to reply to the execution");
                return;
            }

            if (status is ExecuteStatus.Execute)
            {
                Engine.Console.Error("The current execute is already in progress");
                return;
            }

            if (status is ExecuteStatus.Canceled || status is ExecuteStatus.Failed || status is ExecuteStatus.Success)
            {
                Engine.Console.Error("Current execute completed");
                return;
            }

            status = ExecuteStatus.Execute;
        }

        public abstract void Execute(params object[] args);
        protected abstract IEnumerator GenericExeuteCoroutine();
    }
}