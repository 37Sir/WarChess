using com.zyd.common.proto.client;
using Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class RankListView
{
    private GameObject m_object;
    private RankListViewMediator m_mediator;
    private UserDataProxy m_proxy;//todo
    private PVPProxy m_pvpProxy;
    private PVEProxy m_pveProxy;


    public void InitView(GameObject gameObject)
    {
        m_object = gameObject;
        InitUIBinder(gameObject);
        m_mediator = new RankListViewMediator(this);
        App.Facade.RegisterMediator(m_mediator);
    }

    private void InitUIBinder(GameObject gameObject)
    {
        
    }

    public void OpenView(object intent)
    {

    }

    public void CloseView()
    {

    }

    public void DestroyView()
    {
        App.Facade.RemoveMediator(m_mediator.MediatorName);
    }

    #region Private Method

    #endregion

    #region Push Callback

    #endregion

    #region OnClick

    #endregion 
}

