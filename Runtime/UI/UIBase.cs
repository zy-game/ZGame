using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ZGame.Sound;

namespace ZGame.UI
{
    /// <summary>
    /// 标记主界面UI
    /// </summary>
    public sealed class HomeUI : Attribute
    {
    }

    /// <summary>
    /// UI界面
    /// </summary>
    public class UIBase : IDisposable
    {
        public string name { get; }
        public GameObject gameObject { get; }
        public Transform transform { get; }
        public RectTransform rect_transform { get; }

        private Dictionary<object, Coroutine> _coroutines = new Dictionary<object, Coroutine>();

        public UIBase(GameObject gameObject)
        {
            this.name = gameObject.name;
            this.gameObject = gameObject;
            this.transform = gameObject.transform;
            this.rect_transform = gameObject.GetComponent<RectTransform>();
        }

        /// <summary>
        /// 激活界面
        /// </summary>
        public virtual void Awake()
        {
        }

        /// <summary>
        /// 显示界面
        /// </summary>
        public virtual void Enable(params object[] args)
        {
        }

        /// <summary>
        /// 隐藏界面
        /// </summary>
        public virtual void Disable()
        {
        }

        /// <summary>
        /// 释放UI界面
        /// </summary>
        public virtual void Dispose()
        {
            this.StopCountDown(null);
        }

        /// <summary>
        /// 启动一个倒计时
        /// </summary>
        /// <param name="tmp_text"></param>
        /// <param name="count"></param>
        /// <param name="interval"></param>
        /// <param name="format"></param>
        /// <param name="onFinish"></param>
        public void StartCountDown(TMP_Text tmp_text, int count, int interval, string format, Action onFinish)
        {
            if (tmp_text == null)
            {
                return;
            }

            StopCountDown(tmp_text);
            _coroutines.Add(tmp_text, this.StartCoroutine(this.OnStartCountDown(tmp_text, count, interval, format, onFinish)));
        }

        /// <summary>
        /// 启动一个倒计时
        /// </summary>
        /// <param name="tmp_text"></param>
        /// <param name="count"></param>
        /// <param name="interval"></param>
        /// <param name="format"></param>
        /// <param name="onFinish"></param>
        /// <returns></returns>
        private IEnumerator OnStartCountDown(TMP_Text tmp_text, int count, int interval, string format, Action onFinish)
        {
            while (count > 0)
            {
                tmp_text.text = string.Format(format, count);
                yield return new WaitForSeconds(interval);
                count--;
            }

            if (onFinish != null)
            {
                onFinish();
            }
        }

        /// <summary>
        /// 停止倒计时
        /// </summary>
        /// <param name="target"></param>
        public void StopCountDown(object target)
        {
            if (target == null)
            {
                return;
            }

            if (_coroutines.TryGetValue(target, out Coroutine coroutine))
            {
                StopCoroutine(coroutine);
                _coroutines.Remove(target);
            }
        }

        /// <summary>
        /// 开启一个协程
        /// </summary>
        /// <param name="enumerator"></param>
        /// <returns></returns>
        public Coroutine StartCoroutine(IEnumerator enumerator)
        {
            return BehaviourScriptable.instance.StartCoroutine(enumerator);
        }

        /// <summary>
        /// 停止协程
        /// </summary>
        /// <param name="coroutine"></param>
        public void StopCoroutine(Coroutine coroutine)
        {
            BehaviourScriptable.instance.StopCoroutine(coroutine);
        }
    }
}