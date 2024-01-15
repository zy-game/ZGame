using System;
using UnityEngine;
using RectTransform = UnityEngine.RectTransform;

namespace EnhancedUI.EnhancedScroller
{
    public class ScrollManager : MonoBehaviour, IEnhancedScrollerDelegate
    {
        public EnhancedScroller m_Scroller;

        public EnhancedScrollerCellView m_Cell;
        private int _count;
        private Action<int, GameObject> itemChange;
        private Action<int, GameObject> click;

        private void Awake()
        {
            m_Scroller.Delegate = this;
            _count = 10;
        }

        public void Refersh(int count, Action<int, GameObject> itemChange, Action<int, GameObject> click)
        {
            Debug.Log("refresh count:" + count);
            _count = count;
            this.click = click;
            this.itemChange = itemChange;
            m_Scroller.ReloadData();
        }

        public int GetNumberOfCells(EnhancedScroller scroller)
        {
            return _count;
        }

        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            return m_Cell.gameObject.GetComponent<RectTransform>().sizeDelta.y;
        }

        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            EnhancedScrollerCellViewClickable cellView = scroller.GetCellView(m_Cell) as EnhancedScrollerCellViewClickable;
            cellView.click = click;
            cellView.gameObject.SetActive(true);
            itemChange?.Invoke(dataIndex, cellView.gameObject);
            return cellView;
        }
    }
}

/// <summary>
/// 滑动类型
/// </summary>
public enum ScrollType
{
    Horizontal, //竖直滑动
    Vertical, //水平滑动
}