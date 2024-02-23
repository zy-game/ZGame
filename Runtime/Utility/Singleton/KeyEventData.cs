using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ZGame
{
    /// <summary>
    /// 按键事件数据
    /// </summary>
    public class KeyEventData : IDisposable
    {
        public KeyCode keyCode { get; }
        private bool isUsed { get; set; }
        private List<UnityAction<KeyEventData>> action { get; }

        public KeyEventData(KeyCode key)
        {
            this.keyCode = key;
            this.action = new List<UnityAction<KeyEventData>>();
        }

        /// <summary>
        /// 调用按键事件
        /// </summary>
        public void Invoke()
        {
            if (this.action is null || this.action.Count == 0)
            {
                return;
            }

            isUsed = false;
            for (int i = this.action.Count - 1; i >= 0; i--)
            {
                if (isUsed)
                {
                    return;
                }

                this.action[i].Invoke(this);
            }
        }

        /// <summary>
        /// 添加按键事件
        /// </summary>
        /// <param name="action"></param>
        public void AddListener(UnityAction<KeyEventData> action)
        {
            this.action.Add(action);
        }

        /// <summary>
        /// 移除按键事件
        /// </summary>
        /// <param name="action"></param>
        public void RemoveListener(UnityAction<KeyEventData> action)
        {
            this.action.Remove(action);
        }

        /// <summary>
        /// 使用事件，调用此函数后，后续事件将不会触发
        /// </summary>
        public void Use()
        {
            this.isUsed = true;
        }

        /// <summary>
        /// 释放事件
        /// </summary>
        public void Dispose()
        {
            this.action.Clear();
        }
    }
}