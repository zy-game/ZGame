using System;
using EnhancedUI.EnhancedScroller;
using UnityEngine;
using UnityEngine.UI;

public class EnhancedScrollerCellViewClickable : EnhancedScrollerCellView
{
    public Action<int, GameObject> click;

    public void Awake()
    {
        GetComponent<Button>().onClick.RemoveAllListeners();
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        click?.Invoke(dataIndex, this.gameObject);
    }
}