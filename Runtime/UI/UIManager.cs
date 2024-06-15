using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using ZGame.Game;

namespace ZGame.UI
{
    class BackupData : IReference
    {
        public int layer;
        public Type uiType;

        public void Release()
        {
            uiType = null;
            layer = 0;
        }

        public static BackupData Create(int layer, Type uiType)
        {
            BackupData backupData = RefPooled.Alloc<BackupData>();
            backupData.layer = layer;
            backupData.uiType = uiType;
            return backupData;
        }
    }

    /// <summary>
    /// 界面管理器
    /// </summary>
    public sealed class UIManager : GameManager
    {
        private UIStack _stack;
        private UILayers _layers;
        private List<IUIFrom> uiList = new();
        private List<IUIFrom> cacheList = new();
        private Stack<BackupData> _backupQueue = new();


        public override void OnAwake(params object[] args)
        {
            _stack = UIStack.Create();
            _layers = UILayers.Create();
        }


        public void Backup()
        {
            AppCore.Logger.Log("Backup");
            // UIGroup group = _stack.Backup();
            if (_backupQueue.TryPop(out var backup))
            {
                Close(backup.uiType);
                RefPooled.Free(backup);
            }

            if (_backupQueue.TryPeek(out backup) is false)
            {
                return;
            }

            Enable(backup.uiType);
        }

        /// <summary>
        /// 显示或者加载UI
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="args"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Show<T>(int layer, params object[] args) where T : IUIFrom
        {
            return (T)Show(layer, typeof(T), args);
        }

        /// <summary>
        /// 显示或者加载UI
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="args"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Show<T>(UILayer layer, params object[] args) where T : IUIFrom
        {
            return (T)Show((int)layer, typeof(T), args);
        }

        /// <summary>
        /// 显示或者加载UI
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="type"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public IUIFrom Show(UILayer layer, Type type, params object[] args)
        {
            return Show((int)layer, type, args);
        }

        /// <summary>
        /// 显示或者加载UI
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="type"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        public IUIFrom Show(int layer, Type type, params object[] args)
        {
            if (type is null || type.IsInterface || type.IsAbstract || typeof(IUIFrom).IsAssignableFrom(type) is false)
            {
                AppCore.Logger.LogError("创建UI失败");
                return default;
            }

            IUIFrom uiBase = GetWindow(type);
            if (uiBase is null)
            {
                uiBase = cacheList.Find(x => x.GetType() == type);
                if (uiBase is not null)
                {
                    cacheList.Remove(uiBase);
                    uiList.Add(uiBase);
                }
                else
                {
                    RefPath refPath = type.GetCustomAttribute<RefPath>();
                    if (refPath is null)
                    {
                        throw new NullReferenceException(nameof(RefPath));
                    }

                    uiBase = UIBase.Create(refPath.path, type);
                    _layers.SetChild(layer, uiBase.rect_transform);
                    uiList.Add(uiBase);
                    uiBase.Awake();
                }

                uiBase.Start(args);
            }

            if (uiBase.GetType().GetCustomAttribute<UIBackup>() is not null)
            {
                if (_backupQueue.TryPeek(out var data))
                {
                    Disable(data.uiType);
                }

                _backupQueue.Push(BackupData.Create(layer, type));
            }

            UIName uiName = uiBase.GetType().GetCustomAttribute<UIName>();
            if (uiName is not null)
            {
                AppCore.Logger.Log("Show " + uiName.name);
                GetWindow<IUITitle>()?.SetTitle(uiName.name.IsNullOrEmpty() ? AppCore.Language.Query(uiName.code) : uiName.name);
            }


            uiBase.Enable();
            return uiBase;
        }

        /// <summary>
        /// 隐藏窗口
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void Disable<T>() where T : IUIFrom
        {
            Disable(typeof(T));
        }

        /// <summary>
        /// 隐藏窗口
        /// </summary>
        /// <param name="type"></param>
        public void Disable(Type type)
        {
            AppCore.Logger.Log("Disable");
            IUIFrom ui = GetWindow(type);
            if (ui is null)
            {
                return;
            }

            ui.Disable();
        }

        /// <summary>
        /// 隐藏所有窗口
        /// </summary>
        /// <param name="type"></param>
        public void DisableAll(Type type)
        {
            uiList.ForEach(x => x.Disable());
        }

        /// <summary>
        /// 隐藏窗口
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void Enable<T>() where T : IUIFrom
        {
            Enable(typeof(T));
        }

        /// <summary>
        /// 隐藏窗口
        /// </summary>
        /// <param name="type"></param>
        public void Enable(Type type)
        {
            IUIFrom ui = GetWindow(type);
            if (ui is null)
            {
                return;
            }

            ui.Enable();
        }

        /// <summary>
        /// 隐藏所有窗口
        /// </summary>
        /// <param name="type"></param>
        public void EnableAll(Type type)
        {
            uiList.ForEach(x => x.Enable());
        }

        /// <summary>
        /// 关闭指定类型的窗口
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void Close<T>() where T : IUIFrom
        {
            Close(typeof(T));
        }

        /// <summary>
        /// 关闭指定类型的窗口
        /// </summary>
        /// <param name="type"></param>
        public void Close(Type type)
        {
            IUIFrom ui = GetWindow(type);
            if (ui is null)
            {
                return;
            }

            ui.Disable();
            cacheList.Add(ui);
            uiList.Remove(ui);
        }

        /// <summary>
        /// 关闭所有窗口
        /// </summary>
        public void CloseAll()
        {
            AppCore.Logger.Log("Close");
            _backupQueue.Clear();
            uiList.ForEach(x => x.Disable());
            cacheList.AddRange(uiList);
            uiList.Clear();
        }

        /// <summary>
        /// 获取指定类型的窗口
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetWindow<T>() where T : IUIFrom
        {
            return (T)GetWindow(typeof(T));
        }

        /// <summary>
        /// 获取指定类型的窗口
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public IUIFrom GetWindow(Type type)
        {
            if (typeof(IUIFrom).IsAssignableFrom(type) is false)
            {
                throw new NotImplementedException(nameof(type));
            }

            return uiList.Find(x => type.IsAssignableFrom(x.GetType()));
        }

        /// <summary>
        /// 清理所有UI
        /// </summary>
        public void Clear()
        {
            uiList.ForEach(RefPooled.Free);
            cacheList.ForEach(RefPooled.Free);
            uiList.Clear();
            cacheList.Clear();
        }

        public Vector3 WorldToScreenPoint(Vector3 worldPosition, Camera renderCamera)
        {
            return _layers.WorldToScreenPoint(worldPosition, renderCamera);
        }

        public override void Release()
        {
            Clear();
        }
    }
}