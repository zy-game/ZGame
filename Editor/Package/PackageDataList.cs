using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Serialization;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;


namespace ZGame.Editor.Package
{
    public class InstallPackageRunnableHandle : IRunnableHandle<PackageData>
    {
        private TaskCompletionSource<PackageData> _taskCompletionSource;

        public InstallPackageRunnableHandle()
        {
            _taskCompletionSource = new TaskCompletionSource<PackageData>();
        }

        public void Dispose()
        {
        }

        public Task<PackageData> Execute(params object[] args)
        {
            return _taskCompletionSource.Task;
        }
    }

    public class UpdatePackageRunnableHandle : IRunnableHandle<bool>
    {
        public Task<bool> Execute(params object[] args)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }

    public class UninstallPackageRunnableHandle : IRunnableHandle<bool>
    {
        public Task<bool> Execute(params object[] args)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }

    public class InitializedPackageListRunnableHandle : IRunnableHandle<bool>
    {
        public Task<bool> Execute(params object[] args)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }

    [ResourceReference("Assets/Settings/PackageManager")]
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


        protected override void OnSaved()
        {
            packages = packages.Where(x => x.installed == InstallState.Install || x.installed == InstallState.None).ToList();
        }

        public void ReferncePackageList(Action callback)
        {
            if (packages == null)
            {
                packages = new List<PackageData>();
            }

            EditorManager.StartCoroutine(OnStartReferencePackageList(callback));
        }


        public void Remove(string name)
        {
            if (packages == null)
            {
                return;
            }

            PackageData info = packages.Find(x => x.name == name);
            if (info == null)
            {
                return;
            }

            Remove(info);
        }

        public void Clear()
        {
            if (packages == null)
            {
                return;
            }

            packages.ForEach(Remove);
            packages.Clear();
        }

        public async void Remove(PackageData info)
        {
            if (packages == null)
            {
                return;
            }

            UninstallPackageRunnableHandle uninstallPackageRunnableHandle = new UninstallPackageRunnableHandle();
            bool state = await uninstallPackageRunnableHandle.Execute(info);
            if (state is false)
            {
                return;
            }

            packages.Remove(info);
            localPackages.Remove(info);


            // IEnumerator OnStart(PackageData info)
            // {
            //     var request = Client.Remove(info.name);
            //     yield return new WaitUntil(() => request.IsCompleted);
            //     if (request.Status != StatusCode.Success)
            //     {
            //         Debug.LogError(string.Format("Remove {0} failed", info.name));
            //         yield break;
            //     }
            //
            //     info.installed = InstallState.Uninstall;
            //     packages.Remove(info);
            //     localPackages.Remove(info);
            //     Debug.Log(string.Format("Remove {0} success", info.name));
            //     OnSave();
            //     EditorManager.Refresh();
            // }
            //
            // EditorManager.StartCoroutine(OnStart(info));
        }

        public async void Install(string name, string version)
        {
            InstallPackageRunnableHandle installPackageRunnableHandle = new InstallPackageRunnableHandle();
            PackageData packageData = await installPackageRunnableHandle.Execute(name, version);
            if (packageData is null)
            {
                return;
            }

            packages.Add(packageData);
            localPackages.Add(packageData);
        }

        public async void OnUpdate(string name, string version)
        {
            if (packages is null)
            {
                return;
            }

            UpdatePackageRunnableHandle updatePackageRunnableHandle = new UpdatePackageRunnableHandle();
            bool state = await updatePackageRunnableHandle.Execute(name, version);
            if (state is false)
            {
                return;
            }
            
            
            EditorManager.StartCoroutine(OnInstalledPackage(name, version, state =>
            {
                OnSave();
                ReferncePackageList(() => { });
            }));
        }

        IEnumerator OnInstalledPackage(string name, string version, Action<bool> callback)
        {
            if (localPackages.Exists(x => x.name == name && x.version == version))
            {
                callback(true);
                yield break;
            }

            PackageData packageData = remotePackages.Find(x => x.name == name);
            if (packageData is null)
            {
                packageData = new PackageData();
                packageData.title = name;
                remotePackages.Add(packageData);
            }

            packageData.installed = InstallState.Installeding;
            string url = name.StartsWith("com.") ? string.Format("{0}@{1}", name, version) : string.Format("{0}#{1}", name, version);
            var request = Client.Add(url);
            yield return new WaitUntil(() => request.IsCompleted);
            if (request.Status != StatusCode.Success)
            {
                Debug.LogError(string.Format("Update {0} failed:{1}", name, request.Error.message));
                callback?.Invoke(false);
                if (url.StartsWith("https"))
                {
                    remotePackages.Remove(packageData);
                }

                yield break;
            }

            packageData.name = request.Result.name;
            packageData.title = request.Result.displayName;
            foreach (var VARIABLE in request.Result.dependencies)
            {
                if (packages.Exists(x => x.name == VARIABLE.name))
                {
                    continue;
                }

                bool success = false;
                yield return OnInstalledPackage(VARIABLE.name, VARIABLE.version, state => success = state);
                if (success is false)
                {
                    yield break;
                }

                yield return new WaitForSeconds(0.1f);
            }

            packageData.url = name;
            packageData.recommended = request.Result.versions.recommended;
            packageData.installed = InstallState.Install;
            packageData.version = version;
            if (packages.Exists(x => x.name == packageData.name) is false)
            {
                packages.Add(packageData);
            }

            if (localPackages.Exists(x => x.name == packageData.name) is false)
            {
                localPackages.Add(packageData);
            }

            remotePackages.Remove(packageData);
            Debug.Log(string.Format("Update {0} success", name));
            callback?.Invoke(true);
        }

        private IEnumerator OnStartReferencePackageList(Action callback)
        {
            localPackages = new List<PackageData>();
            remotePackages = new List<PackageData>();
            yield return OnGetUnityPackageList();
            yield return GetLocalAllPackageList();
            yield return ReferencePackageState();
            isInit = true;
            callback?.Invoke();
        }

        IEnumerator OnGetUnityPackageList()
        {
            var request = Client.SearchAll();
            yield return new WaitUntil(() => request.IsCompleted);
            if (request.Status == StatusCode.Success)
            {
                foreach (var VARIABLE in request.Result)
                {
                    var packageData = PackageData.OnCreate(VARIABLE);
                    packageData.installed = InstallState.Uninstall;
                    remotePackages.Add(packageData);
                }
            }
        }

        IEnumerator GetLocalAllPackageList()
        {
            var cacheRequest = Client.List();
            yield return new WaitUntil(() => cacheRequest.IsCompleted);
            if (cacheRequest.Status is not StatusCode.Success)
            {
                yield break;
            }

            foreach (var VARIABLE in cacheRequest.Result)
            {
                var packageData = PackageData.OnCreate(VARIABLE);
                packageData.installed = InstallState.Install;
                localPackages.Add(packageData);
                if (packages.Exists(x => x.name == VARIABLE.name) is false)
                {
                    packages.Add(packageData);
                }
            }
        }

        /// <summary>
        /// 刷新代码包状态
        /// </summary>
        /// <returns></returns>
        public IEnumerator ReferencePackageState()
        {
            foreach (var packageData in packages)
            {
                //检查本地是否安装了指定包
                if (localPackages.Exists(x => x.name == packageData.name) is false)
                {
                    packageData.installed = InstallState.Uninstall;
                }

                if (packageData.installed != InstallState.Install)
                {
                    continue;
                }

                yield return GetPackageVersionList(packageData.url, versionList =>
                {
                    versionList = versionList.Where(x => x.Contains("pre") is false && x.Contains("ex") is false).ToList();
                    if (packageData.recommended.IsNullOrEmpty())
                    {
                        packageData.versions = versionList.ToList();
                    }
                    else
                    {
                        packageData.versions = new List<string>();
                        for (int i = 0; i < versionList.Count; i++)
                        {
                            if (versionList[i] == packageData.recommended)
                            {
                                return;
                            }

                            packageData.versions.Add(versionList[i]);
                        }
                    }
                });
                string targetVersion = String.Empty;
                if (packageData.recommended.IsNullOrEmpty())
                {
                    targetVersion = packageData.versions.FirstOrDefault();
                }
                else
                {
                    targetVersion = packageData.recommended;
                }

                if (targetVersion.IsNullOrEmpty())
                {
                    packageData.state = PackageState.None;
                    continue;
                }

                packageData.state = targetVersion.EndsWith(packageData.version) ? PackageState.None : PackageState.Update;
            }
        }

        public void RequestVersionList(string url, Action<List<string>> callback)
        {
            EditorManager.StartCoroutine(GetPackageVersionList(url, callback));
        }

        private IEnumerator GetPackageVersionList(string url, Action<List<string>> callback)
        {
            if (url.StartsWith("https") is false)
            {
                var request = Client.Search(url);
                yield return new WaitUntil(() => request.IsCompleted);
                if (request.Status != StatusCode.Success)
                {
                    Debug.LogError(request.Error.message);
                    callback?.Invoke(new List<string>());
                    yield break;
                }

                callback?.Invoke(request.Result.First()?.versions.all.ToList());
                yield break;
            }

            string[] paths = url.Split('/');
            string repo = Path.GetFileNameWithoutExtension(paths[paths.Length - 1].Split("#")[0]);
            string owner = paths[paths.Length - 2];
            if (url.StartsWith("https://github"))
            {
                url = $"https://api.github.com/repos/{owner}/{repo}/tags";
            }
            else if (url.StartsWith("https://gitee"))
            {
                url = $"https://gitee.com/api/v5/repos/{owner}/{repo}/tags?sort=updated&direction=desc&page=1&per_page=10";
            }

            Debug.Log(url);
            UnityWebRequest request2 = UnityWebRequest.Get(url);
            yield return request2.SendWebRequest();
            if (request2.isNetworkError || request2.isHttpError || request2.result is not UnityWebRequest.Result.Success)
            {
                Debug.LogError(request2.error);
                callback?.Invoke(new List<string>());
                yield break;
            }

            GitData[] packages = JsonConvert.DeserializeObject<GitData[]>(request2.downloadHandler.text);
            callback?.Invoke(packages.Select(x => x.name).ToList());
        }
    }
}