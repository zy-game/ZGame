using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScrollRectExtOfHor : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("必须在Editor中初始化的变量", order = 1)]

    [Header("只支持水平滑动，代码会强制设置", order = 2)]
    public ScrollRect scrollRect = null;

    [Header("滚动列表Item", order = 3)]
    public RectTransform scrollItemTemplate = null;
  
    [Header("Item行数，必须大于0", order = 5)]
    public int line = 1;

    [Header("行列间距", order = 6)]
    public Vector2 spacing = Vector2.zero;

    [Header("        ", order = 7)]
    [Header("        ", order = 8)]
    [Header("******************************************************", order = 9)]
    [Header("运行时计算中间变量", order = 10)]

    [Header("每页显示Item个数", order = 11)]
    public int itemCountPerPage = 0;

    [Header("每个Item大小", order = 16)]
    Vector2 cellSize = Vector2.one * 100;

    [Header("自动滑动时，停止滑动灵敏度", order = 12)]
    public float stopSpeedPerFre = 0;

    [Header("是否有点击", order = 13)]
    public bool isClickedDown = false;

    [Header("数据总条数", order = 14)]
    public int maxDataCount = 50;

    [Header("滑动方向", order = 15)]
    public ScrollDirOfHor scrollDirection = ScrollDirOfHor.Stoped;
   
    RectTransform viewRect = null;
    Vector3[] viewRectCorners = new Vector3[4];

    //更新Item   Transform:待更新的Item    int：更新Item对应的数据索引，从0开始
    public UnityAction<RectTransform, int> onUpdateItemAction = null;
    //获取下一页数据 int：下一页数据页码，从0开始
    public UnityAction<int> onGetNextPageDataAction = null;
    //获取上一页数据 int：上一页数据页码，从0开始
    public UnityAction<int> onGetLastPageDataAction = null;

    List<RectTransform> allItems = new List<RectTransform>();
    RectTransform content = null;
    private void Awake()
    {
        this.scrollItemTemplate.gameObject.SetActive(false);
        if (line <= 0)
        {
            line = 1;
        }

        this.viewRect = this.scrollRect.viewport.GetComponent<RectTransform>();
        this.content = this.scrollRect.content.GetComponent<RectTransform>();
        this.cellSize = new Vector2(this.scrollItemTemplate.rect.width, this.scrollItemTemplate.rect.height);
        //总条数计算
        this.itemCountPerPage = ((int)(this.viewRect.rect.width / (this.cellSize.x + this.spacing.x)) + 3) * line;

        int childCount = this.content.childCount;
        for (int i = 0; i < childCount; i++)
        {
            DestroyImmediate(this.content.GetChild(i).gameObject);
        }
        this.scrollRect.elasticity = 0.05f;
        this.scrollRect.horizontal = true;
        this.scrollRect.vertical = false;
        this.scrollRect.movementType = ScrollRect.MovementType.Clamped;

        RectTransform rect = null;
        Vector2 pivot = new Vector2(0, 1);
        Vector2 anchorMax = new Vector2(0, 1);
        Vector2 anchorMin = new Vector2(0, 1);
        for (int i = 0; i < itemCountPerPage; i++)
        {
            rect = GameObject.Instantiate(this.scrollItemTemplate.gameObject, this.content).GetComponent<RectTransform>();
            rect.gameObject.SetActive(true);
            rect.pivot = pivot;
            rect.anchorMax = anchorMax;
            rect.anchorMin = anchorMin;
            allItems.Add(rect);
        }
        this.scrollDirection = ScrollDirOfHor.Stoped;
    }

    public void Init()
    {
        this.scrollRect.StopMovement();
        this.InitItems();
        this.SetMaxDataCount(0);
    }

    void InitItems()
    {
        Vector2 v2 = Vector2.zero;
        for (int i = 0; i < this.itemCountPerPage; i++)
        {
            var item = this.content.GetChild(i).GetComponent<RectTransform>();
            this.UpdateItem(item, i, false);
            var pos = this.content.anchoredPosition;
            pos.x = 0;
            this.content.anchoredPosition = pos;
        }
    }

    Vector2 tempV2 = Vector2.zero;
    int UpdateItem(RectTransform item, int idx = -1, bool isSetSibling = true)
    {
        if (idx == -1)
        {
            if (this.scrollDirection == ScrollDirOfHor.Left)
            {
                idx = int.Parse(this.content.GetChild(this.itemCountPerPage - 1).gameObject.name) + 1;
            }
            else if (this.scrollDirection == ScrollDirOfHor.Right)
            {
                idx = int.Parse(this.content.GetChild(0).gameObject.name) - 1;
            }
        }
        if (idx >= 0)
        {
            if (isSetSibling)
            {
                if (this.scrollDirection == ScrollDirOfHor.Left)
                {
                    item.SetAsLastSibling();
                }
                else if (this.scrollDirection == ScrollDirOfHor.Right)
                {
                    item.SetAsFirstSibling();
                }
            }
            item.gameObject.name = idx.ToString();
            var x = idx / line;
            var y = idx % line;
            tempV2.x = (this.cellSize.x + this.spacing.x) * x;
            tempV2.y = (this.cellSize.y + this.spacing.y) * -y;
            item.anchoredPosition = tempV2;
            if (idx >= 0 && idx < this.maxDataCount)
            {
                item.gameObject.SetActive(true);
                if (this.onUpdateItemAction != null)
                {
                    this.onUpdateItemAction(item, idx);
                }
                return idx;
            }
            else
            {
                item.gameObject.SetActive(false);
            }
        }
        return -1;
    }

    //更新所有数据
    public void UpdateAllItems()
    {
        var idx = 0;
        foreach (var item in allItems)
        {
            if (int.TryParse(item.gameObject.name, out idx))
            {
                this.UpdateItem(item, idx, false);
            }
        }
    }

    //获取下一页网络数据
    void OnGetNextPageData(int page)
    {
        if (onGetNextPageDataAction != null)
        {
            onGetNextPageDataAction(page);
        }
    }

    //获取上一页网络数据
    void OnGetLastPageData(int page)
    {
        if (onGetLastPageDataAction != null)
        {
            onGetLastPageDataAction(page);
        }
    }

    //由于只支持左右滑动，所有只判断x值即可判断item是否在可视区域
    Vector3[] itemCorners = new Vector3[4];
    bool IsItemInViewRect(RectTransform item)
    {
        this.viewRect.GetWorldCorners(this.viewRectCorners);
        item.GetWorldCorners(itemCorners);
        for (int i = 0; i < 4; i++)
        {
            if (this.IsViewRectContainPoint(itemCorners[i]))
            {
                return true;
            }
        }
        return false;
    }

    //只是左右滚动，所以只判断x值
    bool IsViewRectContainPoint(Vector3 v3)
    {
        bool isContain = false;
        if (v3.x >= this.viewRectCorners[0].x && v3.x <= this.viewRectCorners[3].x)
        {
            isContain = true;
        }
        else
        {
            isContain = false;
        }
        return isContain;
    }

    public void SetMaxDataCount(int count)
    {
        this.maxDataCount = count;
        var row = Mathf.CeilToInt(count * 1.0f / this.line);
        this.content.sizeDelta = new Vector2(row * (this.cellSize.x + this.spacing.x), this.content.rect.height);
    }

    float lastX = -99999999;
    RectTransform tempItem = null;
    void Update()
    {
        if (this.scrollRect == null) return;
        var v2 = this.content.anchoredPosition;
        if (lastX < -1000000)
        {
            lastX = v2.x;
            this.scrollDirection = ScrollDirOfHor.Stoped;
            return;
        }

        if ((isClickedDown == false && Mathf.Abs(lastX - v2.x) < stopSpeedPerFre))
        {
            this.scrollRect.StopMovement();
            return;
        }
        if (lastX > -1000000)
        {
            if (lastX < v2.x)
            {
                this.scrollDirection = ScrollDirOfHor.Right;
                if (Mathf.Abs(lastX - v2.x) > 0.005)
                {
                    this.OnMoveToRight();
                }
            }
            else
            {
                this.scrollDirection = ScrollDirOfHor.Left;
                if (Mathf.Abs(lastX - v2.x) > 0.0001)
                {
                    this.OnMoveToLeft();
                }
            }
            lastX = v2.x;
        }
    }
    
    //待更新的所有Items
    List<RectTransform> updateItems = new List<RectTransform>();
    //content向左移动：左边不可见元素向右补齐
    void OnMoveToLeft()
    {
        updateItems.Clear();
        for (int i = 0; i < this.itemCountPerPage; i++)
        {
            tempItem = this.content.GetChild(i).GetComponent<RectTransform>();
            if (!this.IsItemInViewRect(tempItem))
            {
                updateItems.Add(tempItem);
            }
            else
            {
                break;
            }
        }

        var updateIdx = -1;
        for (int i = 0; i < updateItems.Count; i++)
        {
            tempItem = updateItems[i];
            updateIdx = this.UpdateItem(tempItem);
            if (updateIdx >= 0)
            {
                int idx = 0;
                for (int j = 0; j < 1000; j++)
                {
                    idx = this.itemCountPerPage * j;
                    if (idx > this.maxDataCount)
                    {
                        break;
                    }
                    if (updateIdx == idx)
                    {
                        //Debug.Log("获取下一页数据：" + updateIdx / this.itemCount + "   updateIdx:" + updateIdx + "  maxDataCount:" + this.maxDataCount);
                        this.OnGetNextPageData(updateIdx / this.itemCountPerPage);
                        break;
                    }
                }
            }
        }
    }

    //content向右移动：右边不可见元素向左补齐
    void OnMoveToRight()
    {
        updateItems.Clear();
        for (int i = this.itemCountPerPage - 1; i >= 0; i--)
        {
            tempItem = this.content.GetChild(i).GetComponent<RectTransform>();
            if (!this.IsItemInViewRect(tempItem))
            {
                //先缓存再更新：更新里面有设置tempItem的sibling值，这会导致上面GetChild不准确
                updateItems.Add(tempItem);
            }
            else
            {
                break;
            }
        }

        var updateIdx = -1;
        for (int i = 0; i < updateItems.Count; i++)
        {
            tempItem = updateItems[i];
            updateIdx = this.UpdateItem(tempItem);
            if (updateIdx >= 0)
            {
                int idx = 0;
                for (int j = 0; j < 1000; j++)
                {
                    idx = j * this.itemCountPerPage;
                    if (idx > this.maxDataCount)
                    {
                        break;
                    }
                    if (updateIdx == idx)
                    {
                        this.OnGetLastPageData(updateIdx / this.itemCountPerPage);
                        break;
                    }
                }
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        lastX = -99999999;
        this.isClickedDown = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        lastX = -99999999;
        this.scrollDirection = ScrollDirOfHor.Stoped;
        this.isClickedDown = false;
    }
}