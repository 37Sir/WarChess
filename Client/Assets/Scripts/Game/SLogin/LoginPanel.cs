using Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class LoginPanel
{
    private Button m_login;
    private LoginPanelMediator m_mediator;
    private UserDataProxy m_proxy;//todo

    public void InitView(GameObject gameObject)
    {     
        Debug.Log("login panel Inited");
        m_mediator = new LoginPanelMediator(this);
        m_proxy = new UserDataProxy();
        App.Facade.RegisterMediator(m_mediator);
        App.Facade.RegisterProxy(m_proxy);
        m_login = gameObject.transform.Find("m_Login").gameObject.GetComponent<Button>();
        m_login.onClick.AddListener(OnLoginClick);

        App.Facade.RegisterCommand(NotificationConstant.LevelUp, () => new LobbyConnectCommand());
    }

    public void OpenView()
    {
        Debug.Log("login panel Opened");
    }

    private void OnLoginClick()
    {
        m_mediator.NotifyLevelUp();
    }
}

