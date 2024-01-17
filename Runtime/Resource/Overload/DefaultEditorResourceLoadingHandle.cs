using System;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using ZGame.Window;

namespace ZGame.Resource
{
    class DefaultEditorResourceLoadingHandle : IResourceLoadingHandle
    {
        private string handleName = "EDITOR_MODE_RESOURCES";

        public DefaultEditorResourceLoadingHandle()
        {
            PackageHandleCache.instance.Add(new PackageHandle(handleName, true));
        }

        public bool Contains(string path)
        {
#if UNITY_EDITOR
            return File.Exists(path);
#endif
            return false;
        }

        public void Dispose()
        {
            PackageHandleCache.instance.Remove(handleName);
            GC.SuppressFinalize(this);
        }

        public ResObject LoadAsset(string path)
        {
#if UNITY_EDITOR
            if (ResObjectCache.instance.TryGetValue(handleName, path, out ResObject handle))
            {
                Debug.Log("缓存资源");
                return handle;
            }

            if (PackageHandleCache.instance.TryGetValue(handleName, out PackageHandle _handle) is false)
            {
                return default;
            }

            Debug.Log("Load Assets:" + path);
            if (path.EndsWith(".unity"))
            {
                return ResObject.OnCreate(_handle, null, path);
            }

            var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
            if (asset == null)
            {
                return default;
            }

            return ResObject.OnCreate(_handle, asset, path);
#endif
            return default;
        }

        public async UniTask<ResObject> LoadAssetAsync(string path)
        {
#if UNITY_EDITOR
            if (ResObjectCache.instance.TryGetValue(handleName, path, out ResObject handle))
            {
                return handle;
            }

            if (PackageHandleCache.instance.TryGetValue(handleName, out PackageHandle _handle) is false)
            {
                return default;
            }

            if (path.EndsWith(".unity"))
            {
                return ResObject.OnCreate(_handle, null, path);
            }

            UnityEngine.Object asset = UnityEditor.AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object));
            if (asset == null)
            {
                return default;
            }

            return ResObject.OnCreate(_handle, asset, path);
#endif
            return default;
        }

        public void Release(string handle)
        {
        }
    }
}