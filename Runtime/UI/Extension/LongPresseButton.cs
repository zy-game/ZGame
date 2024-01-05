using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace ZGame.Window
{
    public class LongPresseButton : Button
    {
        [SerializeField] public UnityEvent onDown;
        [SerializeField] public UnityEvent onUp;
        [SerializeField] public UnityEvent onCancel;
        [SerializeField] public float limitTime;
        private float startTime;
        private bool isDown;

        public void SetLimitTime(float time)
        {
            limitTime = time;
        }

        public void OnDown(UnityAction action)
        {
            onDown.AddListener(action);
        }

        public void OnUp(UnityAction action)
        {
            onUp.AddListener(action);
        }

        public void OnCancel(UnityAction action)
        {
            onCancel.AddListener(action);
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            if (onDown != null)
            {
                onDown.Invoke();
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

            isDown = false;
            if (onUp != null)
            {
                onUp.Invoke();
            }
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);
            if (isDown is false)
            {
                return;
            }

            if (onUp != null)
            {
                onUp.Invoke();
            }
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);
            if (isDown)
            {
                isDown = false;
                if (onCancel != null)
                {
                    onCancel.Invoke();
                }
            }
        }
    }
}