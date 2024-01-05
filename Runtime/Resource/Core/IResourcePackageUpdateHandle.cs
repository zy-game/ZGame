using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using ZGame.Window;

namespace ZGame.Resource
{
    public interface IResourcePackageUpdateHandle : IDisposable
    {
        UniTask UpdateResourcePackageList(EntryConfig config);
    }
}