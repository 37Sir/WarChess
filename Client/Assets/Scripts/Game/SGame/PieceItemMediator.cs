using Framework;
using PureMVC.Interfaces;
using PureMVC.Patterns.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class PieceItemMediator : Mediator
{
    public new const string NAME = "PieceItemMediator";
    public Piece pieceData;//棋子数据
    private PieceItem m_viewComponent;

    public PieceItemMediator(PieceItem pieceItem) : base(NAME)
    {
        m_viewComponent = pieceItem;
    }

    public override void OnRegister()
    {
        base.OnRegister();
    }

    public override IList<string> ListNotificationInterests()
    {
        IList<string> list = new List<string>();
        list.Add(NotificationConstant.OnDragEnd);
        return list;
    }

    public override void HandleNotification(INotification notification)
    {
        var body = notification.Body;
        switch (notification.Name)
        {
            case NotificationConstant.OnDragEnd:
                if (body == null) return;
                var piece = (object[])body;
                if ((Config.PieceColor)piece[2] != pieceData.color)
                {
                    if((float)piece[0] == m_viewComponent.m_X && (float)piece[1] == m_viewComponent.m_Z)
                    {
                        m_viewComponent.BeAttached();
                    }
                }
                break;
            default:
                break;
        }
    }

    public void InitPieceData(Piece pieceData)
    {
        this.pieceData = pieceData;
    }

    public void NotityDragEnd(object[] body)
    {
        App.Facade.SendNotification(NotificationConstant.OnDragEnd, body);
    }

    public void NotityGameOver(object body)
    {
        App.Facade.SendNotification(NotificationConstant.OnGameOver, body);
    }

    public void NotifyDragTips(object body)
    {
        App.Facade.SendNotification(NotificationConstant.OnTipsShow, body);
    }
}

