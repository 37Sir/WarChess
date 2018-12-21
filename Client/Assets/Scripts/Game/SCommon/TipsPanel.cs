using com.zyd.common.proto.client;
using Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class TipsPanel
{
    private Button m_BottomButton;
    private Text m_Title;
    private Text m_Content;

    private GameObject m_object;
    private TipsPanelMediator m_mediator;
    private UserDataProxy m_proxy;//todo
    private PVPProxy m_pvpProxy;
    private PVEProxy m_pveProxy;

    public void InitView(GameObject gameObject)
    {
        m_object = gameObject;
        InitUIBinder(gameObject);
        m_mediator = new TipsPanelMediator(this);
        App.Facade.RegisterMediator(m_mediator);
        m_proxy = App.Facade.RetrieveProxy("UserDataProxy") as UserDataProxy;

        m_BottomButton.onClick.AddListener(OnCloseButtonClick);
    }

    private void InitUIBinder(GameObject gameObject)
    {
        m_BottomButton = gameObject.transform.Find("ContentContainer/m_Bottom/m_BottomBtn").GetComponent<Button>();
        m_Title = gameObject.transform.Find("ContentContainer/m_Name").GetComponent<Text>();
        m_Content = gameObject.transform.Find("ContentContainer/Content/m_Tips").GetComponent<Text>();
    }

    public void OpenView(object intent)
    {
        var temp = (object[])intent;
        var title = (string)temp[0];
        var content = (string)temp[1];

        m_Title.text = title;
        m_Content.text = content;
    }

    public void CloseView()
    {

    }

    public void DestroyView()
    {
        App.Facade.RemoveMediator(m_mediator.MediatorName);
    }

    #region OnClick

    private void OnCloseButtonClick()
    {        
        App.UIManager.ClosePanel(m_object.name);
        App.NSceneManager.LoadScene("SLobby");
    }

    #endregion
}

