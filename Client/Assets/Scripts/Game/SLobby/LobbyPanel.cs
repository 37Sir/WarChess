using com.zyd.common.proto.client;
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
    private Button m_PVE;
    private Button m_cancel;
    private GameObject m_Select;
    private GameObject m_Searching;
    private Text m_SearchTime;
    private InputField m_ip;
    private InputField m_port;
    private InputField m_userName;
    private Image m_bird;
    private TweenPlayer m_birdTween;

    private IEnumerator m_matchTimer;   //计时器
    private IEnumerator m_matchBird;    //飞鸟

    private GameObject m_object;
    private LobbyPanelMediator m_mediator;
    private UserDataProxy m_proxy;//todo
    private PVPProxy m_pvpProxy;
    private PVEProxy m_pveProxy;

    public void InitView(GameObject gameObject)
    {
        Debug.Log("Lobby Panel Inited");
        m_object = gameObject;
        InitUIBinder(gameObject);
        m_mediator = new LobbyPanelMediator(this);
        m_pvpProxy = new PVPProxy();
        m_pveProxy = new PVEProxy();
        App.Facade.RegisterMediator(m_mediator);
        App.Facade.RegisterProxy(m_pvpProxy);
        App.Facade.RegisterProxy(m_pveProxy);
        m_proxy = App.Facade.RetrieveProxy("UserDataProxy") as UserDataProxy;

        m_PVP.onClick.AddListener(OnPVPClick);
        m_PVE.onClick.AddListener(OnPVEClick);
        m_cancel.onClick.AddListener(OnCancelClick);
        App.Facade.RegisterCommand(NotificationConstant.Match, () => new MatchCommand());
        App.Facade.RegisterCommand(NotificationConstant.CancelMatch, () => new CancelMatchCommand());
        App.NetworkManager.RegisterPushCall(Config.PushMessage.MatchSuccess, OnMatchSuccess);
    }

    private void InitUIBinder(GameObject gameObject)
    {
        m_bird = gameObject.transform.Find("m_Bird").GetComponent<Image>();
        m_birdTween = m_bird.GetComponent<TweenPlayer>();
        m_Select = gameObject.transform.Find("m_Select").gameObject;
        m_Searching = gameObject.transform.Find("m_Searching").gameObject;
        m_PVE = gameObject.transform.Find("m_Select/m_PVE").gameObject.GetComponent<Button>();
        m_PVP = gameObject.transform.Find("m_Select/m_PVP").gameObject.GetComponent<Button>();
        m_cancel = gameObject.transform.Find("m_Searching/m_CancelBtn").gameObject.GetComponent<Button>();
        m_SearchTime = gameObject.transform.Find("m_Searching/m_SearchContainer/m_SearchTime").gameObject.GetComponent<Text>();

    }

    public void OpenView(object intent)
    {
        Debug.Log("Lobby Panel Opened");
    }

    public void CloseView()
    {

    }

    public void DestroyView()
    {
        App.Facade.RemoveMediator(m_mediator.MediatorName);
    }

    /// <summary>
    /// 开始匹配
    /// </summary>
    public void OnResponseMatch()
    {
        m_Select.SetActive(false);
        m_Searching.SetActive(true);
        StartMatchTimer();
        StartMatchBird();
        App.SoundManager.PlaySoundClip(Config.Sound.MatchStart);
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

    private void StartMatchBird()
    {
        if (m_matchBird == null)
        {
            m_matchBird = _OnMatchBird();
            App.UIManager.StartCoroutine(m_matchBird);
        }
        m_birdTween.enabled = true;
    }

    private IEnumerator _OnMatchTimer()
    {
        for(int i = 0; i < Config.Game.WaitingFindEnemy; i++)
        {
            m_SearchTime.text = "(" + (Config.Game.WaitingFindEnemy - i) + "s)";
            yield return new WaitForSeconds(1);
        }
    }

    private IEnumerator _OnMatchBird()
    {
        for (int i = 0; i < Config.Game.WaitingFindEnemy * 2; i++)
        {
            m_bird.sprite = Resources.Load<Sprite>("ArtRes/Common/c1logo" + (i % 2 + 1).ToString());
            yield return new WaitForSeconds(0.5f);
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

    private void StopMatchBird()
    {
        if (m_matchBird != null)
        {
            App.UIManager.StopCoroutine(m_matchBird);
            m_matchBird = null;
        }
    }
    #endregion

    #region Push Callback
    public void OnMatchSuccess(string name, List<byte[]> packet)
    {
        var push = PlayerStartPush.ParseFrom(packet[0]);
        var selfId = m_proxy.GetPlayerId();
        var playerMes1 = push.GetPlayerMes(0);
        var playerMes2 = push.GetPlayerMes(1);
        var firstHandId = push.UserId;
        if(playerMes1.UserId == selfId)
        {
            m_pvpProxy.SetEnemyName(playerMes2.UserName);
        }
        else
        {
            m_pvpProxy.SetEnemyName(playerMes1.UserName);
        }
        m_pvpProxy.SetFirstId(firstHandId);
        StopMatchTimer();
        StopMatchBird();
        App.Facade.RemoveMediator("LobbyPanelMediator");
        App.NSceneManager.LoadScene("SGame");      
        Debug.Log("On Match Success");
    }
    #endregion

    #region OnClick
    private void OnPVPClick()
    {
        App.SoundManager.PlaySoundClip(Config.Sound.Click1);
        m_mediator.NotifyBeginMatch();
    }

    private void OnPVEClick()
    {
        App.SoundManager.PlaySoundClip(Config.Sound.Click1);
        App.Facade.RemoveMediator("LobbyPanelMediator");
        App.NSceneManager.LoadScene("SGame01");
    }

    private void OnCancelClick()
    {
        m_Searching.SetActive(false);
        m_Select.SetActive(true);
        StopMatchTimer();
        m_mediator.NotifyCancelMatch();
    }
    #endregion 
}

