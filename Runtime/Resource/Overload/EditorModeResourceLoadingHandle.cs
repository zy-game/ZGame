using System;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
using ZGame.Window;

namespace ZGame.Resource
{
    class EditorModeResourceLoadingHandle : IResourceLoadingHandle
    {
        private string handleName = "EDITOR_MODE_RESOURCES";

        public EditorModeResourceLoadingHandle()
        {
            ResourceManager.instance.AddResourcePackageHandle(new ResourcePackageHandle(handleName, true));
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
            ResourcePackageHandle _handle = ResourceManager.instance.GetResourcePackageHandle(handleName);
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
                _handle.Setup(resHandle = new ResHandle(_handle, null, path));
                resHandle.LoadScene();
                return resHandle;
            }

            var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
            if (asset == null)
            {
                return default;
            }

            _handle.Setup(resHandle = new ResHandle(_handle, asset, path));
            return resHandle;
#endif
            return default;
        }

        public async UniTask<ResHandle> LoadAssetAsync(string path, ILoadingHandle loadingHandle = null)
        {
#if UNITY_EDITOR
            ResourcePackageHandle _handle = ResourceManager.instance.GetResourcePackageHandle(handleName);
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
                _handle.Setup(resHandle = new ResHandle(_handle, null, path));
                resHandle.LoadSceneAsync(loadingHandle);
                return resHandle;
            }

            UnityEditor.AssetDatabaseLoadOperation operation = UnityEditor.AssetDatabase.LoadObjectAsync(path, 0);
            await operation.ToUniTask(loadingHandle);
            if (operation.LoadedObject == null)
            {
                return default;
            }

            _handle.Setup(resHandle = new ResHandle(_handle, operation.LoadedObject, path));
            return resHandle;
#endif
            return default;
        }

        public void Release(string handle)
        {
        }
    }
}