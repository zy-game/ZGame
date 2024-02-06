using System;
using System.Collections.Generic;
using UnityEngine;
using ZGame;

namespace ZGame
{
    /// <summary>
    /// 命令管理器
    /// </summary>
    public class CommandManager
    {
        private static List<ICommandExecuter> recviers = new List<ICommandExecuter>();
        private static Dictionary<string, ICommandExecuter> recviersMap = new Dictionary<string, ICommandExecuter>();

        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="command"></param>
        /// <param name="args"></param>
        public static void OnExecuteCommand<T>(params object[] args)
        {
            OnExecuteCommand(typeof(T), args);
        }

        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="type"></param>
        /// <param name="args"></param>
        public static void OnExecuteCommand(Type type, params object[] args)
        {
            ICommandExecuter commandCommandExecuter = recviers.Find(x => x.GetType() == type);
            if (commandCommandExecuter is null)
            {
                recviers.Add(commandCommandExecuter = (ICommandExecuter)Activator.CreateInstance(type));
            }

            commandCommandExecuter.Executer(args);
            Debug.Log("Execute Command:" + type);
        }

        /// <summary>
        /// 注册命令
        /// </summary>
        /// <param name="command"></param>
        /// <param name="action"></param>
        public static void RegisterCommand(string command, Action<object[]> action)
        {
            if (recviersMap.TryGetValue(command, out ICommandExecuter handler) is false)
            {
                recviersMap.Add(command, handler = new CommonCommandHandle());
            }

            ((CommonCommandHandle)handler).Add(action);
        }

        /// <summary>
        /// 注册命令
        /// </summary>
        /// <param name="command"></param>
        /// <param name="action"></param>
        public static void RegisterCommand(string command, Action action)
        {
            if (recviersMap.TryGetValue(command, out ICommandExecuter handler) is false)
            {
                recviersMap.Add(command, handler = new CommonCommandHandle());
            }

            ((CommonCommandHandle)handler).Add(action);
        }

        /// <summary>
        /// 移除命令
        /// </summary>
        /// <param name="command"></param>
        /// <param name="action"></param>
        public static void UnregisterCommand(string command, Action<object[]> action)
        {
            if (recviersMap.TryGetValue(command, out ICommandExecuter handler) is false)
            {
                return;
            }

            ((CommonCommandHandle)handler).Remove(action);
        }

        /// <summary>
        /// 移除命令
        /// </summary>
        /// <param name="command"></param>
        /// <param name="action"></param>
        public static void UnregisterCommand(string command, Action action)
        {
            if (recviersMap.TryGetValue(command, out ICommandExecuter handler) is false)
            {
                return;
            }

            ((CommonCommandHandle)handler).Remove(action);
        }

        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="command"></param>
        /// <param name="args"></param>
        public static void OnExecuteCommand(string command, params object[] args)
        {
            if (recviersMap.TryGetValue(command, out ICommandExecuter handler) is false)
            {
                Debug.Log("Not Find The Command:" + command);
                return;
            }

            handler.Executer(args);
        }
    }
}