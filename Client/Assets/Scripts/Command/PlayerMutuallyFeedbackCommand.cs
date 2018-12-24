using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.zyd.common.proto.client;
using Framework;
using PureMVC.Interfaces;
using PureMVC.Patterns.Command;
using UnityEngine;

public class PlayerMutuallyFeedbackCommand : SimpleCommand
{
    public const string NAME = "PlayerMutuallyFeedbackCommand";
    private int m_type;

    public override void Execute(INotification notification)
    {
        object body = notification.Body;
        Request((bool)body);
    }

    private void Request(bool result)
    {
        Debug.Log("PlayerMutuallyFeedbackCommand!");
        PlayerMutuallyFeedbackRequest.Builder request = PlayerMutuallyFeedbackRequest.CreateBuilder();
        request.SetIsAgree(result);
        var bytes = request.Build().ToByteArray();
        App.NetworkManager.Request("PlayerMutuallyFeedback", bytes, Response);
    }

    private void Response(int error, List<byte[]> btData)
    {
    }
}
