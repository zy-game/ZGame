using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.Networking;

namespace ZGame.Editor.Package
{
    public class ProjectPackageDataList : ScriptableObject
    {
        [NonSerialized] public bool isInit;
        public List<ProjectPackageData> packages;

        public void Init(Action callback)
        {
            if (packages == null)
            {
                packages = new List<ProjectPackageData>();
            }

            EditorManager.StartCoroutine(OnStartReferencePackageList(callback));
        }

        private IEnumerator OnStartReferencePackageList(Action callback)
        {
            Func<ProjectPackageData, bool> exist = y => packages.Exists(x => x.name == y.name) is false;
            yield return OnGetUnityPackageList(remoteList => { packages.AddRange(remoteList.Where(x => exist(x))); });
            yield return GetLocalAllPackageList(localList => { packages.AddRange(localList.Where(x => exist(x))); });
            yield return OnReferenceVersionList();
            isInit = true;
            callback?.Invoke();
        }

        public bool Contains(string name)
        {
            if (packages is null)
            {
                return false;
            }

            return packages.Exists(x => x.name == name);
        }

        public void Remove(string name)
        {
            if (packages == null)
            {
                return;
            }

            ProjectPackageData info = packages.Find(x => x.name == name);
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

        public void UpdateAll()
        {
            if (packages is null)
            {
                return;
            }

            packages.ForEach(x => OnUpdate(x.name, x.latest));
        }

        public void OnUpdate(string name, string version)
        {
            if (packages is null)
            {
                return;
            }

            string url = String.Empty;
            ProjectPackageData info = packages.Find(x => x.name == name);

            IEnumerator OnStart()
            {
                //"https://github.example.com/myuser/myrepository.git#9e72f9d5a6a3dadc38d813d8399e1b0e86781a49"

                if (info == null)
                {
                    if (version.IsNullOrEmpty())
                    {
                        url = name;
                    }
                    else
                    {
                        url = name.StartsWith("http") ? string.Format("{0}#{1}", name, version) : string.Format("{0}@{1}", name, version);
                    }
                }
                else
                {
                    if (info.gitList == null)
                    {
                        url = version.IsNullOrEmpty() ? info.name : string.Format("{0}@{1}", info.name, version);
                    }
                    else
                    {
                        if (info.gitList.ContainsKey(version))
                        {
                            version = info.gitList[version];
                        }

                        url = string.Format("{0}#{1}", info.url.Split('@')[1], version);
                    }
                }

                Debug.Log(url);
                var request = Client.Add(url);
                yield return new WaitUntil(() => request.IsCompleted);
                if (request.Status == StatusCode.Success)
                {
                    Debug.Log(string.Format("Update {0} success", info.name));
                    if (info is null)
                    {
                        info = new ProjectPackageData();
                        info.name = request.Result.name;

                        info.url = request.Result.packageId;
                        packages.Add(info);
                    }

                    info.version = version.IsNullOrEmpty() ? request.Result.version : version;
                }
                else
                {
                    Debug.LogError(string.Format("Update {0} failed", info.name));
                }
            }

            EditorManager.StartCoroutine(OnStart());
        }

        public void Remove(ProjectPackageData info)
        {
            if (packages == null)
            {
                return;
            }

            if (Contains(info.name) is false)
            {
                return;
            }

            IEnumerator OnStart(ProjectPackageData info)
            {
                var request = Client.Remove(info.name);
                yield return new WaitUntil(() => request.IsCompleted);
                if (request.Status == StatusCode.Success)
                {
                    Debug.Log(string.Format("Remove {0} success", info.name));
                }
                else
                {
                    Debug.LogError(string.Format("Remove {0} failed", info.name));
                }
            }

            EditorManager.StartCoroutine(OnStart(info));
        }

        IEnumerator OnGetUnityPackageList(Action<List<ProjectPackageData>> callback)
        {
            List<ProjectPackageData> packageDataList = new List<ProjectPackageData>();
            var request = Client.SearchAll();
            yield return new WaitUntil(() => request.IsCompleted);
            if (request.Status == StatusCode.Success)
            {
                foreach (var VARIABLE in request.Result)
                {
                    ProjectPackageData packageData = new ProjectPackageData();
                    packageData.name = VARIABLE.name;
                    packageData.version = VARIABLE.version;
                    packageData.url = VARIABLE.packageId;
                    packageData.state = PackageState.None;
                    packageData.latest = VARIABLE.versions.latest;
                    packageData.versions = VARIABLE.versions.all.ToList();
                    packageDataList.Add(packageData);
                }
            }

            callback?.Invoke(packageDataList);
        }


        IEnumerator GetLocalAllPackageList(Action<List<ProjectPackageData>> callback)
        {
            List<ProjectPackageData> packageDataList = new List<ProjectPackageData>();
            var cacheRequest = Client.List();
            yield return new WaitUntil(() => cacheRequest.IsCompleted);
            if (cacheRequest.Status == StatusCode.Success)
            {
                foreach (var VARIABLE in cacheRequest.Result)
                {
                    ProjectPackageData packageData = new ProjectPackageData();
                    packageData.name = VARIABLE.name;
                    packageData.version = VARIABLE.version;
                    packageData.url = VARIABLE.packageId;
                    packageData.state = PackageState.None;
                    packageData.latest = VARIABLE.versions.latest;
                    packageData.versions = VARIABLE.versions.all.ToList();
                    packageDataList.Add(packageData);
                }
            }

            callback?.Invoke(packageDataList);
        }

        public IEnumerator OnReferenceVersionList()
        {
            string result = String.Empty;
            foreach (var packageData in packages)
            {
                if (packageData.url.Split("@")[1].StartsWith("http") is false)
                {
                    var request = Client.Search(packageData.name);
                    yield return new WaitUntil(() => request.IsCompleted);
                    if (request.Status != StatusCode.Success)
                    {
                        Debug.LogError(request.Error.message);
                        yield break;
                    }

                    packageData.latest = request.Result.First()?.versions.latest;
                    packageData.versions = request.Result.First()?.versions.all.ToList();
                    continue;
                }


                string[] paths = packageData.url.Split('/');
                string repo = Path.GetFileNameWithoutExtension(paths[paths.Length - 1]);
                string owner = paths[paths.Length - 2];
                yield return OnGetVersion(owner, repo, s => result = s);
                if (result.IsNullOrEmpty())
                {
                    yield break;
                }

                GitPackage[] packages = JsonConvert.DeserializeObject<GitPackage[]>(result);
                if (packages.Length > 0)
                {
                    packageData.latest = packages[0].name;
                    packageData.versions = packages.Select(x => x.name).ToList();
                    packageData.gitList = packages.ToDictionary(x => x.name, x => x.commit.sha);
                }
            }
        }

        static IEnumerator OnRequest(string url, Action<string> callback)
        {
            Debug.Log(url);
            UnityWebRequest request = UnityWebRequest.Get(url);
            yield return request.SendWebRequest();
            if (request.isNetworkError)
            {
                Debug.LogError(request.error);
            }
            else
            {
                callback?.Invoke(request.downloadHandler.text);
            }
        }

        static IEnumerator OnGetVersion(string owner, string repo, Action<string> callback)
        {
            string result = String.Empty;
            yield return OnRequest($"https://api.github.com/repos/{owner}/{repo}/tags", s => result = s);
            if (result.IsNullOrEmpty())
            {
                yield return OnRequest($"https://api.github.com/repos/{owner}/{repo}/releases", s => result = s);
            }

            if (result.IsNullOrEmpty())
            {
                yield return OnRequest($"https://gitee.com/api/v5/repos/{owner}/{repo}/releases?page=1&per_page=10", s => result = s);
            }

            if (result.IsNullOrEmpty())
            {
                yield return OnRequest($"https://gitee.com/api/v5/repos/{owner}/{repo}/tags?sort=updated&direction=asc&page=1&per_page=10", s => result = s);
            }

            callback?.Invoke(result);
        }
    }
}