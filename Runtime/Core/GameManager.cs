using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = System.Object;

namespace ZGame
{
    /// <summary>
    /// 框架模块
    /// </summary>
    public class GameManager : IReference
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
        protected virtual void Update()
        {
        }

        /// <summary>
        /// 固定帧率轮询模块
        /// </summary>
        protected virtual void FixedUpdate()
        {
        }

        /// <summary>
        /// 帧末轮询模块
        /// </summary>
        protected virtual void LateUpdate()
        {
        }

        /// <summary>
        /// 绘制辅助线框
        /// </summary>
        protected virtual void OnDarwGizom()
        {
        }

        /// <summary>
        /// 绘制GUI面板
        /// </summary>
        protected virtual void OnGUI()
        {
        }

        /// <summary>
        /// 释放模块
        /// </summary>
        public virtual void Release()
        {
        }

        internal class Behaviour : MonoBehaviour
        {
            public GameManager Manager;



            void Update()
            {
                Manager.Update();
            }

            void FixedUpdate()
            {
                Manager.FixedUpdate();
            }

            void LateUpdate()
            {
                Manager.LateUpdate();
            }
#if UNITY_EDITOR
            void OnGUI()
            {
                Manager.OnGUI();
            }
#endif
            private void OnDrawGizmos()
            {
                Manager.OnDarwGizom();
            }

            private void OnDrawGizmosSelected()
            {
                Manager.OnDarwGizom();
            }
#if UNITY_EDITOR
            class BehaviourInspector : UnityEditor.Editor
            {
                Behaviour _behaviour;

                private void OnEnable()
                {
                    _behaviour = (Behaviour)target;
                }

                public override void OnInspectorGUI()
                {
                    _behaviour.Manager.OnGUI();
                }
            }
#endif
        }
    }
}