using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace ZEngine
{
    /// <summary>
    /// 执行器
    /// </summary>
    public interface ISchedule : IDisposable
    {
        /// <summary>
        /// 开始执行
        /// </summary>
        /// <param name="args"></param>
        void Execute(params object[] args);
    }

    public interface ISchedule<T> : ISchedule
    {
        T result { get; }
    }
}