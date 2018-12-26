using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.zyd.common.proto.client;
using Framework;
using PureMVC.Interfaces;
using PureMVC.Patterns.Command;
using UnityEngine;

public class NewEndTurnCommand : SimpleCommand
{
    public const string NAME = "NewEndTurnCommand";

    public override void Execute(INotification notification)
    {
        Debug.Log("NewEndTurnCommand Execute!");
        Request();
    }

    private void Request()
    {
        PlayerInitiativeEndRequest.Builder request = PlayerInitiativeEndRequest.CreateBuilder();
        var bytes = request.Build().ToByteArray();
        App.NetworkManager.Request("PlayerInitiativeEnd", bytes, Response);
    }

    private void Response(int error, List<byte[]> btData)
    {
        SendNotification(NotificationConstant.NewEndTurnResponse);
    }
}
