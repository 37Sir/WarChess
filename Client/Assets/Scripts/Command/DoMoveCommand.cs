using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.zyd.common.proto.client;
using Framework;
using PureMVC.Interfaces;
using PureMVC.Patterns.Command;
using UnityEngine;

public class DoMoveCommand : SimpleCommand
{
    public const string NAME = "DoMoveCommand";
    private Vector2 m_from;
    private Vector2 m_to;
    private int m_type;

    public override void Execute(INotification notification)
    {
        object body = notification.Body;
        if (body != null)
        {
            object[] newBody = (object[])body;          
            float fromX = (float)newBody[0];
            float fromY = (float)newBody[1];
            m_from = new Vector2(fromX, fromY);
            m_to = (Vector2)newBody[3];
            int roundNum = (int)newBody[4];
            m_type = (int)newBody[5];
            Request(CoorToIndex((int)fromX, (int)fromY), CoorToIndex((int)m_to.x, (int)m_to.y), roundNum, m_type);
        }
    }

    private void Request(int from, int to, int roundNum, int type)
    {
        PlayerBattleMesRequest.Builder request = PlayerBattleMesRequest.CreateBuilder();
        BattleMes.Builder mes = BattleMes.CreateBuilder();
        mes.SetFrom(from);
        mes.SetTo(to);
        mes.SetPlayNum(roundNum);
        mes.SetPromption(type);
        request.SetBattleMes(mes);
        var bytes = request.Build().ToByteArray();
        App.NetworkManager.Request("PlayerBattleMes", bytes, Response);
    }

    private void Response(int error, List<byte[]> btData)
    {
        var response = PlayerBattleMesResponse.ParseFrom(btData[0]);
        var canMove = response.Res;
        var errorCode = response.Error;
        Debug.Log("Move Response: " + canMove + " errorCode " + errorCode);

        SendNotification(NotificationConstant.DoMoveResponse, new object[] {m_from, m_to, m_type});
    }

    ///坐标转index 且棋盘翻转
    private int CoorToIndex(int x, int y)
    {
        int index = 65 - (y * Config.Board.MaxX - x + 1);
        return index;
    }
}
