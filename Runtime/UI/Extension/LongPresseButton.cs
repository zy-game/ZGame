using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace ZGame.UI
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

        public void SetLimitTime(float time)
        {
            limitTime = time;
        }

        private void MarkDown()
        {
            isDown = true;
            startTime = Time.realtimeSinceStartup;
        }

        private void Refresh()
        {
            isDown = false;
            startTime = 0;
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            base.OnPointerClick(eventData);
            Refresh();
            onClick?.Invoke();
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            MarkDown();
            onDown?.Invoke();
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);
            Refresh();
            onUp?.Invoke();
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);
            if (isDown is false)
            {
                return;
            }

            Refresh();
            onCancel?.Invoke();
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

            Refresh();
            onUp?.Invoke();
        }
    }
}