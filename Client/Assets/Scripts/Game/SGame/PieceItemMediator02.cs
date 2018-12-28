using Framework;
using PureMVC.Interfaces;
using PureMVC.Patterns.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class PieceItem02Mediator : Mediator
{
    public new const string NAME = "PieceItem02Mediator";
    public Piece pieceData;//棋子数据
    private PieceItem02 m_viewComponent;

    public PieceItem02Mediator(PieceItem02 pieceItem, string name) : base(name + NAME)
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
        list.Add(NotificationConstant.OnOtherMove);
        list.Add(NotificationConstant.OnMoveEnd);
        list.Add(NotificationConstant.OnTypeSelect);
        list.Add(NotificationConstant.RoundBegin);
        return list;
    }

    public override void HandleNotification(INotification notification)
    {
        if (m_viewComponent.isDead == true) return;//todo
        var body = notification.Body;
        switch (notification.Name)
        {
            case NotificationConstant.OnOtherMove:
                var move = (Vector2[])body;
                m_viewComponent.DoMove((Vector2)move[0], (Vector2)move[1], (Vector2)move[2]);
                break;

            case NotificationConstant.OnMoveEnd:
                
                Vector2[] endMove = (Vector2[])body;
                var from = endMove[0];
                var to = endMove[1];
                var type = (int)endMove[2].x;
                Debug.Log("OnMoveEnd: from ==" + from + " to== " + to + "m_X:" + m_viewComponent.m_X + "m_Z: " + m_viewComponent.m_Z);
                if (to.x == m_viewComponent.m_X && to.y == m_viewComponent.m_Z)
                {
                    m_viewComponent.BeAttached();
                }
                if (from.x == m_viewComponent.m_X && from.y == m_viewComponent.m_Z)
                {
                    m_viewComponent.SetPiecePos(to.x, to.y);
                }
                break;
            case NotificationConstant.OnGameOver:
                m_viewComponent.OnGameOver();
                break;
            case NotificationConstant.RoundBegin:
                if(m_viewComponent.selfColor == (Config.PieceColor)body)
                {
                    m_viewComponent.OnRoundBegin();
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

    public void NotityDragEnd(object body)
    {
        SendNotification(NotificationConstant.PlayerActive, body);
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

    public void NotifyEndTurn(int type)
    {
        SendNotification(NotificationConstant.EndTurn, type);
    }

    public void NotifyPVEEndTurn()
    {
        SendNotification(NotificationConstant.PVEEndTurn);
    }

    public void NotifyMoveEnd(object body)
    {
        SendNotification(NotificationConstant.OnMoveEnd, body);
    }

    public void NotifyAttackOther()
    {
        SendNotification(NotificationConstant.OnAttackOther);
    }

    private void IsCheck(int color)
    {
        if (App.ChessLogic.IsCheck(color))
        {
            SendNotification(NotificationConstant.OnCheck, color);
        }
    }
}

