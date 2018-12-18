using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.zyd.common.proto.client;
using Framework;
using PureMVC.Interfaces;
using PureMVC.Patterns.Command;
using UnityEngine;

public class PlayerMutuallyCommand : SimpleCommand
{
    public const string NAME = "PlayerMutuallyCommand";
    private int m_type;

    public override void Execute(INotification notification)
    {
        object body = notification.Body;
        Request((int)body);
    }

    private void Request(int type)
    {
        PlayerMutuallyRequest.Builder request = PlayerMutuallyRequest.CreateBuilder();        
        request.SetType(type);
        var bytes = request.Build().ToByteArray();
        App.NetworkManager.Request("PlayerMutually", bytes, Response);
    }

    private void Response(int error, List<byte[]> btData)
    {
    }
}
