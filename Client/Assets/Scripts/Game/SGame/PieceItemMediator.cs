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

    public PieceItemMediator(PieceItem pieceItem, string name) : base(name + NAME)
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
        list.Add(NotificationConstant.OnUndo);
        list.Add(NotificationConstant.OnTypeSelect);
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
                IsCheck((int)m_viewComponent.selfColor);
                break;

            case NotificationConstant.OnMoveEnd:
                Vector2[] endMove = (Vector2[])body;
                var from = endMove[0];
                var to = endMove[1];
                var type = (int)endMove[2].x;
                if (to.x == m_viewComponent.m_X && to.y == m_viewComponent.m_Z)
                {
                    m_viewComponent.BeAttached();
                }
                if(from.x == m_viewComponent.m_X && from.y == m_viewComponent.m_Z)
                {
                    m_viewComponent.SetPiecePos(to.x, to.y);
                    if (type > 0)
                    {
                        m_viewComponent.OnPromoted(type);
                    }
                }
                IsCheck(1 - (int)m_viewComponent.selfColor);
                break;

            case NotificationConstant.OnTypeSelect:
                Vector2[] promoteMove = (Vector2[])body;
                var profrom = promoteMove[0];
                var proto = promoteMove[1];
                var protype = (int)promoteMove[2].x;
                if (proto.x == m_viewComponent.m_X && proto.y == m_viewComponent.m_Z)
                {
                    m_viewComponent.BeAttached();
                }
                if (profrom.x == m_viewComponent.m_X && profrom.y == m_viewComponent.m_Z)
                {
                    m_viewComponent.SetPiecePos(proto.x, proto.y);
                    if (protype > 0)
                    {
                        m_viewComponent.OnPromoted(protype);
                    }
                }
                IsCheck(1 - (int)m_viewComponent.selfColor);
                break;

            case NotificationConstant.OnGameOver:
                m_viewComponent.OnGameOver();
                break;

            //悔棋
            case NotificationConstant.OnUndo:
                Vector2[] undoMove = (Vector2[])body;
                var undoFrom = undoMove[0];
                var undoTo = undoMove[1];
                var undoEat = undoMove[2].x;
                var undoColor = undoMove[2].y;
                if (m_viewComponent.m_X == undoTo.x && m_viewComponent.m_Z == undoTo.y )
                {
                    if(m_viewComponent.isReborn == false && undoColor == (int)m_viewComponent.pieceColor )
                    {
                        m_viewComponent.OnUndo(undoFrom);
                    }
                    else
                    {
                        m_viewComponent.isReborn = false;
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

    public void NotityPPromote(object[] body)
    {
        App.Facade.SendNotification(NotificationConstant.OnPPromote, body);
    }

    public void NotityGameOver(object body)
    {
        App.Facade.SendNotification(NotificationConstant.OnGameOver, body);
    }

    public void NotifyDragTips(object body)
    {
        App.Facade.SendNotification(NotificationConstant.OnTipsShow, body);
    }

    public void NotifyEndTurn()
    {
        SendNotification(NotificationConstant.EndTurn);
    }

    public void NotifyMoveEnd(object body)
    {
        SendNotification(NotificationConstant.OnMoveEnd, body);
    }

    public void NotifyUndoTweenEnd()
    {
        SendNotification(NotificationConstant.OnUndoTweenEnd);
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

