﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.zyd.common.proto.client;
using Framework;
using PureMVC.Interfaces;
using PureMVC.Patterns.Command;
using UnityEngine;

public class DoMoveCommand : SimpleCommand
{
    public const string NAME = "DoMoveCommand";

    public override void Execute(INotification notification)
    {
        Debug.Log("DoMoveCommand Execute!");
        Request();
    }

    private void Request()
    {
        //LoginRequest.Builder request = LoginRequest.CreateBuilder();
        //request.SetUserName(userName);
        //var bytes = request.Build().ToByteArray();
        //App.NetworkManager.Request("Login", bytes, Response);
    }

    private void Response(int error, List<byte[]> btData)
    {
        //var response = LoginResponse.ParseFrom(btData[0]);
        //var playerInfo = response.PlayerInfo;
        //var userName = playerInfo.UserName;
        //var userId = playerInfo.UserId;

        //App.NetworkManager.UserId = userId;
        //Debug.Log("Login Success ! UserName: " + userName + " UserId: " + userId);

        //SendNotification(NotificationConstant.LoginResponse);
    }
}
