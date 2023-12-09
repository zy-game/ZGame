using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.Serialization;

namespace ZGame.Editor.Package
{
    [Serializable]
    public class ProjectPackageData
    {
        public string name;
        public string version;
        public string url;

        public string lastVersion;
        [NonSerialized] public List<string> allVersions;
        [NonSerialized] public bool installed;
        [NonSerialized] public bool canUpdate;

        public IEnumerator CheckUpdate()
        {
            if (url.IsNullOrEmpty())
            {
                throw new Exception("name is null");
            }

            //https://api.github.com/repos/owner/repo/tags
            //https://api.github.com/repos/owner/repo/releases
            //https://gitee.com/api/v5/repos/focus-creative-games/hybridclr_unity/releases?page=1&per_page=20
            //https://gitee.com/api/v5/repos/focus-creative-games/hybridclr_unity/tags?sort=name&direction=asc&page=1&per_page=100
            SearchRequest searchRequest = Client.Search(url);
            yield return searchRequest;
            yield return new WaitUntil(() => searchRequest.IsCompleted);

            Debug.Log(JsonConvert.SerializeObject(searchRequest));
            if (searchRequest.Status == StatusCode.Success)
            {
                var temp = searchRequest.Result.FirstOrDefault(x => x.name == name);
                canUpdate = !temp.versions.latest.Equals(version);
                lastVersion = temp.versions.latest;
            }
        }

        public IEnumerator Update(Action<bool> callback)
        {
            if (name.IsNullOrEmpty())
            {
                throw new Exception("name is null");
            }

            AddRequest request = default;
            if (url.IsNullOrEmpty())
            {
                if (version.IsNullOrEmpty())
                {
                    request = Client.Add(name);
                }
                else
                {
                    request = Client.Add(string.Format("{0}@{1}", name, version));
                }
            }
            else
            {
                request = Client.Add(url);
            }

            yield return request;
            yield return new WaitUntil(() => request.IsCompleted);
            if (request.Status == StatusCode.Success)
            {
                Debug.Log(string.Format("Add {0} success", name));
                callback?.Invoke(true);
                this.version = request.Result.version;
            }
            else
            {
                Debug.LogError(string.Format("Add {0} failed", name));
                callback?.Invoke(false);
            }
        }

        public IEnumerator Remove(Action<bool> callback)
        {
            RemoveRequest request = Client.Remove(name);
            yield return request;
            yield return new WaitUntil(() => request.IsCompleted);
            if (request.Status == StatusCode.Success)
            {
                Debug.Log(string.Format("Remove {0} success", name));
                callback?.Invoke(true);
            }
            else
            {
                Debug.LogError(string.Format("Remove {0} failed", name));
                callback?.Invoke(false);
            }
        }
    }
}