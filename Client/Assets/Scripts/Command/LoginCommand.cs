using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.zyd.common.proto.client;
using Framework;
using PureMVC.Interfaces;
using PureMVC.Patterns.Command;
using UnityEngine;

public class LoginCommand : SimpleCommand
{
    public const string NAME = "LoginCommand";
    private UserDataProxy m_userProxy;

    public override void Execute(INotification notification)
    {
        string userName = notification.Body.ToString();
        m_userProxy = Facade.RetrieveProxy("UserDataProxy") as UserDataProxy;
        Debug.Log("LoginCommand Execute!");
        Request(userName);
    }

    private void Request(string userName)
    {
        LoginRequest.Builder request = LoginRequest.CreateBuilder();
        request.SetUserName(userName);
        var bytes = request.Build().ToByteArray();
        App.NetworkManager.Request("Login", bytes, Response);
    }

    private void Response(int error, List<byte[]> btData)
    {     
        var response = LoginResponse.ParseFrom(btData[0]);
        var playerInfo = response.PlayerInfo;
        var userName = playerInfo.UserName;
        var userId = playerInfo.UserId;
        var rank = playerInfo.Rank;
        var winning = playerInfo.Winning;
        var losing = playerInfo.Losing;
        var winCount = playerInfo.WinCount;
        var loseCount = playerInfo.LoseCount;
        var draw = playerInfo.Draw;

        m_userProxy.SetPlayerInfo(userName, userId, rank, winning, losing, winCount, loseCount, draw);
        App.NetworkManager.UserId = userId;
        Debug.Log("Login Success ! UserName: " + userName + " UserId: " + userId);

        SendNotification(NotificationConstant.LoginResponse);
    }
}
