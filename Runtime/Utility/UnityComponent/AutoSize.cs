using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ZEngine.Utility
{
    public class AutoSize : ContentSizeFitter
    {
        [SerializeField] public List<RectTransform> target;
        [SerializeField] public Vector2 offset;
        [SerializeField] public bool useLimit = false;
        [SerializeField] public Vector2 min;
        [SerializeField] public Vector2 max;
        private RectTransform rt;


        protected override void OnRectTransformDimensionsChange()
        {
            base.OnRectTransformDimensionsChange();
            Refersh();
        }

        public void Refersh()
        {
            if (rt == null)
            {
                rt = this.GetComponent<RectTransform>();
            }

            if (target is null || target.Count is 0)
            {
                return;
            }


            Vector2 size = rt.sizeDelta;
            if (useLimit)
            {
                size = new Vector2(Mathf.Clamp(rt.sizeDelta.x, min.x, max.x), Mathf.Clamp(rt.sizeDelta.y, min.y, max.y));
            }

            for (int i = 0; i < target.Count; i++)
            {
                target[i].sizeDelta = size + offset;
            }
        }
    }
}