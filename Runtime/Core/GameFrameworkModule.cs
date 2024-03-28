using System;

namespace ZGame
{
    /// <summary>
    /// 框架模块
    /// </summary>
    public class GameFrameworkModule : IReferenceObject
    {
        /// <summary>
        /// 激活模块
        /// </summary>
        public virtual void OnAwake(params object[] args)
        {
        }

        /// <summary>
        /// 轮询模块
        /// </summary>
        public virtual void Update()
        {
        }

        /// <summary>
        /// 固定帧率轮询模块
        /// </summary>
        public virtual void FixedUpdate()
        {
        }

        /// <summary>
        /// 帧末轮询模块
        /// </summary>
        public virtual void LateUpdate()
        {
        }

        internal protected virtual void OnDrawingGUI()
        {
        }

        /// <summary>
        /// 释放模块
        /// </summary>
        public virtual void Release()
        {
        }
    }
}