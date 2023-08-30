using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace ZEngine
{
    /// <summary>
    /// 执行器
    /// </summary>
    public interface IExecute : IReference
    {
        /// <summary>
        /// 开始执行
        /// </summary>
        /// <param name="args"></param>
        void Execute();
    }

    public abstract class AbstractExecute : IExecute
    {
        public void Execute()
        {
            ExecuteCommand();
            WaitFor.WaitFormFrameEnd(() => { Engine.Class.Release(this); });
        }

        public abstract void Release();
        protected abstract void ExecuteCommand();
    }
}