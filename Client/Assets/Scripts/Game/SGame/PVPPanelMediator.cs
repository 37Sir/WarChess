using Framework;
using PureMVC.Interfaces;
using PureMVC.Patterns.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class PVPPanelMediator : Mediator
{
    public new const string NAME = "PVPPanelMediator";
    private PVPPanel m_viewComponent;
    public List<Piece> selfPieces = new List<Piece>();
    public List<Piece> enemyPieces = new List<Piece>();

    //temp
    private float m_promoteFromX;
    private float m_promoteFromY;
    private Vector2 m_promoteTo;


    public PVPPanelMediator(PVPPanel pvpPanel) : base(NAME)
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
        list.Add(NotificationConstant.OnUndoTweenEnd);
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
                object[] newBody = { (float)bodys[0], (float)bodys[1], (Config.PieceColor)bodys[2], (Vector2)bodys[3], m_viewComponent.roundNum, -1};        
                NotifyDoMove(newBody);              
                break;
            case NotificationConstant.DoMoveResponse:
                object[] move = (object[])body;
                //m_viewComponent.ShowMove((Vector2)move[0], (Vector2)move[1], (int)move[2]);
                break;
            case NotificationConstant.OnGameOver:
                //m_viewComponent.OnGameOver((Config.PieceColor)body);
                break;
            case NotificationConstant.OnTipsShow:
                m_viewComponent.OnTipsShow((Vector2)body);
                break;
            case NotificationConstant.PlayerReadyResponse:
                m_viewComponent.OnReadyResponse();
                break;
            case NotificationConstant.OnCheck:
                m_viewComponent.OnCheck((int)body);
                break;
            case NotificationConstant.OnPPromote:
                var temp = (object[])body;
                m_promoteFromX = (float)temp[0];
                m_promoteFromY = (float)temp[1];
                m_promoteTo = (Vector2)temp[3];
                m_viewComponent.OnPPromote();
                break;
            case NotificationConstant.OnTypeSelect:
                int type = (int)body;
                NotifyDoMove(new object[]{ m_promoteFromX, m_promoteFromY,null, m_promoteTo, m_viewComponent.roundNum, type});
                break;

            case NotificationConstant.OnUndoTweenEnd:
                m_viewComponent.OnUndoTweenEnd();
                break;
            default:
                break;
        }
    }

    private void DoMove(Vector2 from, Vector2 to)
    {

    }

    public void InitBoardData()
    {
        App.ChessLogic.Init();     //初始化走棋逻辑
    }

    public void NotifySelfReady()
    {
        SendNotification(NotificationConstant.PlayerReady);
    }

    public void NotifyRequestUndo()
    {
        SendNotification(NotificationConstant.PlayerMutually, 1);
    }

    public void NotifyDoMove(object body)
    {
        SendNotification(NotificationConstant.DoMove, body);
    }

    public void NotifyOtherMove(object body)
    {
        SendNotification(NotificationConstant.OnOtherMove, body);
    }

    public void NotifyEndTurn()
    {
        SendNotification(NotificationConstant.EndTurn);
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

