using System.Collections.Generic;

namespace ZGame.Game
{
    class EntityManager : IReferenceObject
    {
        private List<Entity> entityList = new();

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


            GameFrameworkFactory.Release(entity);
        }

        public void Release()
        {
            entityList.ForEach(GameFrameworkFactory.Release);
            entityList.Clear();
        }
    }
}