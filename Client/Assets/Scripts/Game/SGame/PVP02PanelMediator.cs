using Framework;
using PureMVC.Interfaces;
using PureMVC.Patterns.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class PVP02PanelMediator : Mediator
{
    public new const string NAME = "PVP02PanelMediator";
    private PVP02Panel m_viewComponent;
    public List<Piece> selfPieces = new List<Piece>();
    public List<Piece> enemyPieces = new List<Piece>();

    public int Energy = 0;//能量
    public int MaxEnergy = 0;//能量上限

    public int E_Energy = 0;//敌人能量
    public int E_MaxEnergy = 0;//敌人能量上限

    public int CurPieceNum = 0;//当前棋子数目

    //temp
    private float m_promoteFromX;
    private float m_promoteFromY;
    private Vector2 m_promoteTo;


    public PVP02PanelMediator(PVP02Panel pvpPanel) : base(NAME)
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
        list.Add(NotificationConstant.NewEndTurnResponse);
        list.Add(NotificationConstant.PlayerActiveResponse);
        list.Add(NotificationConstant.ShowTips);
        list.Add(NotificationConstant.ShowRedTips);
        list.Add(NotificationConstant.SomeoneDead);
        return list;
    }

    public override void HandleNotification(INotification notification)
    {
        object body = notification.Body;
        switch (notification.Name)
        {
            case NotificationConstant.OnDragEnd:
                m_viewComponent.OnTipsHide();
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
            case NotificationConstant.NewEndTurnResponse:
                m_viewComponent.EndCurRound();
                break;
            case NotificationConstant.PlayerActiveResponse:
                m_viewComponent.SetCanNext(false);
                break;
            case NotificationConstant.ShowTips:
                Vector2 clickPoint = (Vector2)body;
                if(m_viewComponent.canNext == true)
                {
                    m_viewComponent.OnTipsShow(clickPoint);
                }
                else
                {
                    m_viewComponent.OnTipsShow(clickPoint, true);
                }
                
                break;
            case NotificationConstant.ShowRedTips:
                Vector2 redPoint = (Vector2)body;
                m_viewComponent.OnTipsShow(redPoint, true);
                break;
            case NotificationConstant.SomeoneDead:
                if(m_viewComponent.isTurn == false)
                {
                    CurPieceNum--;
                }
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
        App.ChessLogic02.Init();     //初始化走棋逻辑
    }

    public void NotifySelfReady(object body)
    {
        SendNotification(NotificationConstant.PlayerReady, body);
    }

    public void NotifyRequestUndo()
    {
        SendNotification(NotificationConstant.PlayerMutually, 1);
    }

    public void NotifyOtherMove(object body)
    {
        SendNotification(NotificationConstant.OnOtherMove, body);
    }

    public void NotifyEndTurn(object body)
    {
        SendNotification(NotificationConstant.EndTurn, body);
    }

    public void NotifyNewEndTurn()
    {
        SendNotification(NotificationConstant.NewEndTurn);
    }

    public void NotifyMoveEnd(object body)
    {
        SendNotification(NotificationConstant.OnMoveEnd, body);
    }

    public void NotifyUndo(object body)
    {
        SendNotification(NotificationConstant.OnUndo, body);
    }

    public void NotifyRoundBegin(object body)
    {
        SendNotification(NotificationConstant.RoundBegin, body);
    }

    public void NotifyPlayerActive(object body)
    {
        SendNotification(NotificationConstant.PlayerActive, body);
    }

    public void NotifyPlayerChat(object body)
    {
        SendNotification(NotificationConstant.PlayerChat, body);
    }

    public void NotifyPieceClick(object body)
    {
        SendNotification(NotificationConstant.PieceClick, body);
    }

    public void NotifyPieceAnimatorStop()
    {
        SendNotification(NotificationConstant.PieceAnimatorStop);
    }

    public void NotifyRoundEnd()
    {
        SendNotification(NotificationConstant.RoundEnd);
    } 
}

