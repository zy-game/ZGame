using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Serialization;


namespace ZGame.Editor.Package
{
    [ResourceReference("Assets/Settings/PackageManager.asset")]
    public class PackageDataList : SingletonScriptableObject<PackageDataList>
    {
        /// <summary>
        /// 是否自动安装缺省的包
        /// </summary>
        public bool isAutoInstalled;

        /// <summary>
        /// 当前安装列表
        /// </summary>
        public List<PackageData> packages;

        [NonSerialized] public List<PackageData> localPackages;
        [NonSerialized] public List<PackageData> remotePackages;
        [NonSerialized] public bool isInit;

        protected override void OnAwake()
        {
            packages = packages ?? new List<PackageData>();
            localPackages = new List<PackageData>();
            remotePackages = new List<PackageData>();
        }

        /// <summary>
        /// 刷新包列表
        /// </summary>
        /// <param name="callback"></param>
        public async void Refresh(Action callback)
        {
            if (packages == null)
            {
                packages = new List<PackageData>();
            }

            remotePackages = await Api.RequestPackageList(false);
            localPackages = await Api.RequestPackageList(true);
            foreach (var VARIABLE in localPackages)
            {
                if (packages.Exists(x => x.name == VARIABLE.name))
                {
                    continue;
                }

                packages.Add(VARIABLE);
            }

            await Api.RefreshPackageVersionList(packages.ToArray());

            if (isAutoInstalled)
            {
                foreach (var VARIABLE in packages)
                {
                    if (localPackages.Exists(x => x.name == VARIABLE.name && x.version == VARIABLE.version))
                    {
                        continue;
                    }

                    await Api.Add(VARIABLE.name, VARIABLE.version);
                }
            }

            isInit = true;
            callback?.Invoke();
        }

        /// <summary>
        /// 移除包
        /// </summary>
        /// <param name="info"></param>
        public async void Remove(PackageData info)
        {
            if (await Api.Remove(info) is not StatusCode.Success)
            {
                return;
            }

            packages.Remove(info);
            localPackages.Remove(info);
        }

        /// <summary>
        /// 安装包
        /// </summary>
        /// <param name="name"></param>
        /// <param name="version"></param>
        public async void Install(string name, string version)
        {
            List<PackageData> packageData = await Api.Add(name, version);
            if (packageData is null)
            {
                return;
            }

            foreach (var VARIABLE in packageData)
            {
                PackageData temp = packages.Find(x => x.name == VARIABLE.name);
                if (temp == null)
                {
                    continue;
                }

                remotePackages.Remove(temp);
            }

            packages.AddRange(packageData);
            localPackages.AddRange(packageData);
        }

        /// <summary>
        /// 本地是否安装
        /// </summary>
        /// <param name="name"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public bool LocalIsInstalled(string name, string version)
        {
            if (localPackages == null)
            {
                return false;
            }

            return localPackages.Exists(x => x.name == name);
        }

        /// <summary>
        /// 更新包
        /// </summary>
        /// <param name="name"></param>
        /// <param name="version"></param>
        public async void OnUpdate(string name, string version)
        {
            if (await Api.Update(name, version) is not StatusCode.Success)
            {
                return;
            }

            await Api.RefreshPackageVersionList(packages.ToArray());
        }

        /// <summary>
        /// 获取所有引用包
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public List<PackageData> GetDependencyList(string s)
        {
            return packages.Where(x => x.name == s).ToList();
        }

        /// <summary>
        /// 查找包
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public PackageData GetPackageData(string s)
        {
            return packages.Find(x => x.name == s);
        }
    }
}