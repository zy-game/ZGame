using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using ZGame.FileSystem;
using ZGame.Networking;
using ZGame.Resource.Config;

namespace ZGame.Resource
{
    public class PackageManifestManager : Singleton<PackageManifestManager>
    {
        private List<ResourcePackageListManifest> _manifests = new List<ResourcePackageListManifest>();

        /// <summary>
        /// 设置资源包模块
        /// </summary>
        /// <param name="config"></param>
        public async UniTask Setup(EntryConfig config)
        {
#if UNITY_EDITOR
            if (BasicConfig.instance.resMode == ResourceMode.Editor)
            {
                return;
            }
#endif


            await Setup(config, config.module);
        }

        private async UniTask Setup(EntryConfig config, string packageName)
        {
            ResourcePackageListManifest resourcePackageListManifest = _manifests.Find(x => x.name == config.module);
            if (resourcePackageListManifest is not null)
            {
                return;
            }

            string iniFilePath = OSSConfig.instance.GetFilePath(config.ossTitle, packageName + ".ini");
            if (OSSConfig.instance.GetOSSType(config.ossTitle) == OSSType.Streaming && Application.isEditor)
            {
                resourcePackageListManifest = JsonConvert.DeserializeObject<ResourcePackageListManifest>(File.ReadAllText(iniFilePath));
            }
            else
            {
                resourcePackageListManifest = await NetworkManager.GetStreamingAsset<ResourcePackageListManifest>(iniFilePath);
            }

            if (resourcePackageListManifest is null)
            {
                return;
            }

            _manifests.Add(resourcePackageListManifest);
            if (resourcePackageListManifest.dependencies is null || resourcePackageListManifest.dependencies.Count == 0)
            {
                return;
            }

            foreach (var VARIABLE in resourcePackageListManifest.dependencies)
            {
                await Setup(config, VARIABLE);
            }
        }

        public uint GetResourcePackageVersion(string packageName)
        {
            foreach (var VARIABLE in _manifests)
            {
                ResourcePackageManifest resourcePackageManifest = VARIABLE.GetPackageManifest(packageName);
                if (resourcePackageManifest is not null)
                {
                    return resourcePackageManifest.version;
                }
            }

            return 0;
        }

        /// <summary>
        /// 根据资源名查找对应的资源包
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public ResourcePackageManifest GetResourcePackageManifestWithAssetName(string assetName)
        {
            foreach (var VARIABLE in _manifests)
            {
                var m = VARIABLE.packages.FirstOrDefault(x => x.files.Contains(assetName));
                if (m is not null)
                {
                    return m;
                }
            }

            return default;
        }

        /// <summary>
        /// 获取需要更新的资源包列表
        /// </summary>
        /// <param name="packageName"></param>
        /// <returns></returns>
        public List<ResourcePackageManifest> CheckNeedUpdatePackageList(string packageName)
        {
            List<ResourcePackageManifest> needUpdatePackages = new List<ResourcePackageManifest>();
            ResourcePackageListManifest resourcePackageListManifest = _manifests.Find(x => x.name == packageName);
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
            ResourcePackageListManifest resourcePackageListManifest = _manifests.Find(x => x.name == packageName);
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