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
        /// 确保执行成功
        /// </summary>
        /// <returns></returns>
        bool EnsureExecuteSuccessfuly();

        /// <summary>
        /// 开始执行
        /// </summary>
        /// <param name="args"></param>
        void Execute(params object[] args);

        
    }

    public static class ExecuteExtension
    {
        
    }
}