using Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class LobbyPanel
{
    private Button m_PVP;
    private InputField m_ip;
    private InputField m_port;
    private InputField m_userName;

    private LobbyPanelMediator m_mediator;
    private UserDataProxy m_proxy;//todo

    public void InitView(GameObject gameObject)
    {
        Debug.Log("Lobby Panel Inited");
        m_mediator = new LobbyPanelMediator(this);
        m_proxy = new UserDataProxy();
        App.Facade.RegisterMediator(m_mediator);
        App.Facade.RegisterProxy(m_proxy);

        m_PVP = gameObject.transform.Find("m_PVP").gameObject.GetComponent<Button>();

        m_PVP.onClick.AddListener(OnPVPClick);
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

    private void OnPVPClick()
    {
        m_mediator.NotifyBeginMatch();
    }
}

