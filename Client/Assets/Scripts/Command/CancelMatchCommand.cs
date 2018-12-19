using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.zyd.common.proto.client;
using Framework;
using PureMVC.Interfaces;
using PureMVC.Patterns.Command;
using UnityEngine;

public class CancelMatchCommand : SimpleCommand
{
    public const string NAME = "CancelMatchCommand";

    public override void Execute(INotification notification)
    {
        Debug.Log("CancelMatchCommand Execute!");
        Request();
    }

    private void Request()
    {
        PlayerCancelMatchRequest.Builder request = PlayerCancelMatchRequest.CreateBuilder();
        var bytes = request.Build().ToByteArray();
        App.NetworkManager.Request("PlayerCancelMatch", bytes, Response);
    }

    private void Response(int error, List<byte[]> btData)
    {
        SendNotification(NotificationConstant.CancelMatchResponse);
    }
}
