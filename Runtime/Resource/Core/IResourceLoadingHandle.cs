using System;
using Cysharp.Threading.Tasks;

namespace ZGame.Resource
{
    public interface IResourceLoadingHandle : IDisposable
    {
        bool Contains(string path);
        ResObject LoadAsset(string path);
        UniTask<ResObject> LoadAssetAsync(string path);
        void Release(string path);
    }
}