using System;
using System.Collections.Generic;
using UnityEngine;
using ZGame.Resource;

namespace ZGame.Game
{
    /// <summary>
    /// 游戏实体对象
    /// </summary>
    public sealed class GameEntity : IDisposable
    {
        private string _id;
        private string _tag;
        private List<EntityComponent> _components = new List<EntityComponent>();

        /// <summary>
        /// 实体id
        /// </summary>
        public string id => _id;

        /// <summary>
        /// 实体标签
        /// </summary>
        public string tag => _tag;

        /// <summary>
        /// 设置实体标签
        /// </summary>
        /// <param name="tag"></param>
        public void SetTag(string tag)
        {
            this._tag = tag;
        }

        /// <summary>
        /// 添加组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T AddComponent<T>(params object[] args) where T : EntityComponent
        {
            return (T)AddComponent(typeof(T), args);
        }

        /// <summary>
        /// 获取组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetComponent<T>() where T : EntityComponent
        {
            return (T)GetComponent(typeof(T));
        }

        /// <summary>
        /// 移除组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void RemoveComponent<T>() where T : EntityComponent
        {
            RemoveComponent(typeof(T));
        }

        /// <summary>
        /// 添加组件
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public EntityComponent AddComponent(Type type, params object[] args)
        {
            if (type.IsSubclassOf(typeof(EntityComponent)) is false)
            {
                throw new NotImplementedException("类型必须是IComponent的子类");
            }

            EntityComponent component = (EntityComponent)Activator.CreateInstance(type);
            component.SetEntity(this);
            _components.Add(component);
            component.OnAwake(args);
            component.OnEnable();
            return component;
        }

        /// <summary>
        /// 获取组件
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public EntityComponent GetComponent(Type type)
        {
            if (type.IsSubclassOf(typeof(EntityComponent)) is false)
            {
                throw new NotImplementedException("类型必须是IComponent的子类");
            }

            return _components.Find(x => x.GetType() == type);
        }

        /// <summary>
        /// 移除组件
        /// </summary>
        /// <param name="type"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void RemoveComponent(Type type)
        {
            if (type.IsSubclassOf(typeof(EntityComponent)) is false)
            {
                throw new NotImplementedException("类型必须是IComponent的子类");
            }

            for (int i = 0; i < _components.Count; i++)
            {
                if (_components[i].GetType() == type)
                {
                    _components[i].OnDisable();
                    _components[i].Dispose();
                    _components.RemoveAt(i);
                }
            }
        }

        internal void OnUpdate()
        {
            for (int i = 0; i < _components.Count; i++)
            {
                _components[i].OnUpdate();
            }
        }

        internal void OnFixedUpdate()
        {
            for (int i = 0; i < _components.Count; i++)
            {
                _components[i].OnFixedUpdate();
            }
        }

        internal void OnLateUpdate()
        {
            for (int i = 0; i < _components.Count; i++)
            {
                _components[i].OnLateUpdate();
            }
        }

        public void Dispose()
        {
            for (int i = 0; i < _components.Count; i++)
            {
                _components[i].Dispose();
            }

            _components.Clear();
            GC.SuppressFinalize(this);
        }
    }
}