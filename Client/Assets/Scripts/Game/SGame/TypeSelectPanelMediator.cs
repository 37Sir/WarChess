using PureMVC.Interfaces;
using PureMVC.Patterns.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class TypeSelectPanelMediator : Mediator
{
    public new const string NAME = "TypeSelectPanelMediator";
    private TypeSelectPanel m_viewComponent;

    public TypeSelectPanelMediator(TypeSelectPanel typeSelectPanel) : base(NAME)
    {
        m_viewComponent = typeSelectPanel;
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

    public void NotifySelectType(object body)
    {
        SendNotification(NotificationConstant.OnTypeSelect, body);
    }
}

