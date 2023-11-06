using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace ZGame.Resource
{
    public class AssetBundleResourceLoadingPipeline : IAssetLoadingPipeline
    {
        public AssetObjectHandle LoadAsset(string path)
        {
            AssetBundleHandle handle = Engine.Resource.FindAssetBundleHandle(path);
            if (handle is null)
            {
                return default;
            }

            return handle.Load(path);
        }

        public UniTask<AssetObjectHandle> LoadAssetAsync(string path)
        {
            AssetBundleHandle handle = Engine.Resource.FindAssetBundleHandle(path);
            if (handle is null)
            {
                return default;
            }

            return handle.LoadAsync(path);
        }

        public void Release(AssetObjectHandle obj)
        {
            AssetBundleHandle handle = Engine.Resource.FindAssetBundleHandle(obj);
            if (handle is null)
            {
                return;
            }

            handle.Unload(obj);
        }

        public void Dispose()
        {
        }
    }
}