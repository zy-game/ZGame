using System;
using UnityEngine;

namespace ZEngine
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class CommandMethod : Attribute
    {
        internal string command;

        public CommandMethod(string command)
        {
            this.command = command;
        }
    }

    class Console : Singleton<Console>
    {
        private GUISkin skin;

        public void ShowConsole()
        {
            skin = Resources.Load<GUISkin>("New GUISkin");
            Behaviour.OnGUICall(OnGUI);
        }

        private void OnGUI()
        {
            GUILayout.BeginHorizontal(skin.box, GUILayout.Width(60), GUILayout.Height(40));
            GUILayout.Button("Open", skin.button);
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// 在控制台输出一条日志
        /// </summary>
        /// <param name="message"></param>
        public void Log(object message)
            => Debug.Log(message);


        /// <summary>
        /// 输出警告信息
        /// </summary>
        /// <param name="message"></param>
        public void Warning(object message)
            => Debug.LogWarning(message);


        /// <summary>
        /// 输出错误信息
        /// </summary>
        /// <param name="message"></param>
        public void Error(object message)
            => Debug.LogError(message);
    }
}