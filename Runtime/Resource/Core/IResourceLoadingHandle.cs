using System;
using Cysharp.Threading.Tasks;
using ZGame.Window;

namespace ZGame.Resource
{
    public interface IResourceLoadingHandle : IDisposable
    {
        ResHandle LoadAsset(string path);
        UniTask<ResHandle> LoadAssetAsync(string path, ILoadingHandle loadingHandle = null);
        bool Release(ResHandle handle);
    }
}