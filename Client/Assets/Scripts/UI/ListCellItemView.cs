using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Framework;
using System.Collections;
using System.Reflection;

public class ListCellItemView : Widget
{
    public string m_MediatorAlias;
    private Type m_objectType;
    private object m_object;

    public override void InitView(params object[] args)
    {
        base.InitView(args);
        Assembly assembly = Assembly.GetExecutingAssembly();
        m_object = assembly.CreateInstance(viewName);
    }

    protected override void OnInitViewEnd(params object[] args)
    {
        MethodInfo mi = m_objectType.GetMethod("InitView");
        mi.Invoke(m_object, args);
        m_isInited = true;
    }

    public void DrawCell(int listHandle,int index)
    {
        MethodInfo mi = m_objectType.GetMethod("DrawCell");
        mi.Invoke(m_object, new object[] {index});
    }

    private void OnDestroy()
    {
        DestroyView();
    }
}