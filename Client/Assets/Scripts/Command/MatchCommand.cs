using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.zyd.common.proto.client;
using Framework;
using PureMVC.Interfaces;
using PureMVC.Patterns.Command;
using UnityEngine;

public class MatchCommand : SimpleCommand
{
    public const string NAME = "MatchCommand";

    public override void Execute(INotification notification)
    {
        var type = 0;
        var body = notification.Body;
        if (body != null)
        {
            type = (int)body;
        }
        Debug.Log("MatchCommand Execute!");
        Request(type);
    }

    private void Request(int type)
    {     
        PlayerMatchRequest.Builder request = PlayerMatchRequest.CreateBuilder();
        request.SetType(type);
        var bytes = request.Build().ToByteArray();       
        App.NetworkManager.Request("PlayerMatch", bytes, Response);
    }

    private void Response(int error, List<byte[]> btData)
    {
        var response = PlayerMatchResponse.ParseFrom(btData[0]);
        var type = response.Type;
        SendNotification(NotificationConstant.MatchResponse, type);
    }
}
