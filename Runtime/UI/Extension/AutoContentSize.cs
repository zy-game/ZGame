using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

namespace ZGame.UI
{
    /// <summary>
    /// 自动计算内容大小
    /// </summary>
    [RequireComponent(typeof(ContentSizeFitter))]
    public class AutoContentSize : MonoBehaviour
    {
        public Vector2 offset;
        public Vector2 minSize = Vector2.zero;
        public Vector2 maxSize = Vector2.zero;
        public List<RectTransform> targets;
        private RectTransform source;


        private void OnEnable()
        {
            Refresh();
        }

        [Button("Refresh")]
        public void Refresh()
        {
            if (source == null)
            {
                source = GetComponent<RectTransform>();
            }


            if (targets is null || targets.Count == 0)
            {
                return;
            }

            for (int i = 0; i < targets.Count; i++)
            {
                targets[i].sizeDelta = source.sizeDelta + offset;
                float x = Mathf.Clamp(targets[i].sizeDelta.x, minSize.x, maxSize.x == 0 ? float.MaxValue : maxSize.x);
                float y = Mathf.Clamp(targets[i].sizeDelta.y, minSize.y, maxSize.y == 0 ? float.MaxValue : maxSize.y);
                targets[i].sizeDelta = new Vector2(x, y);
            }
        }
    }
}