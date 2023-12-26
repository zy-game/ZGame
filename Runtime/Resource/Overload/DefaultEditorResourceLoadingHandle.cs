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
            ResourceManager.instance.AddResourcePackageHandle(new ResPackageHandle(handleName, true));
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
            ResourceManager.instance.RemoveResourcePackageHandle(handleName);
            GC.SuppressFinalize(this);
        }

        public ResHandle LoadAsset(string path)
        {
#if UNITY_EDITOR
            ResPackageHandle _handle = ResourceManager.instance.GetResourcePackageHandle(handleName);
            if (_handle is null)
            {
                return default;
            }

            if (_handle.TryGetValue(path, out ResHandle resHandle))
            {
                return resHandle;
            }

            Debug.Log("Load Assets:" + path);
            if (path.EndsWith(".unity"))
            {
                _handle.Setup(resHandle = ResHandle.OnCreate(_handle, null, path));
                return resHandle;
            }

            var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
            if (asset == null)
            {
                return default;
            }

            _handle.Setup(resHandle = ResHandle.OnCreate(_handle, asset, path));
            return resHandle;
#endif
            return default;
        }

        public async UniTask<ResHandle> LoadAssetAsync(string path)
        {
#if UNITY_EDITOR
            ResPackageHandle _handle = ResourceManager.instance.GetResourcePackageHandle(handleName);
            if (_handle is null)
            {
                return default;
            }

            if (_handle.TryGetValue(path, out ResHandle resHandle))
            {
                return resHandle;
            }

            if (path.EndsWith(".unity"))
            {
                _handle.Setup(resHandle = ResHandle.OnCreate(_handle, null, path));
                return resHandle;
            }

            UnityEngine.Object asset = UnityEditor.AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object));
            if (asset == null)
            {
                return default;
            }

            _handle.Setup(resHandle = ResHandle.OnCreate(_handle, asset, path));
            return resHandle;
#endif
            return default;
        }

        public void Release(string handle)
        {
        }
    }
}