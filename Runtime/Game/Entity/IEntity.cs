using System;

namespace ZEngine.Game
{
    /// <summary>
    /// 游戏实体
    /// </summary>
    public interface IEntity : IDisposable
    {
        /// <summary>
        /// 实体编号
        /// </summary>
        int id { get; }

        /// <summary>
        /// 添加组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T AddComponent<T>() where T : IEntityComponent
        {
            return (T)AddComponent(typeof(T));
        }

        /// <summary>
        /// 添加组件
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        IEntityComponent AddComponent(Type type)
        {
            return GameManager.instance.current.AddComponent(id, type);
        }

        /// <summary>
        /// 获取组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T GetComponent<T>() where T : IEntityComponent
        {
            return (T)GetComponent(typeof(T));
        }

        /// <summary>
        /// 获取组件
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        IEntityComponent GetComponent(Type type)
        {
            return GameManager.instance.current.GetComponent(id, type);
        }

        /// <summary>
        /// 移除组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        void DestroyComponent<T>() where T : IEntityComponent
        {
            DestroyComponent(typeof(T));
        }

        /// <summary>
        /// 移除组件
        /// </summary>
        /// <param name="type"></param>
        void DestroyComponent(Type type)
        {
            GameManager.instance.current.DestroyComponent(id, type);
        }

        /// <summary>
        /// 获取所有组件
        /// </summary>
        /// <returns></returns>
        IEntityComponent[] GetComponents()
        {
            return GameManager.instance.current.GetComponents(id);
        }

        internal static IEntity Create()
        {
            Entity entity = Activator.CreateInstance<Entity>();
            entity.id = Guid.NewGuid().GetHashCode();
            return entity;
        }

        class Entity : IEntity
        {
            public void Dispose()
            {
                id = 0;
            }

            public int id { get; set; }
        }
    }
}