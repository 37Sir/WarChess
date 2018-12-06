﻿using Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// 场景加载
/// </summary>
public class SceneLoader
{
    public static void SLogin()
    {
        App.UIManager.BindUIControl();
        App.UIManager.RegisterPanel("SLogin", "LoginPanel", UILayerType.NormalLayer, false);
        App.UIManager.OpenPanel("LoginPanel");
    }

    public static void SLobby()
    {
        App.UIManager.BindUIControl();
        App.UIManager.RegisterPanel("SLobby", "LobbyPanel", UILayerType.NormalLayer, false);
        App.UIManager.OpenPanel("LobbyPanel");
    }

    public static void SGame()
    {

    }
}