using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.zyd.common.proto.client;
using Framework;
using PureMVC.Interfaces;
using PureMVC.Patterns.Command;
using UnityEngine;

public class PlayerActiveCommand : SimpleCommand
{
    public const string NAME = "PlayerActiveCommand";
    private Vector2 m_from;
    private Vector2 m_to;
    private int m_type;

    public override void Execute(INotification notification)
    {
        Debug.Log("PlayerActiveCommandExecute!!!");
        object body = notification.Body;
        if (body != null)
        {
            Request((ActiveInfo.Builder)body);
        }
    }

    private void Request(ActiveInfo.Builder activeInfo)
    {
        PlayerActiveRequest.Builder request = PlayerActiveRequest.CreateBuilder();
        request.SetActiveInfo(activeInfo);
        var bytes = request.Build().ToByteArray();
        App.NetworkManager.Request("PlayerActive", bytes, Response);
    }

    private void Response(int error, List<byte[]> btData)
    {
        SendNotification(NotificationConstant.PlayerActiveResponse);
    }

    ///坐标转index 且棋盘翻转
    private int CoorToIndex(int x, int y)
    {
        int index = 65 - (y * Config.Board.MaxX - x + 1);
        return index;
    }
}
