using System;
using System.Collections.Generic;

namespace ZGame.Game
{
    class SystemManager : IReference
    {
        private List<ISystem> systemList = new();


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
            ISystem system = (ISystem)RefPooled.Spawner(systemType);
            if (system is IInitSystem initSystem)
            {
                initSystem.OnAwakw();
            }

            systemList.Add(system);
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

            systemList.RemoveAll(x => x.GetType() == systemType);
        }

        public ISystem GetSystem(Type systemType)
        {
            if (typeof(ISystem).IsAssignableFrom(systemType) is false)
            {
                throw new NotImplementedException(nameof(ISystem));
            }

            return systemList.Find(x => systemType.IsAssignableFrom(x.GetType()));
        }

        public void Update()
        {
            for (int i = systemList.Count - 1; i >= 0; i--)
            {
                if (systemList[i] is IUpdateSystem updateSystem)
                {
                    updateSystem.OnUpdate();
                }
            }
        }

        public void FixedUpdate()
        {
            for (int i = systemList.Count - 1; i >= 0; i--)
            {
                if (systemList[i] is IFixedSystem fixedUpdateSystem)
                {
                    fixedUpdateSystem.OnFixedUpdate();
                }
            }
        }

        public void LateUpdate()
        {
            for (int i = systemList.Count - 1; i >= 0; i--)
            {
                if (systemList[i] is ILateSystem lateUpdateSystem)
                {
                    lateUpdateSystem.OnLateUpdate();
                }
            }
        }

        public void OnDarwGizmons()
        {
            for (int i = systemList.Count - 1; i >= 0; i--)
            {
                if (systemList[i] is IGizmoSystem gizmoSystem)
                {
                    gizmoSystem.OnDrawingGizom();
                }
            }
        }

        public void OnGUI()
        {
        }

        public void Release()
        {
            systemList.ForEach(RefPooled.Release);
            systemList.Clear();
        }
    }
}