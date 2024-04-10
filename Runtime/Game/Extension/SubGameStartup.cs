using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using HybridCLR;
using UnityEngine;

namespace ZGame.Game
{
    public class SubGameStartup : IReferenceObject
    {
        public virtual UniTask<Status> OnEntry()
        {
            return UniTask.FromResult(Status.Success);
        }

        public virtual void Release()
        {
        }
    }
}