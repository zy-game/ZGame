using System;
using System.Collections.Generic;

namespace ZGame.Game
{
    class SystemManager : IReferenceObject
    {
        private List<IGizmoSystem> gizmoSystemList = new();
        private List<IUpdateSystem> updateSystemList = new();
        private List<ILateUpdateSystem> lateUpdateSystemList = new();
        private List<IFixedUpdateSystem> fixedUpdateSystemList = new();

        /// <summary>
        /// 注册逻辑系统
        /// </summary>
        /// <param name="systemType"></param>
        /// <param name="args"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void RegisterGameLogicSystem(Type systemType, params object[] args)
        {
            if (typeof(ISystem).IsAssignableFrom(systemType) is false)
            {
                throw new NotImplementedException(nameof(ISystem));
            }

            UnregisterGameLogicSystem(systemType);
            ISystem system = (ISystem)GameFrameworkFactory.Spawner(systemType);
            bool isInit = false;
            if (system is IInitSystem initSystem)
            {
                initSystem.OnAwakw();
                isInit = true;
            }

            if (system is IGizmoSystem gizmoSystem)
            {
                gizmoSystemList.Add(gizmoSystem);
                isInit = false;
                gizmoSystemList.Sort((x, y) => x.priority.CompareTo(y.priority));
            }

            if (system is IUpdateSystem updateSystem)
            {
                updateSystemList.Add(updateSystem);
                isInit = false;
                updateSystemList.Sort((x, y) => x.priority.CompareTo(y.priority));
            }

            if (system is IFixedUpdateSystem fixedUpdateSystem)
            {
                fixedUpdateSystemList.Add(fixedUpdateSystem);
                isInit = false;
                fixedUpdateSystemList.Sort((x, y) => x.priority.CompareTo(y.priority));
            }

            if (system is ILateUpdateSystem lateUpdateSystem)
            {
                lateUpdateSystemList.Add(lateUpdateSystem);
                isInit = false;
                lateUpdateSystemList.Sort((x, y) => x.priority.CompareTo(y.priority));
            }

            if (isInit)
            {
                GameFrameworkFactory.Release(system);
            }
        }

        /// <summary>
        /// 注册逻辑系统
        /// </summary>
        /// <param name="args"></param>
        /// <typeparam name="T"></typeparam>
        public void RegisterGameLogicSystem<T>(params object[] args) where T : ISystem
        {
            RegisterGameLogicSystem(typeof(T), args);
        }

        /// <summary>
        /// 卸载逻辑系统
        /// </summary>
        /// <param name="systemType"></param>
        public void UnregisterGameLogicSystem(Type systemType)
        {
            if (typeof(ISystem).IsAssignableFrom(systemType) is false)
            {
                throw new NotImplementedException(nameof(ISystem));
            }

            gizmoSystemList.RemoveAll(x => x.GetType() == systemType);
            updateSystemList.RemoveAll(x => x.GetType() == systemType);
            lateUpdateSystemList.RemoveAll(x => x.GetType() == systemType);
            fixedUpdateSystemList.RemoveAll(x => x.GetType() == systemType);
        }

        public void Update()
        {
            for (int i = updateSystemList.Count - 1; i >= 0; i--)
            {
                updateSystemList[i].OnUpdate();
            }
        }

        public void FixedUpdate()
        {
            for (int i = fixedUpdateSystemList.Count - 1; i >= 0; i--)
            {
                fixedUpdateSystemList[i].OnFixedUpdate();
            }
        }

        public void LateUpdate()
        {
            for (int i = lateUpdateSystemList.Count - 1; i >= 0; i--)
            {
                lateUpdateSystemList[i].OnLateUpdate();
            }
        }

        public void OnDarwingGizmons()
        {
            for (int i = gizmoSystemList.Count - 1; i >= 0; i--)
            {
                gizmoSystemList[i].OnDrawingGizom();
            }
        }

        public void Release()
        {
            gizmoSystemList.ForEach(GameFrameworkFactory.Release);
            gizmoSystemList.Clear();
            updateSystemList.ForEach(GameFrameworkFactory.Release);
            updateSystemList.Clear();
            lateUpdateSystemList.ForEach(GameFrameworkFactory.Release);
            lateUpdateSystemList.Clear();
            fixedUpdateSystemList.ForEach(GameFrameworkFactory.Release);
            fixedUpdateSystemList.Clear();
        }
    }
}