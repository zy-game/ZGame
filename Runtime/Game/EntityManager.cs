using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ZGame.Game
{
    public class EntityManager : GameFrameworkModule
    {
        private List<GameEntity> _entities = new();

        public override void Update()
        {
            for (int i = _entities.Count - 1; i >= 0; i--)
            {
                _entities[i].OnUpdate();
            }
        }

        public override void FixedUpdate()
        {
            for (int i = _entities.Count - 1; i >= 0; i--)
            {
                _entities[i].OnFixedUpdate();
            }
        }

        public override void LateUpdate()
        {
            for (int i = _entities.Count - 1; i >= 0; i--)
            {
                _entities[i].OnLateUpdate();
            }
        }

        public GameEntity CreateEntity(string tag = "")
        {
            GameEntity entity = new GameEntity();
            _entities.Add(entity);
            entity.SetTag(tag);
            return entity;
        }

        public GameEntity[] FindByTag(string tag)
        {
            Debug.Log(string.Join(",", _entities.Select(x => x.tag)));
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
            gameEntity.Dispose();
        }

        public void ClearEntitys()
        {
            for (var i = 0; i < _entities.Count; i++)
            {
                _entities[i].Dispose();
            }

            _entities.Clear();
        }

        public override void Dispose()
        {
            ClearEntitys();
            GC.SuppressFinalize(this);
        }
    }
}