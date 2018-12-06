using PureMVC.Interfaces;
using PureMVC.Patterns.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class LobbyPanelMediator : Mediator
{
    public new const string NAME = "LobbyPanelMediator";
    private LobbyPanel m_viewComponent;

    public LobbyPanelMediator(LobbyPanel lobbyPanel) : base(NAME)
    {
        m_viewComponent = lobbyPanel;
    }

    public override void OnRegister()
    {
        base.OnRegister();
        Debug.Log("Lobby panel mediator on registed");
    }

    public override IList<string> ListNotificationInterests()
    {
        IList<string> list = new List<string>();
        list.Add(NotificationConstant.MatchResponse);
        return list;
    }

    public override void HandleNotification(INotification notification)
    {
        switch (notification.Name)
        {
            case NotificationConstant.LevelUp:
                Debug.Log("Level Up!!");
                break;
            case NotificationConstant.MatchResponse:
                m_viewComponent.OnResponseMatch();
                break;
            default:
                break;
        }
    }

    public void NotifyLevelUp()
    {
        SendNotification(NotificationConstant.LevelUp);
    }

    public void NotifyLobbyConnect()
    {
        SendNotification(NotificationConstant.LobbyConnect);
    }

    public void NotifyBeginMatch()
    {
        SendNotification(NotificationConstant.Match);
    }
}

