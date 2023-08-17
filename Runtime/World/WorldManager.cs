using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HybridCLR;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.Universal;
using ZEngine.Resource;

namespace ZEngine.World
{
    public class WorldManager : Single<WorldManager>
    {
        private List<IGameWorld> gameWorldHandles = new List<IGameWorld>();
        public IGameWorld current { get; private set; }

        public override void Dispose()
        {
            base.Dispose();
            gameWorldHandles.ForEach(Engine.Class.Release);
            Engine.Console.Log("关闭游戏管理器");
        }

        public ILoaderGameLogicModuleExecuteHandle LoaderGameLogicModule(GameEntryOptions gameEntryOptions)
        {
            InternalLaunchGameLogicModuleExecuteHandle internalLaunchGameLogicModuleExecuteHandle = Engine.Class.Loader<InternalLaunchGameLogicModuleExecuteHandle>();
            internalLaunchGameLogicModuleExecuteHandle.Execute(gameEntryOptions);
            return internalLaunchGameLogicModuleExecuteHandle;
        }

        public IGameWorld CreateWorld(string name, Camera camera)
        {
            InternalGameWorldHandle internalGameWorldHandle = Engine.Class.Loader<InternalGameWorldHandle>();
            internalGameWorldHandle.name = name;
            internalGameWorldHandle.camera = camera;
            gameWorldHandles.Add(internalGameWorldHandle);
            if (current is not null)
            {
                current.OnDisable();
                current = null;
            }

            Camera.main.GetComponent<UniversalAdditionalCameraData>().cameraStack.Add(camera);
            current = internalGameWorldHandle;
            return internalGameWorldHandle;
        }

        public IGameWorld GetGameWorld(string worldName)
        {
            return gameWorldHandles.Find(x => x.name == worldName);
        }

        public void CloseWorld(string worldName)
        {
            if (current is not null && current.name == worldName)
            {
                current = null;
            }

            IGameWorld world = GetGameWorld(worldName);
            if (world is null)
            {
                return;
            }

            Camera.main.GetComponent<UniversalAdditionalCameraData>().cameraStack.Remove(world.camera);
            Engine.Class.Release(world);
            gameWorldHandles.Remove(world);
        }
    }
}