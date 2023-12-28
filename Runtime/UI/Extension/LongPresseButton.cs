using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ZGame.Window
{
    public class LongPresseButton : Button
    {
        [SerializeField] public UnityEvent _onDown;
        [SerializeField] public UnityEvent _onUp;
        [SerializeField] public UnityEvent _onCancel;
        [SerializeField] public float limitTime;
        private float startTime;
        private bool isDown;

        public void SetLimitTime(float time)
        {
            limitTime = time;
        }

        public void OnDown(UnityAction action)
        {
            _onDown.AddListener(action);
        }

        public void OnUp(UnityAction action)
        {
            _onUp.AddListener(action);
        }

        public void OnCancel(UnityAction action)
        {
            _onCancel.AddListener(action);
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            if (_onDown != null)
            {
                _onDown.Invoke();
            }

            isDown = true;
            startTime = Time.realtimeSinceStartup;
        }

        private void Update()
        {
            if (isDown is false)
            {
                return;
            }

            if (Time.realtimeSinceStartup - startTime < limitTime)
            {
                return;
            }

            if (_onUp != null)
            {
                _onUp.Invoke();
            }

            isDown = false;
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);
            if (isDown is false)
            {
                return;
            }

            if (_onUp != null)
            {
                _onUp.Invoke();
            }
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);
            if (isDown)
            {
                if (_onCancel != null)
                {
                    _onCancel.Invoke();
                }

                isDown = false;
            }
        }
    }
}