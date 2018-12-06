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
        Debug.Log("MatchCommand Execute!");
        Request();
    }

    private void Request()
    {     
        PlayerMatchRequest.Builder request = PlayerMatchRequest.CreateBuilder();
        var bytes = request.Build().ToByteArray();
        App.NetworkManager.Request("PlayerMatch", bytes, Response);
    }

    private void Response(int error, List<byte[]> btData)
    {
        Debug.Log("Begin Match");
    }
}
