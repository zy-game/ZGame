using System;
using Cysharp.Threading.Tasks;

namespace ZGame.Resource
{
    public interface IAssetLoadingPipeline : IDisposable
    {
        AssetObjectHandle LoadAsset(string path);
        UniTask<AssetObjectHandle> LoadAssetAsync(string path);
        void Release(AssetObjectHandle obj);
    }
}