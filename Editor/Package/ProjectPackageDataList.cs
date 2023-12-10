using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace ZGame.Editor.Package
{
    public class ProjectPackageDataList : ScriptableObject
    {
        [NonSerialized] public bool isInit;
        public List<ProjectPackageData> packages;

        public IEnumerator Init(Action callback)
        {
            if (packages == null)
            {
                packages = new List<ProjectPackageData>();
            }

            yield return RequestInstalledPackageList();
            yield return InstalledDeletionPackageList();
            callback?.Invoke();
        }

        private IEnumerator InstalledDeletionPackageList()
        {
            foreach (var VARIABLE in packages)
            {
                if (VARIABLE.installed)
                {
                    continue;
                }

                Debug.Log(string.Format("安装：{0}", VARIABLE.name));
            }

            yield break;
        }

        private IEnumerator RequestInstalledPackageList()
        {
            var request = Client.List();
            yield return request;
            yield return new WaitUntil(() => request.IsCompleted);
            isInit = true;
            if (request.Status == StatusCode.Failure)
            {
                Debug.LogError("Request list failed");
                yield break;
            }

            Debug.Log("Request list success" + request.Result.Count());
            if (request.Status == StatusCode.Success)
            {
                foreach (var VARIABLE in request.Result)
                {
                    ProjectPackageData packageData = packages.Find(x => x.name == VARIABLE.name);
                    if (packageData is null)
                    {
                        packageData = new ProjectPackageData();
                        packages.Add(packageData);
                    }

                    packageData.installed = true;
                    packageData.name = VARIABLE.name;
                    packageData.version = VARIABLE.version;
                    packageData.url = VARIABLE.packageId;
                    yield return packageData.CheckUpdate();
                }
            }
        }

        public bool Contains(string name)
        {
            if (packages is null)
            {
                return false;
            }

            return packages.Exists(x => x.name == name);
        }

        public void Add(ProjectPackageData info)
        {
            if (packages == null)
            {
                packages = new List<ProjectPackageData>();
            }

            if (Contains(info.name))
            {
                Debug.LogError(string.Format("{0} is already exists", info.name));
                return;
            }

            IEnumerator OnStart()
            {
                yield return info.Update((state) =>
                {
                    if (state is false)
                    {
                        return;
                    }

                    packages.Add(info);
                    EditorUtility.SetDirty(this);
                });
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

            IEnumerator OnStart()
            {
                yield return info.Remove(state =>
                {
                    if (state is false)
                    {
                        return;
                    }

                    packages.Remove(info);
                    EditorUtility.SetDirty(this);
                });
            }

            EditorManager.StartCoroutine(OnStart());
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

        public void UpdatePackage(string name)
        {
            if (packages is null)
            {
                return;
            }

            ProjectPackageData info = packages.Find(x => x.name == name);
            if (info is null)
            {
                return;
            }

            EditorManager.StartCoroutine(info.Update(null));
        }

        public void UpdateAll()
        {
            if (packages is null)
            {
                return;
            }


            packages.ForEach(x => x.Update(null));
        }
    }
}