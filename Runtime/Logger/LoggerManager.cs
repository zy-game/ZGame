using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ZGame.Logger
{
    public class LoggerManager : ZModule
    {
        private bool _isDebug = GameConfig.instance.isDebug;

        /// <summary>
        /// 输出普通日志
        /// </summary>
        /// <param name="msg"></param>
        public void Log(object msg)
        {
            if (_isDebug is false)
            {
                return;
            }

            Debug.Log(msg);
        }

        /// <summary>
        /// 输出普通日志
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public void LogFormat(string format, params object[] args)
        {
            if (_isDebug is false)
            {
                return;
            }

            Debug.LogFormat(format, args);
        }

        /// <summary>
        /// 输出错误日志
        /// </summary>
        /// <param name="msg"></param>
        public void LogError(object msg)
        {
            if (_isDebug is false)
            {
                return;
            }

            Debug.LogError(msg);
        }

        /// <summary>
        /// 输出错误日志
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public void LogErrorFormat(string format, params object[] args)
        {
            if (_isDebug is false)
            {
                return;
            }

            Debug.LogErrorFormat(format, args);
        }

        /// <summary>
        /// 输出警告日志
        /// </summary>
        /// <param name="msg"></param>
        public void LogWarning(object msg)
        {
            if (_isDebug is false)
            {
                return;
            }

            Debug.LogWarning(msg);
        }

        /// <summary>
        /// 输出警告日志
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public void LogWarningFormat(string format, params object[] args)
        {
            if (_isDebug is false)
            {
                return;
            }

            Debug.LogWarningFormat(format, args);
        }
    }
}