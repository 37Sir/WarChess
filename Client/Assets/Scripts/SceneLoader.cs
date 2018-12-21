using Framework;
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

        App.UIManager.RegisterPanel("SCommon", "MessagePanel", UILayerType.TopLayer, false);
    }

    public static void SLobby()
    {
        App.UIManager.BindUIControl();
        App.UIManager.RegisterPanel("SLobby", "LobbyPanel", UILayerType.NormalLayer, false);
        App.UIManager.RegisterPanel("SLobby", "UserInfoPanel", UILayerType.TopLayer, false);
        App.UIManager.RegisterPanel("SLobby", "RankListPanel", UILayerType.TopLayer, false);
        App.UIManager.RegisterPanel("SLobby", "UserSimpleInfoPanel", UILayerType.TopLayer, false);

        App.UIManager.RegisterPanel("SCommon", "TipsPanel", UILayerType.TopLayer, false);

        App.UIManager.OpenPanel("UserSimpleInfoPanel");
        App.UIManager.OpenPanel("LobbyPanel");
    }

    public static void SGame()
    {
        App.UIManager.BindUIControl();
        App.UIManager.RegisterPanel("SGame", "PVPPanel", UILayerType.NormalLayer, false);
        App.UIManager.RegisterPanel("SGame", "TypeSelectPanel", UILayerType.TopLayer, false);
        App.UIManager.RegisterPanel("SGame", "ResultPanel", UILayerType.TopLayer, false);
        App.UIManager.RegisterPanel("SGame", "MutuallySelectPanel", UILayerType.TopLayer, false);

        App.UIManager.RegisterPanel("SCommon", "MessagePanel", UILayerType.TopLayer, false);
        App.UIManager.RegisterPanel("SCommon", "TipsPanel", UILayerType.TopLayer, false);
        App.UIManager.RegisterPanel("SCommon", "WaitingPanel", UILayerType.TopLayer, false);

        App.UIManager.OpenPanel("PVPPanel");
    }

    public static void SGame01()
    {
        App.UIManager.BindUIControl();
        App.UIManager.RegisterPanel("SGame", "PVEPanel", UILayerType.NormalLayer, false);
        App.UIManager.RegisterPanel("SGame", "TypeSelectPanel", UILayerType.TopLayer, false);
        App.UIManager.RegisterPanel("SGame", "ResultPanel", UILayerType.TopLayer, false);
        App.UIManager.OpenPanel("PVEPanel");
    }
}
