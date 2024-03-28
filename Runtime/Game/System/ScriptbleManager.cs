using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZGame.Game
{
    public class ScriptbleManager : GameFrameworkModule
    {
        private List<IGameLogicSystem> logicSystemList;


        public override void OnAwake(params object[] args)
        {
            logicSystemList = new List<IGameLogicSystem>();
        }

        public override void Update()
        {
            for (int i = logicSystemList.Count - 1; i >= 0; i--)
            {
                logicSystemList[i].OnUpdate();
            }
        }

        public override void FixedUpdate()
        {
            for (int i = logicSystemList.Count - 1; i >= 0; i--)
            {
                logicSystemList[i].OnFixedUpdate();
            }
        }

        public override void LateUpdate()
        {
            for (int i = logicSystemList.Count - 1; i >= 0; i--)
            {
                logicSystemList[i].OnLateUpdate();
            }
        }

        /// <summary>
        /// 注册逻辑系统
        /// </summary>
        /// <param name="systemType"></param>
        /// <param name="args"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void LoadingLogicSystem(Type systemType, params object[] args)
        {
            if (typeof(IGameLogicSystem).IsAssignableFrom(systemType) is false)
            {
                throw new NotImplementedException(nameof(IGameLogicSystem));
            }

            UnregisterLogicSystem(systemType);
            IGameLogicSystem system = (IGameLogicSystem)GameFrameworkFactory.Spawner(systemType);
            logicSystemList.Add(system);
            system.OnAwakw(args);
        }

        /// <summary>
        /// 注册逻辑系统
        /// </summary>
        /// <param name="args"></param>
        /// <typeparam name="T"></typeparam>
        public void LoadingLogicSystem<T>(params object[] args) where T : IGameLogicSystem
        {
            LoadingLogicSystem(typeof(T), args);
        }

        /// <summary>
        /// 卸载逻辑系统
        /// </summary>
        /// <param name="systemType"></param>
        public void UnregisterLogicSystem(Type systemType)
        {
            if (typeof(IGameLogicSystem).IsAssignableFrom(systemType) is false)
            {
                throw new NotImplementedException(nameof(IGameLogicSystem));
            }

            IGameLogicSystem gameLogicSystem = logicSystemList.Find(x => x.GetType() == systemType);
            if (gameLogicSystem is not null)
            {
                gameLogicSystem.Dispose();
                logicSystemList.Remove(gameLogicSystem);
            }
        }

        /// <summary>
        /// 卸载逻辑系统
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void UnregisterLogicSystem<T>() where T : IGameLogicSystem
        {
            UnregisterLogicSystem(typeof(T));
        }

        public override void Release()
        {
            logicSystemList.ForEach(x => x.Dispose());
            logicSystemList.Clear();
        }
    }
}