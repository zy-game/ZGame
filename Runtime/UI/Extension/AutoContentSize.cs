using System;
using UnityEngine;
using UnityEngine.UI;

namespace ZGame.Window
{
    /// <summary>
    /// 自动计算内容大小
    /// </summary>
    [RequireComponent(typeof(ContentSizeFitter))]
    public class AutoContentSize : MonoBehaviour
    {
        public bool useMinSize = false;
        public bool useMaxSize = false;
        public Vector2 offset;
        public RectTransform target;
        public Vector2 minSize = Vector2.zero;
        public Vector2 maxSize = Vector2.zero;
        private ContentSizeFitter contentSizeFitter;
        private RectTransform source;
        private Vector2 sizeDelta;

        private void OnEnable()
        {
            Refresh();
        }

        public void Refresh()
        {
            if (source == null)
            {
                source = GetComponent<RectTransform>();
                contentSizeFitter = GetComponent<ContentSizeFitter>();
            }

            RefreshSizeDelta();
            RefreshHorizontal();
            RefreshVertical();
            SetTargetSizeDelta();
        }

        private void RefreshSizeDelta()
        {
            sizeDelta = this.source.sizeDelta + offset;
        }

        public void SetTargetSizeDelta()
        {
            if (target == null)
            {
                return;
            }

            target.sizeDelta = sizeDelta;
        }

        private void RefreshHorizontal()
        {
            if (source == null || target == null || contentSizeFitter == null || contentSizeFitter.horizontalFit == ContentSizeFitter.FitMode.Unconstrained)
            {
                return;
            }


            if (useMinSize && sizeDelta.x < minSize.x)
            {
                sizeDelta.x = minSize.x;
            }

            if (useMaxSize && sizeDelta.x > maxSize.x)
            {
                sizeDelta.x = maxSize.x;
            }
        }

        private void RefreshVertical()
        {
            if (source == null || target == null || contentSizeFitter == null || contentSizeFitter.verticalFit == ContentSizeFitter.FitMode.Unconstrained)
            {
                return;
            }

            if (useMinSize && sizeDelta.y < minSize.y)
            {
                sizeDelta.y = minSize.y;
            }

            if (useMaxSize && sizeDelta.y > maxSize.y)
            {
                sizeDelta.y = maxSize.y;
            }
        }
    }
}