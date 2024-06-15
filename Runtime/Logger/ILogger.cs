using System.Diagnostics;
using System.Text;
using Debug = UnityEngine.Debug;

namespace ZGame.Logger
{
    public interface ILogger : IReference
    {
        /// <summary>
        /// 输出普通日志
        /// </summary>
        /// <param name="msg"></param>
        void Log(object msg);

        /// <summary>
        /// 输出普通日志
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        void LogFormat(string format, params object[] args);

        /// <summary>
        /// 输出错误日志
        /// </summary>
        /// <param name="msg"></param>
        void LogError(object msg);

        /// <summary>
        /// 输出错误日志
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        void LogErrorFormat(string format, params object[] args);

        /// <summary>
        /// 输出警告日志
        /// </summary>
        /// <param name="msg"></param>
        void LogWarning(object msg);

        /// <summary>
        /// 输出警告日志
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        void LogWarningFormat(string format, params object[] args);


        internal static ILogger Create(string name)
        {
            return UnityLogger.Create(name);
        }

        class UnityLogger : ILogger
        {
            private string name;

            public static UnityLogger Create(string name)
            {
                UnityLogger logger = RefPooled.Alloc<UnityLogger>();
                logger.name = name;
                return logger;
            }

            public void Release()
            {
            }


            public void Log(object msg)
            {
                Debug.Log($"[{name}] : {msg}");
            }

            public void LogFormat(string format, params object[] args)
            {
                Debug.LogFormat(string.Format($"[{name}] : {string.Format(format, args)}"));
            }

            public void LogError(object msg)
            {
                Debug.LogError($"[{name}] : {msg}");
            }

            public void LogErrorFormat(string format, params object[] args)
            {
                Debug.LogErrorFormat(string.Format($"[{name}] : {string.Format(format, args)}"));
            }

            public void LogWarning(object msg)
            {
                Debug.LogWarning($"[{name}] : {msg}");
            }

            public void LogWarningFormat(string format, params object[] args)
            {
                Debug.LogWarningFormat(string.Format($"[{name}] : {string.Format(format, args)}"));
            }
        }
    }
}