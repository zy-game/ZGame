using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ZGame.Resource
{
    public class AssetObjectLoadingPipeline : IAssetLoadingPipeline
    {
        private List<AssetObjectHandle> resources = new List<AssetObjectHandle>();

        public AssetObjectHandle LoadAsset(string path)
        {
            if (path.StartsWith("http"))
            {
                Debug.LogError("网络资源必须使用异步加载！");
                return default;
            }

            AssetObjectHandle result = resources.Find(x => x.path.Equals(path));
            if (result is null)
            {
                if (path.StartsWith("Resources"))
                {
                    result = new AssetObjectHandle(Resources.Load(path.Substring(10)), path);
                    resources.Add(result);
                }
                else
                {
                    AssetBundleHandle handle = AssetBundleManager.instance.GetBundleHandle(path);
                    if (handle is not null)
                    {
                        result = handle.Load(path);
                    }
                }
            }

            return result;
        }

        public async UniTask<AssetObjectHandle> LoadAssetAsync(string path)
        {
            AssetObjectHandle result = resources.Find(x => x.path.Equals(path));
            if (result is not null)
            {
                return result;
            }

            if (path.StartsWith("http"))
            {
            }

            if (path.StartsWith("Resources"))
            {
                result = new AssetObjectHandle(await Resources.LoadAsync(path.Substring(10)), path);
                resources.Add(result);
            }
            else
            {
                AssetBundleHandle handle = AssetBundleManager.instance.GetBundleHandle(path);
                if (handle is not null)
                {
                    result = await handle.LoadAsync(path);
                }
            }

            return result;
        }

        public void Release(AssetObjectHandle resObject)
        {
            AssetObjectHandle _resObject = resources.Find(x => x.Equals(resObject));
            if (_resObject is null)
            {
                return;
            }

            _resObject.Release();
            if (_resObject.refCount > 0)
            {
                return;
            }

            _resObject.Dispose();
            resources.Remove(_resObject);
        }

        public void Dispose()
        {
            foreach (var VARIABLE in resources)
            {
                VARIABLE.Dispose();
            }

            resources.Clear();
            Resources.UnloadUnusedAssets();
        }
    }
}