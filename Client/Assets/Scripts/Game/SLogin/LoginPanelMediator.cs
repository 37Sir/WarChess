using PureMVC.Interfaces;
using PureMVC.Patterns.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class LoginPanelMediator : Mediator
{
    public new const string NAME = "LoginPanelMediator";

    public LoginPanelMediator(LoginPanel loginPanel) : base(NAME)
    {

    }

    public override void OnRegister()
    {
        base.OnRegister();
        Debug.Log("login panel mediator on registed");
    }

    public override IList<string> ListNotificationInterests()
    {
        IList<string> list = new List<string>();
        list.Add(NotificationConstant.LevelUp);
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

    public void NotifyLevelUp()
    {
        SendNotification(NotificationConstant.LevelUp);
    }
}

