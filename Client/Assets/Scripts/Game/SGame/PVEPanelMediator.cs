using Framework;
using PureMVC.Interfaces;
using PureMVC.Patterns.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class PVEPanelMediator : Mediator
{
    public new const string NAME = "PVEPanelMediator";
    private PVEPanel m_viewComponent;
    public List<Piece> selfPieces = new List<Piece>();
    public List<Piece> enemyPieces = new List<Piece>();

    public PVEPanelMediator(PVEPanel pvpPanel) : base(NAME)
    {
        m_viewComponent = pvpPanel;
    }

    public override void OnRegister()
    {
        base.OnRegister();
    }

    public override IList<string> ListNotificationInterests()
    {
        IList<string> list = new List<string>();
        list.Add(NotificationConstant.OnDragEnd);
        list.Add(NotificationConstant.OnGameOver);
        list.Add(NotificationConstant.OnTipsShow);
        list.Add(NotificationConstant.DoMoveResponse);
        list.Add(NotificationConstant.PlayerReadyResponse);
        list.Add(NotificationConstant.OnPPromote);
        list.Add(NotificationConstant.OnTypeSelect);
        list.Add(NotificationConstant.PVEEndTurn);
        return list;
    }

    public override void HandleNotification(INotification notification)
    {
        object body = notification.Body;
        switch (notification.Name)
        {
            case NotificationConstant.OnDragEnd:
                m_viewComponent.OnTipsHide();
                if (body == null) return;
                object[] bodys = (object[])body;
                object[] newBody = { (float)bodys[0], (float)bodys[1], (Config.PieceColor)bodys[2], (Vector2)bodys[3], m_viewComponent.roundNum };
                m_viewComponent.ShowMove(new Vector2((float)bodys[0], (float)bodys[1]), (Vector2)bodys[3], -1, (int)bodys[4]);
                break;
            case NotificationConstant.OnGameOver:
                m_viewComponent.OnGameOver((Config.PieceColor)body);
                break;
            case NotificationConstant.OnTipsShow:
                m_viewComponent.OnTipsShow((Vector2)body);
                break;
            case NotificationConstant.OnPPromote:
                m_viewComponent.OnPPromote(body);
                break;
            case NotificationConstant.OnTypeSelect:
                m_viewComponent.isPause = false;
                break;
            case NotificationConstant.PVEEndTurn:
                m_viewComponent.EndCurRound();
                break;
            default:
                break;
        }
    }

    public void InitBoardData()
    {
        App.ChessLogic.Init();     //初始化走棋逻辑
    }

    public void NotifySelfReady()
    {
        SendNotification(NotificationConstant.PlayerReady);
    }

    public void NotifyMoveEnd(object body)
    {
        SendNotification(NotificationConstant.OnMoveEnd, body);
    }

    public void NotifyUndo(object body)
    {
        SendNotification(NotificationConstant.OnUndo, body);
    }
}

