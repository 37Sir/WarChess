using PureMVC.Patterns.Proxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class UserDataProxy:Proxy
{
    public new static string NAME = "UserDataProxy";
    private UserDataPackage m_userData;

    public UserDataProxy() : base(NAME)
    {
        m_userData = new UserDataPackage();
    }

    public override void OnRegister()
    {
        Debug.Log("User Data Proxy Registed");
    }

    public void SetPlayerInfo()
    {
        m_userData.UserName = "liujialiang";
        m_userData.UserId = 1111;
        m_userData.Gold = 9999;
    }

    public string GetPlayerName()
    {
        return m_userData.UserName;
    }
}

