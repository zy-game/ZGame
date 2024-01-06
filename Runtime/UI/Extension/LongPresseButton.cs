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
        [SerializeField] public UnityEvent onClick;
        [SerializeField] public float limitTime;
        private float startTime;
        private bool isDown;
        private bool isUp;
        private bool isCancel;
        private bool callDown;

        public void SetLimitTime(float time)
        {
            limitTime = time;
        }


        private void Update()
        {
            if (isDown is false)
            {
                return;
            }

            if (Time.realtimeSinceStartup - startTime > 0.2f && callDown is false)
            {
                callDown = true;
                onDown.Invoke();
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

        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            isDown = true;
            callDown = false;
            startTime = Time.realtimeSinceStartup;
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);
            if (isDown is false)
            {
                return;
            }

            isDown = false;
            if (Time.realtimeSinceStartup - startTime < 1)
            {
                onClick?.Invoke();
            }
            else
            {
                onUp.Invoke();
            }
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);
            if (isDown is false)
            {
                return;
            }

            isDown = false;
            onCancel.Invoke();
        }
    }
}