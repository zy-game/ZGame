using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace ZGame.UI
{
    [Serializable]
    public class AutoContentOptions
    {
        public RectTransform target;
        [NonSerialized] public bool isOn = false;
        public ContentSizeFitter.FitMode HorizontalMode = ContentSizeFitter.FitMode.Unconstrained;
        public ContentSizeFitter.FitMode VerticalMode = ContentSizeFitter.FitMode.Unconstrained;
    }

    public enum LayoutAxis
    {
        Horizontal = 0,
        Vertical = 1
    }

    /// <summary>
    /// 自动计算内容大小
    /// </summary>
    [RequireComponent(typeof(ContentSizeFitter))]
    public class AutoContentSize : MonoBehaviour
    {
        public LayoutAxis layout;
        public bool useMinSize = false;
        public bool useMaxSize = false;
        public Vector2 offset;
        public List<AutoContentOptions> targets;
        public Vector2 minSize = Vector2.zero;
        public Vector2 maxSize = Vector2.zero;
        private RectTransform source;
        private ContentSizeFitter fitter;
        private Vector2 curSize;

        private void OnEnable()
        {
            Refresh();
        }

        public void Refresh()
        {
            if (source == null)
            {
                source = GetComponent<RectTransform>();
            }

            if (fitter == null)
            {
                fitter = GetComponent<ContentSizeFitter>();
            }


            RefreshSizeDelta();
            SetTargetSizeDelta();
        }

        private void RefreshSizeDelta()
        {
            fitter.enabled = true;
            fitter.SetLayoutHorizontal();
            fitter.SetLayoutVertical();
            fitter.enabled = false;


            this.curSize = this.source.sizeDelta;
            int count = 0;
            switch (layout)
            {
                case LayoutAxis.Horizontal:
                    if (useMinSize && curSize.x < minSize.x)
                    {
                        curSize.x = minSize.x;
                    }

                    count = ((int)(curSize.x / maxSize.x)) + 1;
                    if (useMaxSize && curSize.x > maxSize.x)
                    {
                        curSize.x = maxSize.x;
                    }

                    curSize.y = count * minSize.y;
                    break;
                case LayoutAxis.Vertical:
                    if (useMinSize && curSize.y < minSize.y)
                    {
                        curSize.y = minSize.y;
                    }

                    count = (int)(curSize.y / maxSize.y) + 1;
                    if (useMaxSize && curSize.y > maxSize.y)
                    {
                        curSize.y = maxSize.y;
                    }

                    curSize.x = count * minSize.x;
                    break;
            }

            this.source.sizeDelta = this.curSize;
        }

        public void SetTargetSizeDelta()
        {
            Vector2 size = this.curSize + offset;
            if (targets == null || targets.Count == 0)
            {
                return;
            }

            for (int i = 0; i < targets.Count; i++)
            {
                if (targets[i] == null)
                {
                    continue;
                }

                AutoContentOptions options = targets[i];
                if (options.target == null)
                {
                    continue;
                }

                float x = 0, y = 0;

                switch (options.VerticalMode)
                {
                    case ContentSizeFitter.FitMode.MinSize:
                        y = options.target.sizeDelta.y < minSize.y ? minSize.y + offset.y : options.target.sizeDelta.y;
                        break;
                    case ContentSizeFitter.FitMode.PreferredSize:
                        y = size.y;
                        break;
                    case ContentSizeFitter.FitMode.Unconstrained:
                        y = options.target.sizeDelta.y;
                        break;
                }

                switch (options.HorizontalMode)
                {
                    case ContentSizeFitter.FitMode.MinSize:
                        x = options.target.sizeDelta.x < minSize.x ? minSize.x + offset.x : options.target.sizeDelta.x;
                        break;
                    case ContentSizeFitter.FitMode.PreferredSize:
                        x = size.x;
                        break;
                    case ContentSizeFitter.FitMode.Unconstrained:
                        x = options.target.sizeDelta.x;
                        break;
                }

                options.target.sizeDelta = new Vector2(x, y);
            }
        }
    }
}