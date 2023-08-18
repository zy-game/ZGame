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
            if (gameEntryOptions is null)
            {
                Engine.Console.Log("入口配置不能为空");
                return default;
            }

            InternalLaunchGameLogicModuleExecuteHandle internalLaunchGameLogicModuleExecuteHandle = Engine.Class.Loader<InternalLaunchGameLogicModuleExecuteHandle>();
            internalLaunchGameLogicModuleExecuteHandle.Execute(gameEntryOptions);
            return internalLaunchGameLogicModuleExecuteHandle;
        }

        public IGameWorld CreateWorld(string name, Camera camera)
        {
            GameWorldHandle gameWorldHandle = Engine.Class.Loader<GameWorldHandle>();
            gameWorldHandle.name = name;
            gameWorldHandle.camera = camera;
            gameWorldHandles.Add(gameWorldHandle);
            if (current is not null)
            {
                current.OnDisable();
                current = null;
            }

            UniversalAdditionalCameraData universalAdditionalCameraData = camera.gameObject.GetComponent<UniversalAdditionalCameraData>();
            if (universalAdditionalCameraData == null)
            {
                universalAdditionalCameraData = camera.gameObject.AddComponent<UniversalAdditionalCameraData>();
            }

            universalAdditionalCameraData.renderType = CameraRenderType.Overlay;
            Camera.main.GetComponent<UniversalAdditionalCameraData>().cameraStack.Add(camera);
            current = gameWorldHandle;
            return gameWorldHandle;
        }

        public void OnDisable(string name)
        {
            IGameWorld handle = gameWorldHandles.Find(x => x.name == name);
            if (handle is null)
            {
                return;
            }

            handle.OnDisable();
        }

        public void OnEnable(string worldName)
        {
            IGameWorld handle = gameWorldHandles.Find(x => x.name == worldName);
            if (handle is null)
            {
                return;
            }

            handle.OnEnable();
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