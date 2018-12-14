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
    private LoginPanel m_viewComponent;

    public LoginPanelMediator(LoginPanel loginPanel) : base(NAME)
    {
        m_viewComponent = loginPanel;
    }

    public override void OnRegister()
    {
        base.OnRegister();
        Debug.Log("login panel mediator on registed");
    }

    public override IList<string> ListNotificationInterests()
    {
        IList<string> list = new List<string>();
        list.Add(NotificationConstant.LoginResponse);
        return list;
    }

    public override void HandleNotification(INotification notification)
    {
        switch (notification.Name)
        {
            case NotificationConstant.LoginResponse:
                m_viewComponent.OnLoginSuccess();
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

    public void NotifyLogin(string userName)
    {
        SendNotification(NotificationConstant.Login, userName);
    }
}

