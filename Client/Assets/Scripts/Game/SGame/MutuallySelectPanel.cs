using com.zyd.common.proto.client;
using Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class MutuallySelectPanel
{
    private Button m_BottomButton;
    private Button m_agree;
    private Button m_disagree;

    private bool m_select = false;

    private Text m_SelectTime;
    private InputField m_ip;
    private InputField m_port;
    private InputField m_userName;

    private IEnumerator m_selectTimer;   //计时器

    private GameObject m_object;
    private MutuallySelectPanelMediator m_mediator;
    private UserDataProxy m_proxy;//todo
    private PVPProxy m_pvpProxy;
    private PVEProxy m_pveProxy;

    public void InitView(GameObject gameObject)
    {
        InitUIBinder(gameObject);
        m_BottomButton.onClick.AddListener(OnConfirmClick);
        m_agree.onClick.AddListener(OnAgreeClick);
        m_disagree.onClick.AddListener(OnDisagreeClick);
        
        m_object = gameObject;
        m_mediator = new MutuallySelectPanelMediator(this);
        App.Facade.RegisterMediator(m_mediator);
        m_proxy = App.Facade.RetrieveProxy("UserDataProxy") as UserDataProxy;
    }

    private void InitUIBinder(GameObject gameObject)
    {
        m_BottomButton = gameObject.transform.Find("ContentContainer/m_Bottom/m_BottomBtn").gameObject.GetComponent<Button>();
        m_agree = gameObject.transform.Find("ContentContainer/SelectInfoContainer/m_agree").gameObject.GetComponent<Button>();
        m_disagree = gameObject.transform.Find("ContentContainer/SelectInfoContainer/m_disagree").gameObject.GetComponent<Button>();
        m_SelectTime = gameObject.transform.Find("ContentContainer/m_Timer").GetComponent<Text>();
    }

    public void OpenView(object intent)
    {
        StartSelectTimer();
    }

    public void CloseView()
    {

    }

    public void DestroyView()
    {
        m_BottomButton.onClick.RemoveAllListeners();
        App.Facade.RemoveMediator(m_mediator.MediatorName);
    }

    #region Private Method
    private void StartSelectTimer()
    {
        if (m_selectTimer == null)
        {
            m_selectTimer = _OnSelectTimer();
            App.UIManager.StartCoroutine(m_selectTimer);
        }
    }

    private IEnumerator _OnSelectTimer()
    {
        for (int i = 0; i < Config.Game.WaitingMutuallySelect; i++)
        {
            m_SelectTime.text = "(" + (Config.Game.WaitingMutuallySelect - i) + "s)";
            yield return new WaitForSeconds(1);
        }
        App.UIManager.BackPanel();
    }

    private void StopSelectTimer()
    {
        if (m_selectTimer != null)
        {
            App.UIManager.StopCoroutine(m_selectTimer);
            m_selectTimer = null;
        }
    }
    #endregion

    #region Push Callback

    #endregion

    #region OnClick
    private void OnConfirmClick()
    {
        if (m_select != null)
        {
            m_mediator.NotifySelect(m_select);
            StopSelectTimer();
            App.UIManager.ClosePanel(m_object.name);
        }
    }

    private void OnAgreeClick()
    {
        m_select = true;
        m_agree.transform.Find("m_ActiveMask").gameObject.SetActive(true);
        m_disagree.transform.Find("m_ActiveMask").gameObject.SetActive(false);
    }
    private void OnDisagreeClick()
    {
        m_select = false;
        m_agree.transform.Find("m_ActiveMask").gameObject.SetActive(false);
        m_disagree.transform.Find("m_ActiveMask").gameObject.SetActive(true);
    }
    #endregion 
}

