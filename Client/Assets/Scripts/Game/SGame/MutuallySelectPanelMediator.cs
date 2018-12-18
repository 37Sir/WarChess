using PureMVC.Interfaces;
using PureMVC.Patterns.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class MutuallySelectPanelMediator : Mediator
{
    public new const string NAME = "MutuallySelectPanelMediator";
    private MutuallySelectPanel m_viewComponent;

    public MutuallySelectPanelMediator(MutuallySelectPanel mutuallySelectPanel) : base(NAME)
    {
        m_viewComponent = mutuallySelectPanel;
    }

    public override void OnRegister()
    {
        base.OnRegister();
    }

    public override IList<string> ListNotificationInterests()
    {
        IList<string> list = new List<string>();
        return list;
    }

    public override void HandleNotification(INotification notification)
    {
        switch (notification.Name)
        {
            default:
                break;
        }
    }

    public void NotifySelect(bool result)
    {
        SendNotification(NotificationConstant.PlayerMutuallyFeedback, result);
    }
}

