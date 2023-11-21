using System;
using Cysharp.Threading.Tasks;

namespace ZGame.Resource
{
    public interface IAssetLoadingPipeline : IDisposable
    {
        ResHandle LoadAsset(string path);
        UniTask<ResHandle> LoadAssetAsync(string path);
        void Release(ResHandle obj);
    }
}