using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZGame.Config
{
    public abstract class ConfigBase : ScriptableObject
    {
        public abstract IList Config { get; }

        public virtual void Awake()
        {
        }
    }
}