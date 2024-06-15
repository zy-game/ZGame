using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace ZGame.UI
{
    [Serializable]
    public class ContentDatable
    {
        public RectTransform target;
        public Vector2 minSize = Vector2.zero;
        public Vector2 maxSize = Vector2.zero;
    }

    /// <summary>
    /// 自动计算内容大小
    /// </summary>
    public class AutoContentSize : ContentSizeFitter
    {
        public Vector2 offset;
        public Vector2 minSize = Vector2.zero;
        public Vector2 maxSize = Vector2.zero;
        public List<ContentDatable> targets;
        private RectTransform source;


        private void OnEnable()
        {
            Refresh();
        }

        public override void SetLayoutHorizontal()
        {
            // base.SetLayoutHorizontal();
            Refresh();
        }

        public override void SetLayoutVertical()
        {
            // base.SetLayoutVertical();
            Refresh();
        }

        // [Button("Refresh")]
        void Refresh()
        {
            if (source == null)
            {
                source = GetComponent<RectTransform>();
            }

            int rowCount = 1;
            if (horizontalFit is not FitMode.Unconstrained)
            {
                float preferrSize = LayoutUtility.GetPreferredSize(source, 0);
                preferrSize = Mathf.Max(preferrSize, minSize.x);
                rowCount = maxSize.x == 0 ? rowCount : Mathf.Max(rowCount, (int)Mathf.Ceil(source.rect.width / maxSize.x));
                source.SetSizeWithCurrentAnchors((RectTransform.Axis)0, maxSize.x == 0 ? preferrSize : Mathf.Clamp(preferrSize, minSize.x, maxSize.x));
            }

            if (verticalFit is not FitMode.Unconstrained)
            {
                float preferrSize = LayoutUtility.GetPreferredSize(source, 1);
                preferrSize = preferrSize * rowCount;
                preferrSize = Mathf.Clamp(preferrSize, minSize.y, maxSize.y == 0 ? float.MaxValue : maxSize.y);
                source.SetSizeWithCurrentAnchors((RectTransform.Axis)1, preferrSize);
            }

            if (targets is null || targets.Count == 0)
            {
                return;
            }

            Vector2 size = source.sizeDelta + offset;
            for (int i = 0; i < targets.Count; i++)
            {
                if (targets[i] == null) continue;

                float x = Mathf.Clamp(size.x, targets[i].minSize.x, targets[i].maxSize.x == 0 ? float.MaxValue : targets[i].maxSize.x);
                float y = Mathf.Clamp(size.y, targets[i].minSize.y, targets[i].maxSize.y == 0 ? float.MaxValue : targets[i].maxSize.y);

                targets[i].target.sizeDelta = new Vector2(x, y);
            }
        }
    }
}