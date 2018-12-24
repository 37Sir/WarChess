using com.zyd.common.proto.client;
using Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class RankListPanel
{

    private GameObject m_object;
    private RankListPanelMediator m_mediator;
    private UserDataProxy m_proxy;//todo
    private PVPProxy m_pvpProxy;
    private PVEProxy m_pveProxy;

    private GameObject m_MaskImage;
    private Button m_MaskImageButton;
    private Button m_Close;
    private Text m_SelfName;
    private Text m_SelfScore;
    private Text m_SelfIndex;
    private Text m_Score;
    private Text m_IndexText;
    private Text m_UserName;
    private Image m_Icon;

    private float m_offsetY = -75;

    private GameObject m_Cells;
    private RectTransform m_CellsRect;
    private GameObject m_Cell;
    public void InitView(GameObject gameObject)
    {
        m_object = gameObject;
        InitUIBinder(gameObject);
        m_mediator = new RankListPanelMediator(this);
        App.Facade.RegisterMediator(m_mediator);
        m_proxy = App.Facade.RetrieveProxy("UserDataProxy") as UserDataProxy;
        m_pvpProxy = App.Facade.RetrieveProxy("PVPProxy") as PVPProxy;
        m_Close.onClick.AddListener(OnCloseButtonClick);

        App.Facade.RegisterCommand(NotificationConstant.ShowRankList, () => new ShowRankListCommand());
    }

    private void InitUIBinder(GameObject gameObject)
    {
        m_MaskImage = gameObject.transform.Find("m_MaskImage").gameObject;
        m_MaskImageButton = m_MaskImage.GetComponent<Button>();
        m_Close = gameObject.transform.Find("Container/m_Close").GetComponent<Button>();
        m_SelfName = gameObject.transform.Find("Container/RankListContainer/SelfRank/RankContainer/m_SelfName").GetComponent<Text>();
        m_SelfScore = gameObject.transform.Find("Container/RankListContainer/SelfRank/RankContainer/m_SelfScore").GetComponent<Text>();
        m_SelfIndex = gameObject.transform.Find("Container/RankListContainer/SelfRank/RankContainer/m_SelfIndex").GetComponent<Text>();
        m_UserName = gameObject.transform.Find("Container/RankListContainer/RankList/Cells/Cell/RankContainer/m_UserName").GetComponent<Text>();
        m_Score = gameObject.transform.Find("Container/RankListContainer/RankList/Cells/Cell/RankContainer/m_Score").GetComponent<Text>();
        m_IndexText = gameObject.transform.Find("Container/RankListContainer/RankList/Cells/Cell/RankContainer/m_IndexText").GetComponent<Text>();
        
        m_Cells = gameObject.transform.Find("Container/RankListContainer/RankList/Cells").gameObject;
        m_CellsRect = m_Cells.GetComponent<RectTransform>();
        m_Cell = gameObject.transform.Find("Container/RankListContainer/RankList/Cells/Cell").gameObject;
    }

    public void OpenView(object intent)
    {
        m_SelfName.text = m_proxy.GetPlayerName();
        var index = m_proxy.GetPlayerIndex();
        if (index > 10)
        {
            m_SelfIndex.text = "未上榜";
            m_SelfIndex.fontSize = 22;
        }
        else
        {
            m_SelfIndex.text = index.ToString();
            m_SelfIndex.fontSize = 55;
        }
        m_SelfScore.text = m_proxy.GetPlayerRank().ToString();
        m_Cell.SetActive(false);
        m_mediator.NotifyShowRankList();
    }

    public void UpdateRankList()
    {
        var rankListData = m_pvpProxy.GetRankListData();
        var index = 0;
        foreach(RankList rankData in rankListData)
        {
            GameObject obj = DynamicCell(m_Cell, index);
            RankListCellView rankListCellView = new RankListCellView();
            rankListCellView.InitView(obj, rankData);
            index++;
        }
    }

    public void UpdateRank()
    {
        m_SelfName.text = m_proxy.GetPlayerName();
        var index = m_proxy.GetPlayerIndex();
        if (index > 10)
        {
            m_SelfIndex.text = "未上榜";
            m_SelfIndex.fontSize = 22;
        }
        else
        {
            m_SelfIndex.text = index.ToString();
            m_SelfIndex.fontSize = 55;
        }
        m_SelfScore.text = m_proxy.GetPlayerRank().ToString();
    }

    GameObject DynamicCell(GameObject cell, int index)
    {
        GameObject go = GameObject.Instantiate(cell);
        go.transform.SetParent(m_Cells.transform);
        go.SetActive(false);
        go.transform.localPosition = Vector3.zero;
        RectTransform rectTrans = go.GetComponent<RectTransform>();      
        go.transform.localScale = Vector3.one;
        go.transform.localRotation = Quaternion.Euler(Vector3.zero);
        rectTrans.anchoredPosition = new Vector2(0, m_offsetY * index);
        m_CellsRect.sizeDelta = new Vector2(0, Math.Abs(m_offsetY) * (index + 1));
        return go;
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
    private void OnCloseButtonClick()
    {
        App.UIManager.ClosePanel(m_object.name);
    }
    #endregion 
}

