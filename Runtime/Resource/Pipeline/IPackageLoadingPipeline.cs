using System;
using Cysharp.Threading.Tasks;

namespace ZGame.Resource
{
    public interface IPackageLoadingPipeline : IDisposable
    {
        UniTask LoadingPackageList(Action<float> progressCallback, params string[] args);
    }
}