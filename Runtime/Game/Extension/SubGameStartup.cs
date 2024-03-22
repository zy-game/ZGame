using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using HybridCLR;
using UnityEngine;
using ZGame.Resource;

namespace ZGame.Game
{
    public class SubGameStartup : IDisposable
    {
        public virtual UniTask<ResultStatus> OnEntry()
        {
            return UniTask.FromResult(ResultStatus.Success);
        }

        public virtual void Dispose()
        {
        }
    }
}