using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using ZGame.Config;
using ZGame.Language;
using ZGame.UI;

namespace ZGame.Resource
{
    public class PackageManifestManager : GameManager
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
        public async UniTask<Status> SetPackageManifest(string packageName)
        {
            ResourcePackageListManifest resourcePackageListManifest = _packageListManifests.Find(x => x.name == packageName);
            if (resourcePackageListManifest is not null)
            {
                return Status.Success;
            }

            string iniFilePath = AppCore.GetFileUrl($"{packageName}.ini");
            if (iniFilePath.IsNullOrEmpty())
            {
                return Status.Fail;
            }
#if !UNITY_EDITOR
            resourcePackageListManifest = JsonConvert.DeserializeObject<ResourcePackageListManifest>(await AppCore.Network.GetData(iniFilePath));
#else
            if (AppCore.resMode == ResourceMode.Editor)
            {
                resourcePackageListManifest = GetPackageManifest(packageName);
            }
            else
            {
                resourcePackageListManifest = JsonConvert.DeserializeObject<ResourcePackageListManifest>(await AppCore.Network.GetData(iniFilePath));
            }
#endif
            if (resourcePackageListManifest is null)
            {
                AppCore.Logger.LogError("没有找到资源包列表配置文件：" + packageName);
                return Status.Fail;
            }

            _packageListManifests.Add(resourcePackageListManifest);
            if (resourcePackageListManifest.dependencies is not null && resourcePackageListManifest.dependencies.Count > 0)
            {
                foreach (var VARIABLE in resourcePackageListManifest.dependencies)
                {
                    await SetPackageManifest(VARIABLE);
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

            return _packageListManifests.Select(x => x.packages.FirstOrDefault(x => x.name == packageName)).FirstOrDefault();
        }


        /// <summary>
        /// 根据资源名查找对应的资源包
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public bool TryGetManifest(string assetName, out ResourcePackageManifest manifest)
        {
            foreach (var VARIABLE in _packageListManifests)
            {
                if (VARIABLE.TryGetPackageManifestWithAssetName(assetName, out manifest))
                {
                    return true;
                }
            }

            manifest = default;
            return false;
        }

        /// <summary>
        /// 获取资源的全路径
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public bool TryGetAssetFullPath(string assetName, out string fullPath)
        {
            if (assetName.IsNullOrEmpty())
            {
                throw new NullReferenceException(nameof(assetName));
            }

            if (assetName.StartsWith("Assets") || assetName.StartsWith("Resources"))
            {
                fullPath = assetName;
                return true;
            }

            fullPath = String.Empty;
            foreach (var VARIABLE in _packageListManifests)
            {
                if (VARIABLE.TryGetFilePath(assetName, out fullPath))
                {
                    break;
                }
            }

            return fullPath.IsNullOrEmpty() is false;
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
            if (AppCore.resMode == ResourceMode.Editor)
            {
                return result;
            }
#endif
            ResourcePackageListManifest resourcePackageListManifest = _packageListManifests.Find(x => x.name == packageName);
            if (resourcePackageListManifest is null)
            {
                AppCore.Logger.Log("没有找到资源包列表配置文件：" + packageName);
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

        public static ResourcePackageListManifest LoadOrCreate(string name)
        {
            ResourcePackageListManifest packageListManifest = null;
            string fileName = name + ".ini";
            if (File.Exists(AppCore.GetFileOutputPath(fileName)) is false)
            {
                packageListManifest = new ResourcePackageListManifest()
                {
                    name = name,
                    version = 0,
                    dependencies = new List<string>(),
                    packages = new List<ResourcePackageManifest>(),
                    appVersion = Application.version
                };
            }
            else
            {
                string json = File.ReadAllText(AppCore.GetFileOutputPath(fileName));
                packageListManifest = JsonConvert.DeserializeObject<ResourcePackageListManifest>(json);
            }

            return packageListManifest;
        }

        public static void Save(params ResourcePackageListManifest[] packageListManifest)
        {
            foreach (var VARIABLE in packageListManifest)
            {
                File.WriteAllText(AppCore.GetFileOutputPath(VARIABLE.name + ".ini"), JsonConvert.SerializeObject(VARIABLE));
            }
        }

        public static ResourcePackageListManifest GetPackageManifest(string name)
        {
            return GetAllPackageManifests().Find(x => x.name == name);
        }


        public static List<ResourcePackageListManifest> GetAllPackageManifests()
        {
            List<ResourcePackageListManifest> optionsList = new List<ResourcePackageListManifest>();
#if UNITY_EDITOR
            var ids = UnityEditor.AssetDatabase.FindAssets($"t:{nameof(ResourcePackageListManifest)}");
            foreach (var id in ids)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(id);
                optionsList.Add(UnityEditor.AssetDatabase.LoadAssetAtPath<ResourcePackageListManifest>(path));
            }
#endif
            return optionsList;
        }

        public static List<string> GetAllManifestNames()
        {
            return GetAllPackageManifests().Select(x => x.name).ToList();
        }
    }
}