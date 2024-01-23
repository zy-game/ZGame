using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ZGame.UI
{
    public class CountDownHelper : IDisposable
    {
        private TMP_Text _text;
        private int _count;
        private int _interval;
        private string _format;
        private Action _onFinish;
        private Coroutine _coroutine;

        public CountDownHelper(TMP_Text tmpText, int count, int interval, string format, Action onFinish)
        {
            this._text = tmpText;
            this._count = count;
            this._interval = interval;
            this._format = format;
            this._onFinish = onFinish;
        }

        public void OnStart()
        {
            this._coroutine = UIManager.instance.StartCoroutine(this.StartCountDown());
        }

        private IEnumerator StartCountDown()
        {
            while (this._count > 0)
            {
                this._count--;
                this._text.text = string.Format(_format, this._count);
                yield return new WaitForSeconds(this._interval);
            }

            if (this._onFinish != null)
            {
                this._onFinish();
            }

            this._text.text = "";
        }

        public void Dispose()
        {
            if (this._coroutine == null)
            {
                return;
            }

            UIManager.instance.StopCoroutine(this._coroutine);
            this._coroutine = null;
            this._onFinish = null;
            this._text = null;
            this._count = 0;
            this._interval = 0;
            this._format = null;
            GC.SuppressFinalize(this);
        }
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

        private CountDownHelper countDownHelper;


        public UIBase(GameObject gameObject)
        {
            this.gameObject = gameObject;
            this.name = gameObject.name;
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
            this.StopCountDown();
        }

        public void StartCountDown(TMP_Text tmp_text, int count, int interval, string format, Action onFinish)
        {
            this.countDownHelper = new CountDownHelper(tmp_text, count, interval, format, onFinish);
            this.countDownHelper.OnStart();
        }

        public void StopCountDown()
        {
            if (this.countDownHelper != null)
            {
                this.countDownHelper.Dispose();
                this.countDownHelper = null;
            }
        }

        public Coroutine StartCoroutine(IEnumerator enumerator)
        {
            return UIManager.instance.StartCoroutine(enumerator);
        }

        public void StopCoroutine(Coroutine coroutine)
        {
            UIManager.instance.StopCoroutine(coroutine);
        }
    }
}