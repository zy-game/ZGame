using System;
using System.Collections.Generic;
using System.Linq;

namespace ZGame.Logger
{
    public class LoggerManager : GameManager
    {
        private bool _isDebug;
        private ILogger _log;

        public override void OnAwake(params object[] args)
        {
            _isDebug = AppCore.isDebug;
            _log = ILogger.Create("DEFAULT LOGGER");
        }

        public ILogger CreateLogger(string name)
        {
            return ILogger.Create(name);
        }

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

            _log.Log(msg);
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

            _log.LogFormat(format, args);
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

            _log.LogError(msg);
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

            _log.LogErrorFormat(format, args);
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

            _log.LogWarning(msg);
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

            _log.LogWarningFormat(format, args);
        }
    }
}