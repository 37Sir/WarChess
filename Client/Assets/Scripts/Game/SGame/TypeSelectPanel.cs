using com.zyd.common.proto.client;
using Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class TypeSelectPanel
{
    private Button m_BottomButton;
    private Button m_N;
    private Button m_R;
    private Button m_B;
    private Button m_Q;

    private int m_selectType = -1;

    private Text m_SearchTime;
    private InputField m_ip;
    private InputField m_port;
    private InputField m_userName;

    private IEnumerator m_matchTimer;   //计时器

    private GameObject m_object;
    private TypeSelectPanelMediator m_mediator;
    private UserDataProxy m_proxy;//todo
    private PVPProxy m_pvpProxy;
    private PVEProxy m_pveProxy;

    public void InitView(GameObject gameObject)
    {
        InitUIBinder(gameObject);
        m_BottomButton.onClick.AddListener(OnConfirmClick);
        m_N.onClick.AddListener(OnNClick);
        m_R.onClick.AddListener(OnRClick);
        m_B.onClick.AddListener(OnBClick);
        m_Q.onClick.AddListener(OnQClick);

        m_object = gameObject;
        m_mediator = new TypeSelectPanelMediator(this);
        m_pvpProxy = new PVPProxy();
        m_pveProxy = new PVEProxy();
        App.Facade.RegisterMediator(m_mediator);
        m_proxy = App.Facade.RetrieveProxy("UserDataProxy") as UserDataProxy;
    }

    private void InitUIBinder(GameObject gameObject)
    {
        m_BottomButton = gameObject.transform.Find("ContentContainer/m_Bottom/m_BottomBtn").gameObject.GetComponent<Button>();
        m_N = gameObject.transform.Find("ContentContainer/SelectInfoContainer/N").gameObject.GetComponent<Button>();
        m_R = gameObject.transform.Find("ContentContainer/SelectInfoContainer/R").gameObject.GetComponent<Button>();
        m_B = gameObject.transform.Find("ContentContainer/SelectInfoContainer/B").gameObject.GetComponent<Button>();
        m_Q = gameObject.transform.Find("ContentContainer/SelectInfoContainer/Q").gameObject.GetComponent<Button>();
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
    private void StartMatchTimer()
    {
        if (m_matchTimer == null)
        {
            m_matchTimer = _OnMatchTimer();
            App.UIManager.StartCoroutine(m_matchTimer);
        }
    }

    private IEnumerator _OnMatchTimer()
    {
        for (int i = 0; i < Config.Game.WaitingFindEnemy; i++)
        {
            m_SearchTime.text = "(" + (Config.Game.WaitingFindEnemy - i) + "s)";
            yield return new WaitForSeconds(1);
        }
    }

    private void StopMatchTimer()
    {
        if (m_matchTimer != null)
        {
            App.UIManager.StopCoroutine(m_matchTimer);
            m_matchTimer = null;
        }
    }
    #endregion

    #region Push Callback

    #endregion

    #region OnClick
    private void OnConfirmClick()
    {
        if(m_selectType != -1)
        {
            m_mediator.NotifySelectType(m_selectType);
            App.UIManager.BackPanel();
        }      
    }

    private void OnNClick()
    {
        m_selectType = 1;
        m_N.transform.Find("m_ActiveMask").gameObject.SetActive(true);
        m_R.transform.Find("m_ActiveMask").gameObject.SetActive(false);
        m_B.transform.Find("m_ActiveMask").gameObject.SetActive(false);
        m_Q.transform.Find("m_ActiveMask").gameObject.SetActive(false);
    }
    private void OnRClick()
    {
        m_selectType = 2;
        m_N.transform.Find("m_ActiveMask").gameObject.SetActive(false);
        m_R.transform.Find("m_ActiveMask").gameObject.SetActive(true);
        m_B.transform.Find("m_ActiveMask").gameObject.SetActive(false);
        m_Q.transform.Find("m_ActiveMask").gameObject.SetActive(false);
    }
    private void OnBClick()
    {
        m_selectType = 3;
        m_N.transform.Find("m_ActiveMask").gameObject.SetActive(false);
        m_R.transform.Find("m_ActiveMask").gameObject.SetActive(false);
        m_B.transform.Find("m_ActiveMask").gameObject.SetActive(true);
        m_Q.transform.Find("m_ActiveMask").gameObject.SetActive(false);
    }
    private void OnQClick()
    {
        m_selectType = 4;
        m_N.transform.Find("m_ActiveMask").gameObject.SetActive(false);
        m_R.transform.Find("m_ActiveMask").gameObject.SetActive(false);
        m_B.transform.Find("m_ActiveMask").gameObject.SetActive(false);
        m_Q.transform.Find("m_ActiveMask").gameObject.SetActive(true);
    }
    #endregion 
}

