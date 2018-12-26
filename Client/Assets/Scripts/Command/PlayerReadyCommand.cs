using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.zyd.common.proto.client;
using Framework;
using PureMVC.Interfaces;
using PureMVC.Patterns.Command;
using UnityEngine;

public class PlayerReadyCommand : SimpleCommand
{
    public const string NAME = "PlayerReadyCommand";

    public override void Execute(INotification notification)
    {
        object body = notification.Body;
        if(body != null)
        {
            Request((int)body);
        }
        else
        {
            Request(0);
        }
    }

    private void Request(int type)
    {
        PlayerReadyRequest.Builder request = PlayerReadyRequest.CreateBuilder();
        request.SetType(type);
        var bytes = request.Build().ToByteArray();
        App.NetworkManager.Request("PlayerReady", bytes, Response);
    }

    private void Response(int error, List<byte[]> btData)
    {
        SendNotification(NotificationConstant.PlayerReadyResponse);
    }
}
