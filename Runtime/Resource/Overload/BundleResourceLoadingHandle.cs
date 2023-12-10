using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using ZGame.Window;

namespace ZGame.Resource
{
    class BundleResourceLoadingHandle : IResourceLoadingHandle
    {
        public void Dispose()
        {
        }

        public ResHandle LoadAsset(string path)
        {
            if (path.StartsWith("Resources") || path.StartsWith("http"))
            {
                return default;
            }

            ResourcePackageHandle handle = BundleManager.instance.GetABHandleWithAssetPath(path);
            if (handle is null)
            {
                return default;
            }

            if (path.EndsWith(".unity"))
            {
                SceneManager.LoadScene(Path.GetFileNameWithoutExtension(path));
                handle.AddRef();
                return default;
            }

            return handle.LoadAsset(path);
        }

        public async UniTask<ResHandle> LoadAssetAsync(string path, ILoadingHandle loadingHandle = null)
        {
            if (path.StartsWith("Resources") || path.StartsWith("http"))
            {
                return default;
            }


            ResourcePackageHandle handle = BundleManager.instance.GetABHandleWithAssetPath(path);
            if (handle is null)
            {
                return default;
            }

            if (path.EndsWith(".unity"))
            {
                await SceneManager.LoadSceneAsync(Path.GetFileNameWithoutExtension(path)).ToUniTask(loadingHandle);
                handle.AddRef();
                return default;
            }

            return await handle.LoadAssetAsync(path, loadingHandle);
        }

        public bool Release(ResHandle handle)
        {
            return BundleManager.instance.Release(handle);
        }
    }
}