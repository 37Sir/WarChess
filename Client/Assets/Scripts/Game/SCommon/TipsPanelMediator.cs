using PureMVC.Interfaces;
using PureMVC.Patterns.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class TipsPanelMediator : Mediator
{
    public new const string NAME = "TipsPanelMediator";
    private TipsPanel m_viewComponent;

    public TipsPanelMediator(TipsPanel lobbyPanel) : base(NAME)
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
            default:
                break;
        }
    }
}

