using com.zyd.common.proto.client;
using Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class UserSimpleInfoPanel
{
    private Button m_UserButton;
    private Button m_RankButton;

    private Text m_UserRank;
    private Text m_UserName;
    private Image m_UserIcon;
    private Text m_Index;

    private GameObject m_object;
    private UserSimpleInfoPanelMediator m_mediator;
    private UserDataProxy m_proxy;//todo
    private PVPProxy m_pvpProxy;
    private PVEProxy m_pveProxy;

    public void InitView(GameObject gameObject)
    {
        m_object = gameObject;
        InitUIBinder(gameObject);
        m_mediator = new UserSimpleInfoPanelMediator(this);
        App.Facade.RegisterMediator(m_mediator);
        m_proxy = App.Facade.RetrieveProxy("UserDataProxy") as UserDataProxy;

        m_UserButton.onClick.AddListener(OnUserButtonClick);
        m_RankButton.onClick.AddListener(OnRankButtonClick);
    }

    private void InitUIBinder(GameObject gameObject)
    {
        m_UserIcon = gameObject.transform.Find("m_UserInfoWidget/m_UserIcon").GetComponent<Image>();
        m_UserButton = gameObject.transform.Find("m_UserInfoWidget").GetComponent<Button>();
        m_RankButton = gameObject.transform.Find("RankList").GetComponent<Button>();
        m_UserRank = gameObject.transform.Find("RankList/RankImage/m_Rank").GetComponent<Text>();
        m_UserName = gameObject.transform.Find("m_UserInfoWidget/m_UserName").GetComponent<Text>();
        m_Index = gameObject.transform.Find("RankList/m_Index").GetComponent<Text>();
    }

    public void OpenView(object intent)
    {
        m_UserName.text = m_proxy.GetPlayerName();
        m_UserRank.text = m_proxy.GetPlayerRank().ToString();
        var index = m_proxy.GetPlayerIndex();
        if (index > 10)
        {
            m_Index.text = "未上榜";
        }
        else
        {
            m_Index.text = "第"+index+"名";
        }       
    }

    public void UpdateRank(int rank, int index)
    {
        m_UserRank.text = rank.ToString();
        if (index > 10)
        {
            m_Index.text = "未上榜";
        }
        else
        {
            m_Index.text = "第" + index + "名";
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

    #region Push Callback

    #endregion

    #region OnClick
    private void OnUserButtonClick()
    {
        App.SoundManager.PlaySoundClip(Config.Sound.Click1);
        App.UIManager.OpenPanel("UserInfoPanel");
    }

    private void OnRankButtonClick()
    {
        App.SoundManager.PlaySoundClip(Config.Sound.Click1);
        App.UIManager.OpenPanel("RankListPanel");
    }
    #endregion 
}

