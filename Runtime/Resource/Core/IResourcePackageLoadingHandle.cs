using System;
using Cysharp.Threading.Tasks;
using ZGame.Window;

namespace ZGame.Resource
{
    public interface IResourcePackageLoadingHandle : IDisposable
    {
        UniTask Loading(params string[] paths);
    }
}