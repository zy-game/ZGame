using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using ZGame.UI;

namespace ZGame.VFS
{
    public class ResourcePackageManifestManager : IReference
    {
        private List<ResourcePackageListManifest> _packageListManifests = new List<ResourcePackageListManifest>();

        public void Release()
        {
            _packageListManifests.Clear();
        }

        /// <summary>
        /// 设置资源包模块
        /// </summary>
        /// <param name="packageName"></param>
        public async UniTask<Status> LoadingPackageManifestData(string packageName)
        {
#if UNITY_EDITOR
            if (ResConfig.instance.resMode == ResourceMode.Editor)
            {
                return Status.Success;
            }
#endif
            ResourcePackageListManifest resourcePackageListManifest = _packageListManifests.Find(x => x.name == packageName);
            if (resourcePackageListManifest is not null)
            {
                return Status.Success;
            }

            string iniFilePath = ResConfig.instance.GetFilePath($"{packageName}.ini");
            if (ResConfig.instance.current.type == OSSType.Streaming && Application.isEditor)
            {
                resourcePackageListManifest = JsonConvert.DeserializeObject<ResourcePackageListManifest>(File.ReadAllText(iniFilePath));
            }
            else
            {
                resourcePackageListManifest = await CoreAPI.Network.GetData<ResourcePackageListManifest>(iniFilePath);
            }


            if (resourcePackageListManifest is null)
            {
                CoreAPI.Logger.LogError("没有找到资源包列表配置文件：" + iniFilePath);
                return Status.Fail;
            }

            if (resourcePackageListManifest.appVersion.Equals(GameConfig.instance.version) is false)
            {
                if (await UIMsgBox.ShowAsync(CoreAPI.Language.Query("App 版本过低，请重新安装App后在使用")))
                {
                    Application.OpenURL(GameConfig.instance.apkUrl);
                    GameFrameworkStartup.Quit();
                }

                throw new Exception("App 版本过低，请重新安装App后在使用");
            }

            _packageListManifests.Add(resourcePackageListManifest);
            if (resourcePackageListManifest.dependencies is not null && resourcePackageListManifest.dependencies.Count > 0)
            {
                foreach (var VARIABLE in resourcePackageListManifest.dependencies)
                {
                    await LoadingPackageManifestData(VARIABLE);
                }
            }

            return Status.Success;
        }

        public ResourcePackageManifest GetResourcePackageManifest(string packageName)
        {
            ResourcePackageManifest manifest = default;
            foreach (var VARIABLE in _packageListManifests)
            {
                if ((manifest = VARIABLE.packages.FirstOrDefault(x => x.name == packageName)) != null)
                {
                    break;
                }
            }

            return manifest;
        }


        /// <summary>
        /// 根据资源名查找对应的资源包
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public bool TryGetPackageManifestWithAssetName(string assetName, out ResourcePackageManifest manifest)
        {
            foreach (var VARIABLE in _packageListManifests)
            {
                var m = VARIABLE.packages.FirstOrDefault(x => x.Contains(assetName));
                if (m is not null)
                {
                    manifest = m;
                    return true;
                }
            }

            manifest = default;
            return false;
        }

        /// <summary>
        /// 获取资源包版本
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public bool TryGetPackageVersion(string packageName, out uint version)
        {
            ResourcePackageManifest resourcePackageManifest = GetResourcePackageManifest(packageName);
            if (resourcePackageManifest is null)
            {
                version = 0;
                return false;
            }

            version = resourcePackageManifest.version;
            return true;
        }

        /// <summary>
        /// 获取资源的全路径
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public string GetAssetFullPath(string assetName)
        {
            ResourcePackageListManifest resourcePackageManifest = _packageListManifests.Find(x => x.HasAsset(assetName));
            if (resourcePackageManifest is null)
            {
                return null;
            }

            return resourcePackageManifest.GetAssetFullPath(assetName);
        }

        /// <summary>
        /// 获取需要更新的资源包列表
        /// </summary>
        /// <param name="packageName"></param>
        /// <returns></returns>
        public List<ResourcePackageManifest> GetUpdateResourcePackageList(string packageName)
        {
            List<ResourcePackageManifest> needUpdatePackages = new List<ResourcePackageManifest>();
#if UNITY_EDITOR
            if (ResConfig.instance.resMode == ResourceMode.Editor)
            {
                return needUpdatePackages;
            }
#endif
            ResourcePackageListManifest resourcePackageListManifest = _packageListManifests.Find(x => x.name == packageName);
            if (resourcePackageListManifest is null)
            {
                CoreAPI.Logger.LogError("没有找到资源包列表配置文件：" + packageName);
                return needUpdatePackages;
            }


            foreach (var package in resourcePackageListManifest.packages)
            {
                if (CoreAPI.VFS.Exist(package.name, package.version) || needUpdatePackages.Contains(package))
                {
                    continue;
                }

                needUpdatePackages.Add(package);
            }

            if (resourcePackageListManifest.dependencies is not null && resourcePackageListManifest.dependencies.Count > 0)
            {
                foreach (string dependency in resourcePackageListManifest.dependencies)
                {
                    needUpdatePackages.AddRange(GetUpdateResourcePackageList(dependency));
                }
            }

            return needUpdatePackages;
        }

        /// <summary>
        /// 获取资源包以及资源包的依赖列表
        /// </summary>
        /// <param name="packageName"></param>
        /// <returns></returns>
        public List<ResourcePackageManifest> GetResourcePackageAndDependencyList(string packageName)
        {
            List<ResourcePackageManifest> result = new List<ResourcePackageManifest>();
            ResourcePackageListManifest resourcePackageListManifest = _packageListManifests.Find(x => x.name == packageName);
            if (resourcePackageListManifest is null)
            {
                return result;
            }

            result.AddRange(resourcePackageListManifest.packages.Where(x => x.isBundle));
            if (resourcePackageListManifest.dependencies is not null && resourcePackageListManifest.dependencies.Count > 0)
            {
                foreach (string dependency in resourcePackageListManifest.dependencies)
                {
                    result.AddRange(GetResourcePackageAndDependencyList(dependency));
                }
            }

            return result;
        }
    }
}