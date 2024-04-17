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
    public class UIBase : IReference
    {
        private RectTransform _rectTransform;
        private Dictionary<object, Coroutine> _coroutines = new Dictionary<object, Coroutine>();

        public string name { get; private set; }
        public Transform transform => gameObject.transform;
        public GameObject gameObject { get; set; }

        public RectTransform rect_transform
        {
            get
            {
                if (_rectTransform == null)
                {
                    _rectTransform = gameObject.GetComponent<RectTransform>();
                }

                return _rectTransform;
            }
        }


        internal static UIBase Create(string path, Type type, Canvas canvas)
        {
            GameObject gameObject = CoreAPI.VFS.GetGameObjectSync(path);
            if (gameObject == null)
            {
                CoreAPI.Logger.Log("加载资源失败：" + path);
                return default;
            }

            UIBase uiBase = (UIBase)RefPooled.Spawner(type);
            uiBase.gameObject = gameObject;
            uiBase.gameObject.transform.SetParent(canvas.transform);
            uiBase.gameObject.transform.position = Vector3.zero;
            uiBase.gameObject.transform.rotation = Quaternion.Euler(Vector3.zero);
            uiBase.gameObject.transform.localScale = Vector3.one;
            uiBase.name = gameObject.name;
            uiBase.rect_transform.sizeDelta = Vector2.zero;
            uiBase.rect_transform.anchoredPosition = Vector2.zero;
            return uiBase;
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

            gameObject.SetActive(false);
        }

        
        /// <summary>
        /// 释放UI界面
        /// </summary>
        public virtual void Release()
        {
            name = String.Empty;
            foreach (var VARIABLE in _coroutines.Values)
            {
                CoreAPI.StopCoroutine(VARIABLE);
            }

            _coroutines.Clear();
            GameObject.DestroyImmediate(gameObject);
            gameObject = null;
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
            _coroutines.Add(tmp_text, CoreAPI.StartCoroutine(this.OnStartCountDown(tmp_text, count, interval, format, onFinish)));
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
                CoreAPI.StopCoroutine(coroutine);
                _coroutines.Remove(target);
            }
        }
    }
}