using System.Collections.Generic;

namespace ZGame.Game
{
    class EntityManager : IReference
    {
        private List<Entity> entityList = new();

        public void OnGUI()
        {
        }

        public Entity FindEntity(uint id)
        {
            return entityList.Find(x => x.id == id);
        }


        public Entity CreateEntity()
        {
            Entity entity = Entity.Create();
            entityList.Add(entity);
            return entity;
        }

        public void DestroyEntity(uint id)
        {
            Entity entity = FindEntity(id);
            if (entity == null)
            {
                return;
            }

            DestroyEntity(entity);
        }

        public void DestroyEntity(Entity entity)
        {
            entityList.Remove(entity);


            RefPooled.Release(entity);
        }

        public void Clear()
        {
            entityList.ForEach(RefPooled.Release);
            entityList.Clear();
        }

        public void Release()
        {
            Clear();
        }
    }
}