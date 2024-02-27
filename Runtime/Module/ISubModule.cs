using System;

namespace ZGame
{
    /// <summary>
    /// 子模块
    /// </summary>
    public interface ISubModule : IDisposable
    {
        /// <summary>
        /// 激活模块
        /// </summary>
        /// <param name="args"></param>
        void Active(params object[] args);

        /// <summary>
        /// 失活模块
        /// </summary>
        void Inactive();

        /// <summary>
        /// 执行动作
        /// </summary>
        /// <param name="actionName"></param>
        /// <param name="args"></param>
        object DOAction(string actionName, params object[] args);

        /// <summary>
        /// 执行动作
        /// </summary>
        /// <param name="actionName"></param>
        /// <param name="args"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T DOAction<T>(string actionName, params object[] args);
    }
}