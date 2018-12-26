using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.zyd.common.proto.client;
using Framework;
using PureMVC.Interfaces;
using PureMVC.Patterns.Command;
using UnityEngine;

public class EndTurnCommand : SimpleCommand
{
    public const string NAME = "EndTurnCommand";

    public override void Execute(INotification notification)
    {
        int type = 0;
        object body = notification.Body;
        if (body != null) type = (int)body;
        Debug.Log("EndTurnCommand Execute!");
        Request(type);
    }

    private void Request(int type)
    {
        PlayerPaintingEndRequest.Builder request = PlayerPaintingEndRequest.CreateBuilder();
        request.SetType(type);
        var bytes = request.Build().ToByteArray();
        App.NetworkManager.Request("PlayerPaintingEnd", bytes, Response);
    }

    private void Response(int error, List<byte[]> btData)
    {
        Debug.Log("Response: PlayerPaintingEnd");
    }
}
