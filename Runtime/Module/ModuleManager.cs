using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using ZGame;

namespace ZGame
{
    /// <summary>
    /// 模块管理器
    /// </summary>
    public class ModuleManager
    {
        private static Dictionary<Type, ISubModule> _modules = new Dictionary<Type, ISubModule>();

        /// <summary>
        /// 调用模块
        /// </summary>
        /// <param name="actionName"></param>
        /// <param name="args"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T OnAction<T>(string actionName, params object[] args)
        {
            return (T)OnAction(actionName, args);
        }

        /// <summary>
        /// 调用模块
        /// </summary>
        /// <param name="actionName"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static object OnAction(string actionName, params object[] args)
        {
            object result = default;
            foreach (var VARIABLE in _modules.Values)
            {
                result = VARIABLE.DOAction(actionName, args);
                if (result is not null)
                {
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// 加载模块
        /// </summary>
        /// <param name="args"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        static public T LoadAndActiveModule<T>(params object[] args) where T : ISubModule
        {
            return (T)LoadAndActiveModule(typeof(T), args);
        }

        /// <summary>
        /// 加载模块
        /// </summary>
        /// <param name="type"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        static public ISubModule LoadAndActiveModule(Type type, params object[] args)
        {
            if (type is null || typeof(ISubModule).IsAssignableFrom(type) is false)
            {
                return default;
            }

            ISubModule module = (ISubModule)Activator.CreateInstance(type);
            _modules.Add(type, module);
            module.Active(args);
            return module;
        }

        /// <summary>
        /// 卸载模块
        /// </summary>
        /// <typeparam name="T"></typeparam>
        static public void UnloadModule<T>() where T : ISubModule
        {
            UnloadModule(typeof(T));
        }

        /// <summary>
        /// 卸载模块
        /// </summary>
        /// <param name="type"></param>
        static public void UnloadModule(Type type)
        {
            if (type is null || typeof(ISubModule).IsAssignableFrom(type) is false)
            {
                return;
            }

            ISubModule module = _modules[type];
            module.Inactive();
            module.Dispose();
            _modules.Remove(type);
        }

        /// <summary>
        /// 卸载所有模块
        /// </summary>
        static public void UnloadAllModule()
        {
            foreach (var module in _modules.Values)
            {
                module.Inactive();
                module.Dispose();
            }

            _modules.Clear();
        }

        /// <summary>
        /// 激活模块
        /// </summary>
        /// <param name="args"></param>
        /// <typeparam name="T"></typeparam>
        static public void ActiveModule<T>(params object[] args) where T : ISubModule
        {
            ActiveModule(typeof(T), args);
        }

        /// <summary>
        /// 激活模块
        /// </summary>
        /// <param name="type"></param>
        /// <param name="args"></param>
        static public void ActiveModule(Type type, params object[] args)
        {
            if (type is null || typeof(ISubModule).IsAssignableFrom(type) is false)
            {
                return;
            }

            ISubModule module = _modules[type];
            module.Active(args);
        }

        /// <summary>
        /// 失活模块
        /// </summary>
        /// <typeparam name="T"></typeparam>
        static public void InactiveModule<T>() where T : ISubModule
        {
            InactiveModule(typeof(T));
        }

        /// <summary>
        /// 失活模块
        /// </summary>
        /// <param name="type"></param>
        static public void InactiveModule(Type type)
        {
            if (type is null || typeof(ISubModule).IsAssignableFrom(type) is false)
            {
                return;
            }

            ISubModule module = _modules[type];
            module.Inactive();
        }

        /// <summary>
        /// 是否加载模块
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        static public bool IsModuleLoaded<T>() where T : ISubModule
        {
            return IsModuleLoaded(typeof(T));
        }

        /// <summary>
        /// 是否加载模块
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        static public bool IsModuleLoaded(Type type)
        {
            return _modules.ContainsKey(type);
        }

        /// <summary>
        /// 获取模块
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        static public T GetModule<T>() where T : ISubModule
        {
            return (T)GetModule(typeof(T));
        }

        /// <summary>
        /// 获取模块
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        static public ISubModule GetModule(Type type)
        {
            ISubModule module = default;
            if (_modules.ContainsKey(type))
            {
                module = _modules[type];
            }

            return module;
        }

        /// <summary>
        /// 记载或获取模块
        /// </summary>
        /// <param name="args"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        static public T GetOrLoadModule<T>(params object[] args) where T : ISubModule
        {
            return (T)GetOrLoadModule(typeof(T), args);
        }

        /// <summary>
        /// 记载或获取模块
        /// </summary>
        /// <param name="type"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        static public ISubModule GetOrLoadModule(Type type, params object[] args)
        {
            if (IsModuleLoaded(type))
            {
                return GetModule(type);
            }
            else
            {
                return LoadAndActiveModule(type, args);
            }
        }
    }
}