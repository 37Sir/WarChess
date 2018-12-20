using com.zyd.common.proto.client;
using Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class RankListCellView
{
    private Button m_CloseButton;

    private GameObject m_object;
    private RankListCellViewMediator m_mediator;
    private UserDataProxy m_proxy;//todo
    private PVPProxy m_pvpProxy;
    private PVEProxy m_pveProxy;

    private RankList m_rankData;

    private Text m_Score;
    private Text m_IndexText;
    private Text m_UserName;


    public void InitView(GameObject gameObject, RankList rankData)
    {
        m_object = gameObject;
        InitUIBinder(gameObject);
        m_mediator = new RankListCellViewMediator(this);
        App.Facade.RegisterMediator(m_mediator);
        m_rankData = rankData;

        DrawCell();
    }

    private void InitUIBinder(GameObject gameObject)
    {
        m_UserName = gameObject.transform.Find("RankContainer/m_UserName").GetComponent<Text>();
        m_Score = gameObject.transform.Find("RankContainer/m_Score").GetComponent<Text>();
        m_IndexText = gameObject.transform.Find("RankContainer/m_IndexText").GetComponent<Text>();
    }

    private void DrawCell()
    {
        m_UserName.text = m_rankData.userName;
        m_Score.text = m_rankData.rank.ToString();
        m_IndexText.text = m_rankData.index.ToString();
        m_object.SetActive(true);
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

