using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Framework;
using UnityEngine.UI;
using System.Collections;
using System.Reflection;

public enum ListViewOrientation
{
    Vertical = 0,
    Horizontal = 1,
}

public class ListView : Widget
{
    public Scrollbar m_ScrollBar;
    public string m_MediatorAlias;
    public float m_CellSize = 100;
    public float m_CellSpacing = 0;
    public bool m_GridItemMode = false;
    public int m_CellItemSize = 0;
    public float m_StartPadding = 0;
    public float m_EndPadding = 0;

    private Type m_objectType;
    private object m_object;

    public ListViewOrientation m_Orientation = ListViewOrientation.Vertical;

    int lastScrollToPos = -1;
    bool initList = false;

    public int dataSize
    {
        get
        {
            return m_DataSize;
        }
    }
    int m_DataSize = 0;
    int m_RealDataSize = 0;
    public int realDataSize
    {
        get
        {
            return m_RealDataSize;
        }
    }

    List<ListCellView> m_UnuseCells = new List<ListCellView>();
    List<ListCellView> m_VisiableCells = new List<ListCellView>();

    RectTransform m_ListRect;
    RectTransform m_ListCellsRect;

    int m_LastPosition = -1;
    int m_LastVisiableNum = 0;
    float m_CellOffset = 0;

    GameObject m_DynamicCell;
    RectTransform m_DynamicCellRect;
    Transform m_Cells;


    public void SetDataSize(int num)
    {
        m_RealDataSize = num;
        if (m_GridItemMode)
        {
            m_DataSize = num / m_CellItemSize + ((num % m_CellItemSize>0)?1:0);
        }
        else
        {
            m_DataSize = num;
        }
        NotifyDataSetChanged();
    }

    int ComputeCurrentPosition()
    {
        float pos = 0;
        switch (m_Orientation)
        {
            case ListViewOrientation.Horizontal:
                pos = m_ListCellsRect.anchoredPosition.x;
                break;
            case ListViewOrientation.Vertical:
                pos = m_ListCellsRect.anchoredPosition.y;
                break;
        }
        int index = (int)((pos- m_StartPadding) / (m_CellSize + m_CellSpacing));
        if (index < 0) index = 0;
        if (index >= m_DataSize) index = m_DataSize - 1;
        return index;
    }

    int ComputeVisiableNum()
    {
        float size = 0;
        switch (m_Orientation)
        {
            case ListViewOrientation.Horizontal:
                size = m_ListRect.sizeDelta.x;
                break;
            case ListViewOrientation.Vertical:
                size = m_ListRect.sizeDelta.y;
                break;
        }
        int num = (int)(size / m_CellSize);
        num += 3;
        return num;
    }

    private void Awake()
    {
        m_ListRect = GetComponent<RectTransform>();
        
        Transform cells = transform.Find("Cells");
        m_Cells = cells;
        m_ListCellsRect = cells.GetComponent<RectTransform>();

        ListCellView[] cellViews = cells.GetComponentsInChildren<ListCellView>();
        m_DynamicCell = cellViews[0].gameObject;
        m_DynamicCellRect = m_DynamicCell.GetComponent<RectTransform>();
        int needCellNum = ComputeVisiableNum();
        for(int i = 0; i < cellViews.Length; i++)
        {
            cellViews[i].gameObject.SetActive(false);
            cellViews[i].Init(this);
            cellViews[i].InitView();
            m_UnuseCells.Add(cellViews[i]);
        }
        for(int i = 0; i < needCellNum - cellViews.Length; i++)
        {
            ListCellView cellView = DynamicCell().GetComponent<ListCellView>();
            cellView.Init(this);
            cellView.InitView();
            m_UnuseCells.Add(cellView);
        }
        Assembly assembly = Assembly.GetExecutingAssembly();
        m_object = assembly.CreateInstance(viewName);
        m_objectType = Type.GetType(viewName);
        InitView();
    }

    protected override void OnInitViewEnd(params object[] args)
    {
        MethodInfo mi = m_objectType.GetMethod("InitView");
        mi.Invoke(m_object, null);
        m_isInited = true;
    }

    private void Start()
    {
        UpdateList();
    }

    GameObject DynamicCell()
    {
        GameObject go = Instantiate(m_DynamicCell);
        go.transform.SetParent(m_Cells);
        go.SetActive(false);
        go.transform.localPosition = Vector3.zero;
        RectTransform rectTrans = go.GetComponent<RectTransform>();
        rectTrans.offsetMin = m_DynamicCellRect.offsetMin;
        rectTrans.offsetMax = m_DynamicCellRect.offsetMax;

        go.transform.localScale = Vector3.one;
        go.transform.localRotation = Quaternion.Euler(Vector3.zero);
        return go;
    }

    public void NotifyDataSetChanged()
    {
        UpdateList(true);
    }

    public void ResetStatus()
    {
        
    }

    public void ScrollTo(int index)
    {
        float size = 0;
        switch (m_Orientation)
        {
            case ListViewOrientation.Horizontal:
                size = m_ListRect.sizeDelta.x;
                break;
            case ListViewOrientation.Vertical:
                size = m_ListRect.sizeDelta.y;
                break;
        }
        float topPos = m_StartPadding + (index * (m_CellSize + m_CellSpacing));
        float cellsSize = m_StartPadding + m_DataSize * (m_CellSize + m_CellSpacing) - m_CellSpacing + m_EndPadding;
        float hidenArea = cellsSize - size;
        if (m_ScrollBar != null)
        {
            float v = topPos / hidenArea;
            if (v > 1) v = 1;
            if (v < 0) v = 0;
            v = 1 - v;
            m_ScrollBar.value = v;
            if (!initList)
            {
                lastScrollToPos = index;
            }
        }
    }

    void UpdateCellsRect()
    {
        float cellsSize = m_StartPadding + m_DataSize * (m_CellSize + m_CellSpacing) - m_CellSpacing + m_EndPadding;
        switch (m_Orientation)
        {
            case ListViewOrientation.Horizontal:
                m_ListCellsRect.sizeDelta = new Vector2(cellsSize, m_ListCellsRect.sizeDelta.y);
                break;
            case ListViewOrientation.Vertical:
                m_ListCellsRect.sizeDelta = new Vector2(m_ListCellsRect.sizeDelta.x, cellsSize);
                break;
        }
        m_CellOffset = 0;//m_CellSize / 2;
    }

    void UpdateList(bool forceUpdateCells=false)
    {
        UpdateCellsRect();
        int pos = ComputeCurrentPosition();
        pos--;
        if (pos < 0) pos = 0;
        int num = ComputeVisiableNum();
        float offset = m_StartPadding+pos*(m_CellSize+m_CellSpacing);
        float cellPos = 0;
        if(pos!=m_LastPosition || num != m_LastVisiableNum || forceUpdateCells)
        {
            ListCellView[] cells = new ListCellView[num];
            List<ListCellView> unuse = new List<ListCellView>();
            for(int i = 0; i < m_VisiableCells.Count; i++)
            {
                ListCellView cellView = m_VisiableCells[i];
                int p = cellView.index - pos;
                if (cellView.index > -1 && cellView.index < m_DataSize && p > -1 && p < cells.Length)
                {
                    cells[p] = cellView;
                }
                else
                {
                    unuse.Add(cellView);
                }
            }
            for(int i = 0; i < unuse.Count; i++)
            {
                DeactiveCell(unuse[i]);
            }
            for(int i = 0; i < cells.Length; i++)
            {
                if(pos+i>=0 && pos + i < m_DataSize && cells[i] == null)
                { 
                    cells[i] = ActiveCell(pos + i);
                    cells[i].DrawCell(handle, cells[i].index);
                }
                else if(cells[i] != null)
                {
                    if (forceUpdateCells)
                    {
                        cells[i].DrawCell(handle, cells[i].index);
                    }
                }
            }
        }
        m_LastPosition = pos;
        m_LastVisiableNum = num;
    }

    void DeactiveCell(ListCellView cellView)
    {
        cellView.index = -1;
        m_VisiableCells.Remove(cellView);
        cellView.CloseView();
        cellView.gameObject.SetActive(false);
        //if (m_UnuseCells.IndexOf(cellView) == -1)
        {
            m_UnuseCells.Add(cellView);
        }
    }

    ListCellView ActiveCell(int index)
    {
        ListCellView cellView;
        if (m_UnuseCells.Count <= 0)
        {
            ListCellView cellView_ = DynamicCell().GetComponent<ListCellView>();
            cellView_.InitView();
            m_UnuseCells.Add(cellView_);
        }
        cellView = m_UnuseCells[0];
        cellView.index = index;
        m_UnuseCells.Remove(cellView);
        //if (m_VisiableCells.IndexOf(cellView) == -1)
        {
            m_VisiableCells.Add(cellView);
        }
        Vector3 pos = Vector3.zero;
        switch (m_Orientation)
        {
            case ListViewOrientation.Horizontal:
                pos = new Vector3(1, 0, 0);
                break;
            case ListViewOrientation.Vertical:
                pos = new Vector3(0, -1, 0);
                break;
        }
        float d = m_CellOffset + index * (m_CellSize + m_CellSpacing);
        
        cellView.transform.localPosition = pos * d;
        cellView.gameObject.SetActive(true);
        return cellView;
    }

    public void OnScrollChanged(Vector2 pos)
    {
        if (!initList)
        {
            ScrollTo(lastScrollToPos);
        }
        initList = true;
        UpdateList();
    }

    private void OnEnable()
    {
        OpenView();
    }

    private void OnDisable()
    {
        CloseView();
    }

    private void OnDestroy()
    {
        DestroyView();
    }
}