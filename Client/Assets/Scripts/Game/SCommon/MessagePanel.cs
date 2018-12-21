using com.zyd.common.proto.client;
using Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class MessagePanel
{
    private Text m_Content;

    private GameObject m_object;
    private MessagePanelMediator m_mediator;
    private UserDataProxy m_proxy;//todo
    private PVPProxy m_pvpProxy;
    private PVEProxy m_pveProxy;
    private TweenPlayer m_tweenPlayer;

    public void InitView(GameObject gameObject)
    {
        m_object = gameObject;
        InitUIBinder(gameObject);
        m_mediator = new MessagePanelMediator(this);
        App.Facade.RegisterMediator(m_mediator);
        m_proxy = App.Facade.RetrieveProxy("UserDataProxy") as UserDataProxy;
    }

    private void InitUIBinder(GameObject gameObject)
    {
        m_Content = gameObject.transform.Find("Image/m_Text").GetComponent<Text>();
        m_tweenPlayer = gameObject.transform.Find("Image").GetComponent<TweenPlayer>();
    }

    public void OpenView(object intent)
    {
        if (intent != null)
        {
            var content = (string)intent;
            m_Content.text = content;
        }
        var tween = m_tweenPlayer.GetClipTween("move_out");
        tween.SetOnComplete(OnShowComplete, null);
    }

    private void OnShowComplete(object[] args)
    {
        App.UIManager.ClosePanel("MessagePanel");
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

