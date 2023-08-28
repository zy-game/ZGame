using System;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace ZEngine.Game
{
    public class GameManager : Single<GameManager>
    {
        private List<IWorld> worlds = new List<IWorld>();

        public IWorld current
        {
            get { return default; }
        }

        public IWorld OpenWorld(string name)
        {
            IWorld world = Find(name);
            if (world is not null)
            {
                return world;
            }

            world = IWorld.Create(name);
            worlds.Add(world);
            return world;
        }

        public IWorld Find(string name)
        {
            return worlds.Find(x => x.name == name);
        }

        public void CloseWorld(string name)
        {
            IWorld world = Find(name);
            if (world is null)
            {
                return;
            }

            worlds.Remove(world);
            Engine.Class.Release(world);
        }
    }
}