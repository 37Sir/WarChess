using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.zyd.common.proto.client;
using Framework;
using PureMVC.Interfaces;
using PureMVC.Patterns.Command;
using UnityEngine;

public class PlayerChatCommand : SimpleCommand
{
    public const string NAME = "PlayerChatCommand";

    public override void Execute(INotification notification)
    {
        object body = notification.Body;
        if (body != null)
        {
            Request((int)body);
        }
        else
        {
            Request(0);
        }
    }

    private void Request(int index)
    {
        PlayerChatRequest.Builder request = PlayerChatRequest.CreateBuilder();
        request.SetNumber(index);
        var bytes = request.Build().ToByteArray();
        App.NetworkManager.Request("PlayerChat", bytes, Response);
    }

    private void Response(int error, List<byte[]> btData)
    {
        SendNotification(NotificationConstant.PlayerChatResponse);
    }
}
