using System;
using UnityEngine;

namespace ZEngine
{
    class Consoler : Singleton<Consoler>
    {
        public Consoler()
        {
            if (Application.isPlaying)
            {
                BehaviourSingleton.OnGUICall(OnGUI);
            }
        }

        private void OnGUI()
        {
        }

        /// <summary>
        /// 在控制台输出一条日志
        /// </summary>
        /// <param name="message"></param>
        public void Log(object message)
            => Debug.Log($"[INFO] {message}");


        /// <summary>
        /// 输出警告信息
        /// </summary>
        /// <param name="message"></param>
        public void Warning(object message)
            => Debug.LogWarning($"[WARNING] {message}");


        /// <summary>
        /// 输出错误信息
        /// </summary>
        /// <param name="message"></param>
        public void Error(object message)
            => Debug.LogError($"[ERROR] {message}");
    }
}