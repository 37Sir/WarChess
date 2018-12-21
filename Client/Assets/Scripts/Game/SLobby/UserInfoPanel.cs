using com.zyd.common.proto.client;
using Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class UserInfoPanel
{
    private Button m_SwitchButton;
    private Button m_CloseButton;

    private Text m_UserRank;
    private Text m_UserIndex;
    private Text m_WinCount;
    private Text m_TotalCount;
    private Text m_UserName;
    private Image m_UserIcon;

    private GameObject m_object;
    private UserInfoPanelMediator m_mediator;
    private UserDataProxy m_proxy;//todo
    private PVPProxy m_pvpProxy;
    private PVEProxy m_pveProxy;

    public void InitView(GameObject gameObject)
    {
        m_object = gameObject;
        InitUIBinder(gameObject);
        m_mediator = new UserInfoPanelMediator(this);
        App.Facade.RegisterMediator(m_mediator);
        m_proxy = App.Facade.RetrieveProxy("UserDataProxy") as UserDataProxy;

        m_SwitchButton.onClick.AddListener(OnSwitchButtonClick);
        m_CloseButton.onClick.AddListener(OnCloseButtonClick);
    }

    private void InitUIBinder(GameObject gameObject)
    {
        m_UserIcon = gameObject.transform.Find("ContentContainer/UserInfoContainer/m_UserIcon").GetComponent<Image>();
        m_SwitchButton = gameObject.transform.Find("ContentContainer/UserInfoContainer/Bottom/m_BottomBtn").GetComponent<Button>();
        m_CloseButton = gameObject.transform.Find("ContentContainer/m_Close").GetComponent<Button>();
        m_UserRank = gameObject.transform.Find("ContentContainer/UserInfoContainer/Bottom/PVE/PVEBattleCount/m_PVEBattle").GetComponent<Text>();
        m_UserIndex = gameObject.transform.Find("ContentContainer/UserInfoContainer/Bottom/PVE/PVEWinCount/m_PVEWin").GetComponent<Text>();
        m_TotalCount = gameObject.transform.Find("ContentContainer/UserInfoContainer/Bottom/PVP/PVPBattleCount/m_PVPBattle").GetComponent<Text>();
        m_WinCount = gameObject.transform.Find("ContentContainer/UserInfoContainer/Bottom/PVP/PVPWinCount/m_PVPWin").GetComponent<Text>();

        m_UserName = gameObject.transform.Find("ContentContainer/UserInfoContainer/Left/Container/m_UserName").GetComponent<Text>();
    }

    public void OpenView(object intent)
    {
        m_UserName.text = m_proxy.GetPlayerName();
        m_UserRank.text = m_proxy.GetPlayerRank().ToString();

        var win = m_proxy.GetPlayerWinCount();
        var lose = m_proxy.GetPlayerLoseCount();
        var draw = m_proxy.GetPlayerDraw();
        m_WinCount.text = win.ToString();
        m_TotalCount.text = (draw + win + lose).ToString();
        m_UserIndex.text = m_proxy.GetPlayerIndex().ToString();
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
    private void OnSwitchButtonClick()
    {
        App.NSceneManager.LoadScene("SLogin");
    }

    private void OnCloseButtonClick()
    {
        App.UIManager.ClosePanel(m_object.name);
    }
    
    #endregion 
}

