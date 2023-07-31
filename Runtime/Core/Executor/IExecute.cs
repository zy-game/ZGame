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
        Status status { get; protected set; }

        /// <summary>
        /// 确保执行成功
        /// </summary>
        /// <returns></returns>
        public bool EnsureExecuteSuccessfuly()
        {
            return status == Status.Success;
        }

        /// <summary>
        /// 开始执行
        /// </summary>
        /// <param name="args"></param>
        void Execute(params object[] args);
    }

 
}