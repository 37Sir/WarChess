using com.zyd.common.proto.client;
using Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class WaitingPanel
{
    private Text m_Content;
    private GameObject m_Hide;

    private GameObject m_object;
    private WaitingPanelMediator m_mediator;
    private UserDataProxy m_proxy;//todo
    private PVPProxy m_pvpProxy;
    private PVEProxy m_pveProxy;
    private TweenPlayer m_tweenPlayer;

    public void InitView(GameObject gameObject)
    {
        m_object = gameObject;
        InitUIBinder(gameObject);
        m_mediator = new WaitingPanelMediator(this);
        App.Facade.RegisterMediator(m_mediator);
        m_proxy = App.Facade.RetrieveProxy("UserDataProxy") as UserDataProxy;
    }

    private void InitUIBinder(GameObject gameObject)
    {
        m_Hide = gameObject.transform.Find("m_Hide").gameObject;
        m_Content = m_Hide.transform.Find("Text").GetComponent<Text>();
    }

    public void OpenView(object intent)
    {
        if (intent != null)
        {
            m_Hide.SetActive(true);
            var content = (string)intent;
            m_Content.text = content;
        }
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
}

