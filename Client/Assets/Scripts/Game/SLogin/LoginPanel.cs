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
    private InputField m_ip;
    private InputField m_port;
    private InputField m_userName;

    private LoginPanelMediator m_mediator;
    private UserDataProxy m_proxy;//todo

    public void InitView(GameObject gameObject)
    {     
        Debug.Log("login panel Inited");
        m_mediator = new LoginPanelMediator(this);
        m_proxy = new UserDataProxy();
        App.Facade.RegisterMediator(m_mediator);
        App.Facade.RegisterProxy(m_proxy);

        m_ip = gameObject.transform.Find("m_IP").gameObject.GetComponent<InputField>();
        m_port = gameObject.transform.Find("m_Port").gameObject.GetComponent<InputField>();
        m_userName = gameObject.transform.Find("m_UserName").gameObject.GetComponent<InputField>();
        m_login = gameObject.transform.Find("m_Login").gameObject.GetComponent<Button>();

        m_login.onClick.AddListener(OnLoginClick);

        App.Facade.RegisterCommand(NotificationConstant.LobbyConnect, () => new LobbyConnectCommand());
        App.Facade.RegisterCommand(NotificationConstant.Login, () => new LoginCommand());
    }

    public void OpenView()
    {
        Debug.Log("login panel Opened");
        App.NetworkManager.SetConfig("192.168.90.12", 10000, (int)Config.NetworkType.TCP, 512, 5000, 1, 1, 5000);
        m_mediator.NotifyLobbyConnect();
    }

    public void OnLoginSuccess()
    {
        Debug.Log("On Login Success");
        App.NSceneManager.LoadScene("SLobby");
    }

    private void OnLoginClick()
    {
        App.NetworkManager.Token = "liujialiang";
        
        m_mediator.NotifyLogin();
    }

    public void CloseView()
    {

    }

    public void DestroyView()
    {

    }
}

