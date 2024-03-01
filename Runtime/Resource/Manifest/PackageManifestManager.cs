using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using ZGame.Config;
using ZGame.FileSystem;
using ZGame.Game;
using ZGame.Networking;
using ZGame.Resource.Config;
using ZGame.UI;

namespace ZGame.Resource
{
    /// <summary>
    /// 资源包信息管理
    /// </summary>
    public class PackageManifestManager : Singleton<PackageManifestManager>
    {
        private List<ResourcePackageListManifest> _packageListManifests = new List<ResourcePackageListManifest>();

        public override void Dispose()
        {
            _packageListManifests.Clear();
        }

        /// <summary>
        /// 设置资源包模块
        /// </summary>
        /// <param name="packageName"></param>
        public async UniTask<bool> SetupPackageManifest(string packageName)
        {
#if UNITY_EDITOR
            if (BasicConfig.instance.resMode == ResourceMode.Editor)
            {
                return true;
            }
#endif
            ResourcePackageListManifest resourcePackageListManifest = _packageListManifests.Find(x => x.name == packageName);
            if (resourcePackageListManifest is not null)
            {
                return true;
            }

            string iniFilePath = OSSConfig.instance.GetFilePath($"{packageName}.ini");
            if (OSSConfig.instance.current.type == OSSType.Streaming && Application.isEditor)
            {
                resourcePackageListManifest = JsonConvert.DeserializeObject<ResourcePackageListManifest>(File.ReadAllText(iniFilePath));
            }
            else
            {
                resourcePackageListManifest = await Request.GetData<ResourcePackageListManifest>(iniFilePath);
            }

            if (resourcePackageListManifest is null)
            {
                Debug.LogError("没有找到资源包列表配置文件：" + iniFilePath);
                return false;
            }

            if (resourcePackageListManifest.appVersion.Equals(BasicConfig.instance.curEntry.version) is false)
            {
                UIMsgBox.Show(Localliztion.instance.Query("App 版本过低，请重新安装App后在使用"), () =>
                {
                    Application.OpenURL(BasicConfig.instance.apkUrl);
                    GameManager.instance.QuitGame();
                });
                throw new Exception("App 版本过低，请重新安装App后在使用");
            }

            _packageListManifests.Add(resourcePackageListManifest);
            if (resourcePackageListManifest.dependencies is not null && resourcePackageListManifest.dependencies.Count > 0)
            {
                foreach (var VARIABLE in resourcePackageListManifest.dependencies)
                {
                    await SetupPackageManifest(VARIABLE);
                }
            }

            return true;
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
        public ResourcePackageManifest GetResourcePackageManifestWithAssetName(string assetName)
        {
            foreach (var VARIABLE in _packageListManifests)
            {
                var m = VARIABLE.packages.FirstOrDefault(x => x.Contains(assetName));
                if (m is not null)
                {
                    return m;
                }
            }

            return default;
        }

        /// <summary>
        /// 获取资源的全路径
        /// </summary>
        /// <param name="moduleName"></param>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public string GetAssetFullPath(string moduleName, string assetName)
        {
            ResourcePackageListManifest resourcePackageManifest = _packageListManifests.Find(x => x.name == moduleName);
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
        public List<ResourcePackageManifest> CheckNeedUpdatePackageList(string packageName)
        {
            List<ResourcePackageManifest> needUpdatePackages = new List<ResourcePackageManifest>();
#if UNITY_EDITOR
            if (BasicConfig.instance.resMode == ResourceMode.Editor)
            {
                return needUpdatePackages;
            }
#endif

            ResourcePackageListManifest resourcePackageListManifest = _packageListManifests.Find(x => x.name == packageName);
            if (resourcePackageListManifest is null)
            {
                return needUpdatePackages;
            }


            foreach (var package in resourcePackageListManifest.packages)
            {
                if (VFSManager.instance.Exist(package.name, package.version) || needUpdatePackages.Contains(package))
                {
                    continue;
                }

                needUpdatePackages.Add(package);
            }

            if (resourcePackageListManifest.dependencies is not null && resourcePackageListManifest.dependencies.Count > 0)
            {
                foreach (string dependency in resourcePackageListManifest.dependencies)
                {
                    needUpdatePackages.AddRange(CheckNeedUpdatePackageList(dependency));
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
#if UNITY_EDITOR
            if (BasicConfig.instance.resMode == ResourceMode.Editor)
            {
                return result;
            }
#endif

            ResourcePackageListManifest resourcePackageListManifest = _packageListManifests.Find(x => x.name == packageName);
            if (resourcePackageListManifest is null)
            {
                return result;
            }

            result.AddRange(resourcePackageListManifest.packages);
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