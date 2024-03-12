using System;
using Cysharp.Threading.Tasks;
using EnhancedUI;
using EnhancedUI.EnhancedScroller;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace ZGame.UI
{
    [RequireComponent(typeof(ScrollRect))]
    [RequireComponent(typeof(EnhancedScroller))]
    public class LoopScrollViewer : MonoBehaviour, IEnhancedScrollerDelegate
    {
        private SmallList<object> _data;
        private EnhancedScroller scroller;
        public LoopScrollCellView prefab;
        public UnityEvent<LoopScrollCellView> onCellViewRefreshed;

        private void Start()
        {
            scroller = GetComponent<EnhancedScroller>();
            scroller.Delegate = this;
            scroller?.ReloadData();
        }


        public void SetDataList(params object[] args)
        {
            if (_data is null)
            {
                _data = new SmallList<object>();
            }

            _data.Clear();
            for (var i = 0; i < args.Length; i++)
            {
                _data.Add(args[i]);
            }

            scroller?.ReloadData();
        }

        public int GetNumberOfCells(EnhancedScroller scroller)
        {
            return _data.Count;
        }


        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            return 100;
        }

        // public void LateUpdate()
        // {
        //     
        //     int count =GetNumberOfCells(scroller);
        //     for (int i = 0; i < count; ++i)
        //     {
        //         LoopListViewItem2 item = mLoopListView.GetShownItemByIndex(i);
        //         ListItem2 itemScript = item.GetComponent<ListItem2>();
        //         float scale = 1 - Mathf.Abs(item.DistanceWithViewPortSnapCenter)/ 700f;
        //         scale = Mathf.Clamp(scale, 0.4f, 1);
        //         itemScript.mContentRootObj.GetComponent<CanvasGroup>().alpha = scale;
        //         itemScript.mContentRootObj.transform.localScale = new Vector3(scale, scale, 1);
        //     }
        // }

        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            LoopScrollCellView cellView = scroller.GetCellView(prefab) as LoopScrollCellView;
            cellView.name = "Cell " + dataIndex.ToString();
            cellView.SetData(_data[dataIndex]);
            onCellViewRefreshed.Invoke(cellView);
            return cellView;
        }
    }
}