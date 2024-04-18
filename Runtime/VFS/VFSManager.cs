using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using HybridCLR;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using ZGame.Config;
using ZGame.Game;
using ZGame.Language;
using ZGame.Networking;
using ZGame.UI;
using ZGame.VFS.Command;
using Object = UnityEngine.Object;

namespace ZGame.VFS
{
    /// <summary>
    /// 虚拟文件系统
    /// </summary>
    public class VFSManager : GameFrameworkModule
    {
        private NFSManager _disk;
        private float lastCheckUnuseResourceTime;
        private PackageManifestManager manifestManager;

        public override void OnAwake(params object[] args)
        {
            manifestManager = new PackageManifestManager();
            lastCheckUnuseResourceTime = Time.realtimeSinceStartup;
            _disk = NFSManager.OpenOrCreateDisk(GameConfig.instance.title + " virtual data.disk");
        }

        public override void Release()
        {
            RefPooled.Release(manifestManager);
            RefPooled.Release(_disk);
        }
#if UNITY_EDITOR
        public override void OnGUI()
        {
            ResPackage.OnDrawingGUI();
            ResObject.OnDrawingGUI();
        }
#endif

        public override void Update()
        {
            if (Time.realtimeSinceStartup - lastCheckUnuseResourceTime < ResConfig.instance.timeout)
            {
                return;
            }

            UnloadUnuseResources();
        }

        /// <summary>
        /// 获取资源所在包的清单
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="manifest"></param>
        /// <returns></returns>
        public bool TryGetPackageManifestWithAssetName(string assetName, out ResourcePackageManifest manifest)
        {
            return manifestManager.TryGetPackageManifestWithAssetName(assetName, out manifest);
        }


        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="fileName"></param>
        public void Delete(string fileName)
        {
            _disk.Delete(fileName);
        }

        /// <summary>
        /// 是否存在文件
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public bool Exist(string fileName, uint version)
        {
            return _disk.Exists(fileName, version);
        }

        /// <summary>
        /// 是否存在文件
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public bool Exist(string fileName)
        {
            return _disk.Exists(fileName);
        }

        /// <summary>
        /// 写入文件数据
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="bytes"></param>
        /// <param name="version"></param>
        public Status Write(string fileName, byte[] bytes, uint version)
        {
            return _disk.Write(fileName, bytes, version);
        }

        /// <summary>
        /// 写入文件数据
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="bytes"></param>
        /// <param name="version"></param>
        public UniTask<Status> WriteAsync(string fileName, byte[] bytes, uint version)
        {
            return _disk.WriteAsync(fileName, bytes, version);
        }

        /// <summary>
        /// 读取文件数据
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <param name="version">文件版本，当为0时，忽略版本控制</param>
        /// <returns>文件数据</returns>
        public byte[] Read(string fileName, uint version = 0)
        {
            return _disk.Read(fileName);
        }

        /// <summary>
        /// 读取文件数据
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <param name="version">文件版本，当为0时，忽略版本控制</param>
        /// <returns>文件数据</returns>
        public UniTask<byte[]> ReadAsync(string fileName, uint version = 0)
        {
            return _disk.ReadAsync(fileName);
        }

        /// <summary>
        /// 加载资源包
        /// </summary>
        /// <param name="packageName"></param>
        /// <returns></returns>
        public async UniTask<Status> LoadingResourcePackageAsync(string packageName)
        {
            if (await manifestManager.LoadingPackageManifestData(packageName) is not Status.Success)
            {
                return Status.Fail;
            }
#if !UNITY_WEBGL
            using (UpdatePackageCommand command = RefPooled.Spawner<UpdatePackageCommand>())
            {
                if (await command.OnExecute(manifestManager.GetUpdateResourcePackageList(packageName).ToArray()) is not Status.Success)
                {
                    return Status.Fail;
                }
            }
#endif
            using (LoadingResPackageAsyncCommand command = RefPooled.Spawner<LoadingResPackageAsyncCommand>())
            {
                if (await command.OnExecute(manifestManager.GetResourcePackageAndDependencyList(packageName).ToArray()) is not Status.Success)
                {
                    return Status.Fail;
                }
            }

            return Status.Success;
        }

        /// <summary>
        /// 卸载未使用的资源对象和资源包
        /// </summary>
        /// <param name="packageNameList"></param>
        public void UnloadUnuseResources()
        {
            lastCheckUnuseResourceTime = Time.realtimeSinceStartup;
            ResPackage.ReleaseUnusedRefObject();
            ResPackage.CheckUnusedRefObject();
            ResObject.ReleaseUnusedRefObject();
            ResObject.CheckUnusedRefObject();
        }

        /// <summary>
        /// 卸载指定的资源包
        /// </summary>
        /// <param name="packageNameList"></param>
        public void UnloadResPackageList(string packageManifestName)
        {
            if (packageManifestName.IsNullOrEmpty())
            {
                return;
            }

            List<ResourcePackageManifest> manifests = manifestManager.GetResourcePackageAndDependencyList(packageManifestName);
            if (manifests is null || manifests.Count == 0)
            {
                return;
            }

            manifests.ForEach(x => ResPackage.Unload(x.name));
        }

        /// <summary>
        /// 加载资源
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public ResObject GetAsset(string path)
        {
            using (LoadingResObjectCommand command = RefPooled.Spawner<LoadingResObjectCommand>())
            {
                return command.OnExecute(path);
            }
        }

        /// <summary>
        /// 加载资源
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public async UniTask<ResObject> GetAssetAsync(string path)
        {
            using (LoadingResObjectAsyncCommand command = RefPooled.Spawner<LoadingResObjectAsyncCommand>())
            {
                return await command.OnExecute(path);
            }
        }

        /// <summary>
        /// 获取网络资源
        /// </summary>
        /// <param name="path"></param>
        /// <param name="type"></param>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public async UniTask<ResObject> GetStreamingAssetAsync(string path, StreamingAssetType type, string assetName = "")
        {
            if (ResObject.TryGetValue(path, out ResObject resObject))
            {
                return resObject;
            }

            using (LoadingStreamingAssetCommand command = RefPooled.Spawner<LoadingStreamingAssetCommand>())
            {
                return await command.OnExecute(path, type, assetName);
            }
        }

        /// <summary>
        /// 加载场景
        /// </summary>
        /// <param name="path">场景路径</param>
        /// <param name="callback">场景加载进度回调</param>
        /// <param name="mode">场景加载模式</param>
        /// <returns></returns>
        public async UniTask<ResObject> GetSceneAsync(string path, IProgress<float> callback, LoadSceneMode mode = LoadSceneMode.Single)
        {
            if (path.IsNullOrEmpty())
            {
                return ResObject.DEFAULT;
            }

            if (ResObject.TryGetValue(path, out ResObject sceneObject))
            {
                return sceneObject;
            }

            using (LoadingSceneCommand command = RefPooled.Spawner<LoadingSceneCommand>())
            {
                return await command.OnExecute(path, callback, mode, manifestManager);
            }
        }

        /// <summary>
        /// 加载热更新代码
        /// </summary>
        /// <param name="dllName"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        /// <exception cref="FileNotFoundException"></exception>
        public async UniTask<Status> LoadingSubGameEntryPoint(string dllName, CodeMode mode)
        {
            using (LoadingHotfixAssemblyCommand command = RefPooled.Spawner<LoadingHotfixAssemblyCommand>())
            {
                return await command.OnExecute(dllName, mode, manifestManager);
            }
        }
    }
}