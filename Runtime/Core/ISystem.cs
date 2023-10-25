using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZGame
{
    
    public interface ISystem : IEntity
    {
        void Startup();
    }
}