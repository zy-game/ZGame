using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ZGame.Game
{
    public class EntityManager : GameFrameworkModule
    {
        private List<GameEntity> _entities = new();

        public GameEntity CreateEntity(string tag = "")
        {
            GameEntity entity = new GameEntity();
            _entities.Add(entity);
            entity.SetTag(tag);
            return entity;
        }

        public GameEntity[] FindByTag(string tag)
        {
            return _entities.FindAll(x => x.tag == tag).ToArray();
        }

        public GameEntity FindEntityById(string id)
        {
            return _entities.Find(x => x.id == id);
        }


        public void RemoveEntityById(string id)
        {
            GameEntity gameEntity = FindEntityById(id);
            if (gameEntity is null)
            {
                return;
            }

            _entities.Remove(gameEntity);
            GameFrameworkFactory.Release(gameEntity);
        }

        public void ClearEntitys()
        {
            for (var i = 0; i < _entities.Count; i++)
            {
                GameFrameworkFactory.Release(_entities[i]);
            }

            _entities.Clear();
        }

        public override void Release()
        {
            ClearEntitys();
        }
    }
}