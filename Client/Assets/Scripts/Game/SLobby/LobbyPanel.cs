using Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class LobbyPanel
{
    private Button m_PVP;
    private Button m_cancel;
    private GameObject m_Select;
    private GameObject m_Searching;
    private Text m_SearchTime;
    private InputField m_ip;
    private InputField m_port;
    private InputField m_userName;

    private IEnumerator m_matchTimer;   //计时器

    private GameObject m_object;
    private LobbyPanelMediator m_mediator;
    private UserDataProxy m_proxy;//todo

    public void InitView(GameObject gameObject)
    {
        Debug.Log("Lobby Panel Inited");
        m_object = gameObject;
        m_mediator = new LobbyPanelMediator(this);
        m_proxy = new UserDataProxy();
        App.Facade.RegisterMediator(m_mediator);
        App.Facade.RegisterProxy(m_proxy);
        m_Select = gameObject.transform.Find("m_Select").gameObject;
        m_Searching = gameObject.transform.Find("m_Searching").gameObject;
        m_PVP = gameObject.transform.Find("m_Select/m_PVP").gameObject.GetComponent<Button>();
        m_cancel = gameObject.transform.Find("m_Searching/m_CancelBtn").gameObject.GetComponent<Button>();
        m_SearchTime = gameObject.transform.Find("m_Searching/m_SearchContainer/m_SearchTime").gameObject.GetComponent<Text>();

        m_PVP.onClick.AddListener(OnPVPClick);
        m_cancel.onClick.AddListener(OnCancelClick);
        App.Facade.RegisterCommand(NotificationConstant.Match, () => new MatchCommand());
        App.NetworkManager.RegisterPushCall(Config.PushMessage.MatchSuccess, OnMatchSuccess);
        App.NetworkManager.RegisterPushCall(Config.PushMessage.OnePlayerReady, OnOnePlayerReady);
        App.NetworkManager.RegisterPushCall(Config.PushMessage.PlayerNotReady, OnPlayerNotReady);
        App.NetworkManager.RegisterPushCall(Config.PushMessage.PlayerReadyFinish, OnReadyFinish);
    }

    public void OpenView()
    {
        Debug.Log("Lobby Panel Opened");
    }

    /// <summary>
    /// 开始匹配
    /// </summary>
    public void OnResponseMatch()
    {
        m_Select.SetActive(false);
        m_Searching.SetActive(true);
        StartMatchTimer();
    }

    #region Private Method
    private void StartMatchTimer()
    {
        if(m_matchTimer == null)
        {
            m_matchTimer = _OnMatchTimer();
            App.UIManager.StartCoroutine(m_matchTimer);
        }
    }

    private IEnumerator _OnMatchTimer()
    {
        for(int i = 0; i < Config.Game.WaitingFindEnemy; i++)
        {
            m_SearchTime.text = "(" + (Config.Game.WaitingFindEnemy - i) + "s)";
            yield return new WaitForSeconds(1);
        }
    }

    private void StopMatchTimer()
    {
        if(m_matchTimer != null)
        {
            App.UIManager.StopCoroutine(m_matchTimer);
            m_matchTimer = null;
        }
    }
    #endregion

    #region Push Callback
    public void OnMatchSuccess(string name, List<byte[]> packet)
    {
        Debug.Log("On Match Success");
    }

    public void OnPlayerNotReady(string name, List<byte[]> packet)
    {
        Debug.Log("On Player Not Ready");
    }

    public void OnOnePlayerReady(string name, List<byte[]> packet)
    {
        Debug.Log("On One Player Ready");
    }

    public void OnReadyFinish(string name, List<byte[]> packet)
    {
        Debug.Log("On Ready Finish");
    }
    #endregion

    #region OnClick
    private void OnPVPClick()
    {
        m_mediator.NotifyBeginMatch();
    }

    private void OnCancelClick()
    {
        m_Searching.SetActive(false);
        m_Select.SetActive(true);
        StopMatchTimer();
    }
    #endregion 
}

