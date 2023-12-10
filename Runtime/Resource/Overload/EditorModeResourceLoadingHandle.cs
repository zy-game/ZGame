using System;
using Cysharp.Threading.Tasks;
using ZGame.Window;

namespace ZGame.Resource
{
    class EditorModeResourceLoadingHandle : IResourceLoadingHandle
    {
        private ResourcePackageHandle _handle;

        public EditorModeResourceLoadingHandle()
        {
            _handle = new ResourcePackageHandle("EDITOR_MODE_RESOURCES");
        }

        public void Dispose()
        {
            _handle.Dispose();
            _handle = null;
            GC.SuppressFinalize(this);
        }

        public ResHandle LoadAsset(string path)
        {
#if UNITY_EDITOR
            if (_handle.TryGetValue(path, out ResHandle resHandle))
            {
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
            await UniTask.Delay(0);
            return LoadAsset(path);
#endif
            return default;
        }

        public bool Release(ResHandle handle)
        {
            if (_handle.Contains(handle) is false)
            {
                return false;
            }

            _handle.Release(handle);
            return true;
        }
    }
}