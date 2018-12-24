using PureMVC.Interfaces;
using PureMVC.Patterns.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class UserSimpleInfoPanelMediator : Mediator
{
    public new const string NAME = "UserSimpleInfoPanelMediator";
    private UserSimpleInfoPanel m_viewComponent;

    public UserSimpleInfoPanelMediator(UserSimpleInfoPanel lobbyPanel) : base(NAME)
    {
        m_viewComponent = lobbyPanel;
    }

    public override void OnRegister()
    {
        base.OnRegister();
    }

    public override IList<string> ListNotificationInterests()
    {
        IList<string> list = new List<string>();
        list.Add(NotificationConstant.RankUpdate);
        return list;
    }

    public override void HandleNotification(INotification notification)
    {
        object body = notification.Body;
        switch (notification.Name)
        {
            case NotificationConstant.RankUpdate:
                var args = (object[])body;
                m_viewComponent.UpdateRank((int)args[0], (int)args[1]);
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

    public void NotifyCancelMatch()
    {
        SendNotification(NotificationConstant.CancelMatch);
    }
}

