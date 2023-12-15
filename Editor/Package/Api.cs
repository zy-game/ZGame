using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Unity.EditorCoroutines.Editor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.Networking;

namespace ZGame.Editor.Package
{
    public sealed class Api
    {
        /// <summary>
        /// 获取包列表
        /// </summary>
        /// <param name="isLocal"></param>
        /// <returns></returns>
        public static Task<List<PackageData>> RequestPackageList(bool isLocal)
        {
            var _taskCompletionSource = new TaskCompletionSource<List<PackageData>>();
            List<PackageData> resultList = new List<PackageData>();

            IEnumerator GetPackageList(bool isLocal, TaskCompletionSource<List<PackageData>> _taskCompletionSource)
            {
                Request request = isLocal ? Client.List(true, true) : Client.SearchAll(false);
                yield return new WaitUntil(() => request.IsCompleted);
                if (request.Status != StatusCode.Success)
                {
                    Debug.LogError(request.Error.message);
                    yield break;
                }

                PackageInfo[] result = isLocal ? ((ListRequest)request).Result.ToArray() : ((SearchRequest)request).Result.ToArray();
                foreach (var VARIABLE in result)
                {
                    resultList.Add(PackageData.OnCreate(VARIABLE));
                }

                _taskCompletionSource.SetResult(resultList);
            }

            EditorManager.StartCoroutine(GetPackageList(isLocal, _taskCompletionSource));
            return _taskCompletionSource.Task;
        }

        public static async void OnGetPackageVersionList(string url, Action<List<string>> versionCallback)
        {
            List<string> result = await GetPackageVersionList(url);
            versionCallback?.Invoke(result);
        }

        /// <summary>
        /// 获取包版本列表
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static Task<List<string>> GetPackageVersionList(string url)
        {
            TaskCompletionSource<List<string>> taskCompletionSource = new TaskCompletionSource<List<string>>();

            IEnumerator OnStart()
            {
                if (url.StartsWith("https") is false)
                {
                    var request = Client.Search(url);
                    yield return new WaitUntil(() => request.IsCompleted);
                    if (request.Status != StatusCode.Success)
                    {
                        Debug.LogError(request.Error.message);
                        yield break;
                    }

                    PackageInfo packageInfo = request.Result.First();
                    taskCompletionSource.SetResult(packageInfo.versions.all.Where(x => x.Contains("pre") is false && x.Contains("dev") is false && x.Contains("ex") is false).ToList());
                    yield break;
                }

                string[] paths = url.Split('/');
                string repo = Path.GetFileNameWithoutExtension(paths[paths.Length - 1].Split("#")[0]);
                string owner = paths[paths.Length - 2];
                url = url.StartsWith("https://github") ? $"https://api.github.com/repos/{owner}/{repo}/tags" : $"https://gitee.com/api/v5/repos/{owner}/{repo}/tags?sort=updated&direction=desc&page=1&per_page=10";
                Debug.Log(url);
                UnityWebRequest request2 = UnityWebRequest.Get(url);
                request2.timeout = 10;
                request2.useHttpContinue = true;
                yield return request2.SendWebRequest();
                if (request2.isNetworkError || request2.isHttpError || request2.result is not UnityWebRequest.Result.Success)
                {
                    Debug.LogError(request2.error);
                    taskCompletionSource.SetResult(new List<string>());
                    yield break;
                }

                GitData[] packages = JsonConvert.DeserializeObject<GitData[]>(request2.downloadHandler.text);
                taskCompletionSource.SetResult(packages.Select(x => x.name).Where(x => x.Contains("pre") is false && x.Contains("dev") is false && x.Contains("ex") is false).ToList());
            }

            EditorManager.StartCoroutine(OnStart());
            return taskCompletionSource.Task;
        }

        /// <summary>
        /// 刷新包版本列表
        /// </summary>
        /// <param name="packages"></param>
        /// <returns></returns>
        public static async Task<StatusCode> RefreshPackageVersionList(params PackageData[] packages)
        {
            if (packages.Length == 0)
            {
                return StatusCode.Success;
            }

            foreach (var VARIABLE in packages)
            {
                List<string> versions = await GetPackageVersionList(VARIABLE.url);
                if (versions is null || versions.Count == 0)
                {
                    continue;
                }

                if (VARIABLE.recommended.IsNullOrEmpty())
                {
                    VARIABLE.versions = versions;
                    continue;
                }

                VARIABLE.versions = new List<string>();
                for (int i = 0; i < versions.Count; i++)
                {
                    if (versions[i].Contains("pre") || versions[i].Contains("exp"))
                    {
                        continue;
                    }

                    if (versions[i] == VARIABLE.recommended)
                    {
                        VARIABLE.versions.Add(versions[i]);
                        break;
                    }

                    VARIABLE.versions.Add(versions[i]);
                }
            }


            return StatusCode.Success;
        }

        /// <summary>
        /// 移除包
        /// </summary>
        /// <param name="packages"></param>
        /// <returns></returns>
        public static Task<StatusCode> Remove(PackageData packages)
        {
            if (PackageDataList.instance.LocalIsInstalled(packages.name, packages.version) is false)
            {
                return Task.FromResult(StatusCode.Failure);
            }

            TaskCompletionSource<StatusCode> taskCompletionSource = new TaskCompletionSource<StatusCode>();

            IEnumerator OnStart()
            {
                RemoveRequest request = Client.Remove(packages.name);
                yield return new WaitUntil(() => request.IsCompleted);

                if (request.Status != StatusCode.Success)
                {
                    Debug.LogError(request.Error.message);
                    taskCompletionSource.SetResult(StatusCode.Failure);
                    yield break;
                }

                RemoveDependencies(packages.dependenceis);
            }

            async void RemoveDependencies(DependencyInfo[] infos)
            {
                if (infos is not null && infos.Length > 0)
                {
                    for (int i = 0; i < infos.Length; i++)
                    {
                        List<PackageData> dependList = PackageDataList.instance.GetDependencyList(infos[i].name);
                        if (dependList.Count > 1)
                        {
                            continue;
                        }

                        await Remove(PackageDataList.instance.GetPackageData(infos[i].name));
                    }
                }

                packages.CloseWaiting();
                taskCompletionSource.SetResult(StatusCode.Success);
            }

            packages.ShowWaiting();
            EditorManager.StartCoroutine(OnStart());
            return taskCompletionSource.Task;
        }

        /// <summary>
        /// 添加包
        /// </summary>
        /// <param name="name"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public static Task<List<PackageData>> Add(string name, string version)
        {
            if (PackageDataList.instance.LocalIsInstalled(name, version))
            {
                return Task.FromResult(new List<PackageData>());
            }

            TaskCompletionSource<List<PackageData>> taskCompletionSource = new TaskCompletionSource<List<PackageData>>();
            string url = name.StartsWith("com.") ? string.Format("{0}@{1}", name, version) : string.Format("{0}#{1}", name, version);
            List<PackageData> result = new List<PackageData>();

            PackageData packageData = PackageDataList.instance.remotePackages.Find(x => x.name == name);
            if (packageData == null)
            {
                PackageDataList.instance.remotePackages.Add(packageData = PackageData.OnCreate(name));
            }

            packageData.ShowWaiting();

            IEnumerator OnStart()
            {
                AddRequest request = Client.Add(url);
                yield return new WaitUntil(() => request.IsCompleted);
                if (request.Status != StatusCode.Success)
                {
                    Debug.LogError(request.Error.message);
                    taskCompletionSource.SetResult(null);
                    packageData.CloseWaiting();
                    yield break;
                }

                InstallDependenciesPackageList(request.Result, request.Result.dependencies);
            }

            async void InstallDependenciesPackageList(PackageInfo package, DependencyInfo[] dependencies)
            {
                if (dependencies is not null && dependencies.Length > 0)
                {
                    foreach (var VARIABLE in dependencies)
                    {
                        List<PackageData> packages = await Add(VARIABLE.name, VARIABLE.version);
                        if (packages is null)
                        {
                            continue;
                        }

                        result.AddRange(packages);
                    }
                }

                result.Add(PackageData.OnCreate(package));
                taskCompletionSource.SetResult(result);
                packageData.CloseWaiting();
                PackageDataList.instance.remotePackages.Remove(packageData);
                taskCompletionSource.SetResult(result);
            }

            EditorManager.StartCoroutine(OnStart());
            return taskCompletionSource.Task;
        }

        /// <summary>
        /// 更新包
        /// </summary>
        /// <param name="name"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public static Task<StatusCode> Update(string name, string version)
        {
            TaskCompletionSource<StatusCode> taskCompletionSource = new TaskCompletionSource<StatusCode>();
            string url = name.StartsWith("com.") ? string.Format("{0}@{1}", name, version) : string.Format("{0}#{1}", name, version);
            PackageData packageData = PackageDataList.instance.packages.Find(x => x.name == name);

            IEnumerator OnStart()
            {
                AddRequest request = Client.Add(url);
                yield return new WaitUntil(() => request.IsCompleted);
                taskCompletionSource.SetResult(request.Status);
                packageData?.CloseWaiting();
            }

            packageData?.ShowWaiting();
            EditorManager.StartCoroutine(OnStart());
            return taskCompletionSource.Task;
        }
    }
}