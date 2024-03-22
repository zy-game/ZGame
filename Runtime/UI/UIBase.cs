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
    /// UI界面
    /// </summary>
    public class UIBase : IDisposable
    {
        public string name { get; private set; }
        public Transform transform { get; private set; }
        public GameObject gameObject { get; private set; }
        public RectTransform rect_transform { get; private set; }

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
            if (gameObject == null)
            {
                return;
            }

            Debug.Log("Enable:" + gameObject.name);
            gameObject.SetActive(true);
        }

        /// <summary>
        /// 隐藏界面
        /// </summary>
        public virtual void Disable()
        {
            if (gameObject == null)
            {
                return;
            }

            Debug.Log("Disable:" + gameObject.name);
            gameObject.SetActive(false);
        }

        /// <summary>
        /// 释放UI界面
        /// </summary>
        public virtual void Dispose()
        {
            transform = null;
            name = String.Empty;
            rect_transform = null;
            foreach (var VARIABLE in _coroutines.Values)
            {
                GameFrameworkEntry.Coroutine.StopCoroutine(VARIABLE);
            }

            _coroutines.Clear();
            GameObject.DestroyImmediate(gameObject);
            gameObject = null;
            GC.SuppressFinalize(this);
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
            _coroutines.Add(tmp_text, GameFrameworkEntry.Coroutine.StartCoroutine(this.OnStartCountDown(tmp_text, count, interval, format, onFinish)));
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
                GameFrameworkEntry.Coroutine.StopCoroutine(coroutine);
                _coroutines.Remove(target);
            }
        }
    }
}