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
    private Button m_PVP02;
    private Button m_PVP02Local;
    private Button m_Easy;
    private Button m_Normal;
    private Button m_Hard;
    private Button m_cancel;
    private GameObject m_Mode;
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

    private int m_GameMode = -1;

    private GameObject m_object;
    private LobbyPanelMediator m_mediator;
    private UserDataProxy m_proxy;//todo
    private PVPProxy m_pvpProxy;
    private PVP02Proxy m_pvp02Proxy;
    private PVEProxy m_pveProxy;

    public void InitView(GameObject gameObject)
    {
        Debug.Log("Lobby Panel Inited");
        m_object = gameObject;
        InitUIBinder(gameObject);
        m_mediator = new LobbyPanelMediator(this);
        m_pvpProxy = App.Facade.RetrieveProxy("PVPProxy") as PVPProxy;
        if (m_pvpProxy == null)
        {
            m_pvpProxy = new PVPProxy();
            App.Facade.RegisterProxy(m_pvpProxy);
        }
        m_pvp02Proxy = App.Facade.RetrieveProxy("PVP02Proxy") as PVP02Proxy;
        if (m_pvp02Proxy == null)
        {
            m_pvp02Proxy = new PVP02Proxy();
            App.Facade.RegisterProxy(m_pvp02Proxy);
        }

        m_pveProxy = App.Facade.RetrieveProxy("PVEProxy") as PVEProxy;
        if (m_pveProxy == null)
        {
            m_pveProxy = new PVEProxy();
            App.Facade.RegisterProxy(m_pveProxy);
        }
        
        App.Facade.RegisterMediator(m_mediator);       
        m_proxy = App.Facade.RetrieveProxy("UserDataProxy") as UserDataProxy;

        m_PVP.onClick.AddListener(OnPVPClick);
        m_PVE.onClick.AddListener(OnPVEClick);
        m_PVP02.onClick.AddListener(OnPVP02Click);
        m_PVP02Local.onClick.AddListener(OnPVP02LocalClick);
        m_Easy.onClick.AddListener(OnEasyClick);
        m_Hard.onClick.AddListener(OnHardClick);
        m_Normal.onClick.AddListener(OnNormalClick);
        m_cancel.onClick.AddListener(OnCancelClick);
        App.Facade.RegisterCommand(NotificationConstant.Match, () => new MatchCommand());
        App.Facade.RegisterCommand(NotificationConstant.CancelMatch, () => new CancelMatchCommand());
        App.NetworkManager.RegisterPushCall(Config.PushMessage.MatchSuccess, OnMatchSuccess);
    }

    private void InitUIBinder(GameObject gameObject)
    {
        m_bird = gameObject.transform.Find("m_Bird").GetComponent<Image>();
        m_birdTween = m_bird.GetComponent<TweenPlayer>();
        m_Mode = gameObject.transform.Find("m_Mode").gameObject;
        m_Select = gameObject.transform.Find("m_Select").gameObject;
        m_Searching = gameObject.transform.Find("m_Searching").gameObject;
        m_PVE = gameObject.transform.Find("m_Select/m_PVE").gameObject.GetComponent<Button>();
        m_PVP = gameObject.transform.Find("m_Select/m_PVP").gameObject.GetComponent<Button>();
        m_PVP02 = gameObject.transform.Find("m_Select/m_PVP02").gameObject.GetComponent<Button>();
        m_PVP02Local = gameObject.transform.Find("m_Select/m_PVP02Test").gameObject.GetComponent<Button>();
        m_cancel = gameObject.transform.Find("m_Searching/m_CancelBtn").gameObject.GetComponent<Button>();
        m_SearchTime = gameObject.transform.Find("m_Searching/m_SearchContainer/m_SearchTime").gameObject.GetComponent<Text>();
        m_Easy = gameObject.transform.Find("m_Mode/m_Easy").gameObject.GetComponent<Button>();
        m_Normal = gameObject.transform.Find("m_Mode/m_Normal").gameObject.GetComponent<Button>();
        m_Hard = gameObject.transform.Find("m_Mode/m_Hard").gameObject.GetComponent<Button>();
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
        RemovePush();
        App.Facade.RemoveMediator(m_mediator.MediatorName);

    }

    private void RemovePush()
    {
        App.NetworkManager.RemovePushCall(Config.PushMessage.MatchSuccess);
    }

    /// <summary>
    /// 开始匹配
    /// </summary>
    public void OnResponseMatch(int type)
    {
        m_GameMode = type;
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
            if (m_SearchTime != null)
            {
                m_SearchTime.text = "(" + (Config.Game.WaitingFindEnemy - i) + "s)";
            }
            yield return new WaitForSeconds(1);
        }
        OnCancelClick();
    }

    private IEnumerator _OnMatchBird()
    {
        for (int i = 0; i < Config.Game.WaitingFindEnemy * 2; i++)
        {
            if (m_bird != null)
            {
                m_bird.sprite = Resources.Load<Sprite>("ArtRes/Common/c1logo" + (i % 2 + 1).ToString());
            }
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
        m_birdTween.enabled = false;
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

        StopMatchTimer();
        StopMatchBird();
        if(m_GameMode == Config.GameMode.PVP)
        {
            if (playerMes1.UserId == selfId)
            {
                m_pvpProxy.SetEnemyName(playerMes2.UserName);
            }
            else
            {
                m_pvpProxy.SetEnemyName(playerMes1.UserName);
            }
            m_pvpProxy.SetFirstId(firstHandId);
            App.NSceneManager.LoadScene("SGame");
        }
        else
        {
            if (playerMes1.UserId == selfId)
            {
                m_pvp02Proxy.SetEnemyName(playerMes2.UserName);
            }
            else
            {
                m_pvp02Proxy.SetEnemyName(playerMes1.UserName);
            }
            m_pvp02Proxy.SetFirstId(firstHandId);
            m_pvp02Proxy.SetTestMode(false);
            App.NSceneManager.LoadScene("SGame02");
        }
             
        Debug.Log("On Match Success");
    }
    #endregion

    #region OnClick
    /// <summary>
    /// 排位赛
    /// </summary>
    private void OnPVPClick()
    {
        m_GameMode = Config.GameMode.PVP;
        App.SoundManager.PlaySoundClip(Config.Sound.Click1);
        m_mediator.NotifyBeginMatch();
    }

    /// <summary>
    /// 人机对战
    /// </summary>
    private void OnPVEClick()
    {      
        m_Select.SetActive(false);
        m_Mode.SetActive(true);
        App.SoundManager.PlaySoundClip(Config.Sound.Click1);
    }

    /// <summary>
    /// 新模式
    /// </summary>
    private void OnPVP02Click()
    {
        m_GameMode = Config.GameMode.PVP02;
        m_pvp02Proxy.SetTestMode(false);
        App.SoundManager.PlaySoundClip(Config.Sound.Click1);
        m_mediator.Notify02BeginMatch();
    }

    /// <summary>
    /// 新模式本地版
    /// </summary>
    private void OnPVP02LocalClick()
    {
        App.SoundManager.PlaySoundClip(Config.Sound.Click1);
        App.NSceneManager.LoadScene("SGame02");
        m_pvp02Proxy.SetTestMode(true);
        //m_mediator.Notify02BeginMatch();
    }

    private void OnCancelClick()
    {
        m_GameMode = -1;
        m_Searching.SetActive(false);
        m_Select.SetActive(true);
        StopMatchTimer();
        StopMatchBird();
        m_mediator.NotifyCancelMatch();
    }

    private void OnEasyClick()
    {
        m_pveProxy.SetMode(0);
        App.SoundManager.PlaySoundClip(Config.Sound.Click1);
        App.Facade.RemoveMediator("LobbyPanelMediator");
        App.NSceneManager.LoadScene("SGame01");
    }

    private void OnNormalClick()
    {
        m_pveProxy.SetMode(1);
        App.SoundManager.PlaySoundClip(Config.Sound.Click1);
        App.Facade.RemoveMediator("LobbyPanelMediator");
        App.NSceneManager.LoadScene("SGame01");
        //App.UIManager.OpenPanel("TipsPanel", new object[] { "提示", "暂未开放" });
    }

    private void OnHardClick()
    {
        m_pveProxy.SetMode(2);
        App.SoundManager.PlaySoundClip(Config.Sound.Click1);
        //App.Facade.RemoveMediator("LobbyPanelMediator");
        //App.NSceneManager.LoadScene("SGame01");
        App.UIManager.OpenPanel("TipsPanel", new object[] { "提示", "暂未开放" });
    }
    #endregion 
}

