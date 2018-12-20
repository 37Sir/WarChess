using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Framework;
using System.Collections;
using System.Reflection;

public class ListCellView : Widget
{
    public string m_MediatorAlias;
    [HideInInspector]
    public int index = -1;

    int itemCount = 0;
    bool itemMode = false;
    float itemSpacing = 0;

    private Type m_objectType;
    private object m_object;

    private ListView m_listView;

    ListCellItemView[] m_CellItems;

    public void Init(ListView listView)
    {
        m_listView = listView;
        itemCount = m_listView.m_CellItemSize;
        itemMode = m_listView.m_GridItemMode;
        if (!itemMode) return;
        m_CellItems = gameObject.GetComponentsInChildren<ListCellItemView>(true);
        for(int i = 0; i < m_CellItems.Length; i++)
        {
            m_CellItems[i].gameObject.SetActive(false);
        }
    }

    public override void InitView(params object[] args)
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        m_object = assembly.CreateInstance(viewName);
        m_objectType = Type.GetType(viewName);
        if (itemMode)
        {
            for (int i = 0; i < m_CellItems.Length; i++)
            {
                m_CellItems[i].InitView(args);
            }
            m_isInited = true;
        }
        else
        {
            base.InitView(args);
        }
    }

    protected override void OnInitViewEnd(params object[] args)
    {
        if (itemMode) return;
        MethodInfo mi = m_objectType.GetMethod("InitView");
        mi.Invoke(m_object, args);
        m_isInited = true;
    }

    public void DrawCell(int listHandle, int index)
    {
        if (itemMode)
        {
            int forward = index * itemCount;
            int realCount = m_listView.realDataSize - forward;
            if (realCount > itemCount) realCount = itemCount;
            for (int i = 0; i < realCount; i++)
            {
                m_CellItems[i].gameObject.SetActive(true);
                m_CellItems[i].DrawCell(listHandle, forward + i);
            }
        }
        else
        {
            MethodInfo mi = m_objectType.GetMethod("DrawCell");
            mi.Invoke(m_object, new object[] { index });
        }
        
    }

    private void OnDestroy()
    {
        if (itemMode) return;
        DestroyView();
    }

    public override void CloseView()
    {
        if (itemMode)
        {
            for(int i = 0; i < m_CellItems.Length; i++)
            {
                if (m_CellItems[i].gameObject.activeSelf)
                {
                    m_CellItems[i].CloseView();
                    m_CellItems[i].gameObject.SetActive(false);
                }
            }
        }
        else
        {
            base.CloseView();
        }
    }
}