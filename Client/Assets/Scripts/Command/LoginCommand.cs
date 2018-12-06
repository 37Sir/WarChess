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

    public override void Execute(INotification notification)
    {
        Debug.Log("LoginCommand Execute!");
        Request("liujialiang");
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

        App.NetworkManager.UserId = userId;
        Debug.Log("Login Success ! UserName: " + userName + " UserId: " + userId);

        SendNotification(NotificationConstant.LoginResponse);
    }
}
