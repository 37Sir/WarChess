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
        return list;
    }

    public override void HandleNotification(INotification notification)
    {
        object body = notification.Body;
        switch (notification.Name)
        {
            case NotificationConstant.OnDragEnd:
                m_viewComponent.EndCurRound();
                break;
            case NotificationConstant.OnGameOver:
                m_viewComponent.OnGameOver((Config.PieceColor)body);
                break;
            default:
                break;
        }
    }

    public void InitBoardData()
    {
        Piece temp;
        temp = new Piece(Config.PieceColor.WHITE, Config.PieceType.P, 1, 2);
        selfPieces.Add(temp);
        temp = new Piece(Config.PieceColor.WHITE, Config.PieceType.P, 2, 2);
        selfPieces.Add(temp);
        temp = new Piece(Config.PieceColor.WHITE, Config.PieceType.P, 3, 2);
        selfPieces.Add(temp);
        temp = new Piece(Config.PieceColor.WHITE, Config.PieceType.P, 4, 2);
        selfPieces.Add(temp);
        temp = new Piece(Config.PieceColor.WHITE, Config.PieceType.P, 5, 2);
        selfPieces.Add(temp);
        temp = new Piece(Config.PieceColor.WHITE, Config.PieceType.P, 6, 2);
        selfPieces.Add(temp);
        temp = new Piece(Config.PieceColor.WHITE, Config.PieceType.P, 7, 2);
        selfPieces.Add(temp);
        temp = new Piece(Config.PieceColor.WHITE, Config.PieceType.P, 8, 2);
        selfPieces.Add(temp);
        temp = new Piece(Config.PieceColor.WHITE, Config.PieceType.R, 1, 1);
        selfPieces.Add(temp);
        temp = new Piece(Config.PieceColor.WHITE, Config.PieceType.N, 2, 1);
        selfPieces.Add(temp);
        temp = new Piece(Config.PieceColor.WHITE, Config.PieceType.B, 3, 1);
        selfPieces.Add(temp);
        temp = new Piece(Config.PieceColor.WHITE, Config.PieceType.Q, 4, 1);
        selfPieces.Add(temp);
        temp = new Piece(Config.PieceColor.WHITE, Config.PieceType.K, 5, 1);
        selfPieces.Add(temp);
        temp = new Piece(Config.PieceColor.WHITE, Config.PieceType.B, 6, 1);
        selfPieces.Add(temp);
        temp = new Piece(Config.PieceColor.WHITE, Config.PieceType.N, 7, 1);
        selfPieces.Add(temp);
        temp = new Piece(Config.PieceColor.WHITE, Config.PieceType.R, 8, 1);
        selfPieces.Add(temp);

        temp = new Piece(Config.PieceColor.BLACK, Config.PieceType.P, 1, 7);
        enemyPieces.Add(temp);
        temp = new Piece(Config.PieceColor.BLACK, Config.PieceType.P, 2, 7);
        enemyPieces.Add(temp);
        temp = new Piece(Config.PieceColor.BLACK, Config.PieceType.P, 3, 7);
        enemyPieces.Add(temp);
        temp = new Piece(Config.PieceColor.BLACK, Config.PieceType.P, 4, 7);
        enemyPieces.Add(temp);
        temp = new Piece(Config.PieceColor.BLACK, Config.PieceType.P, 5, 7);
        enemyPieces.Add(temp);
        temp = new Piece(Config.PieceColor.BLACK, Config.PieceType.P, 6, 7);
        enemyPieces.Add(temp);
        temp = new Piece(Config.PieceColor.BLACK, Config.PieceType.P, 7, 7);
        enemyPieces.Add(temp);
        temp = new Piece(Config.PieceColor.BLACK, Config.PieceType.P, 8, 7);
        enemyPieces.Add(temp);
        temp = new Piece(Config.PieceColor.BLACK, Config.PieceType.R, 1, 8);
        enemyPieces.Add(temp);
        temp = new Piece(Config.PieceColor.BLACK, Config.PieceType.N, 2, 8);
        enemyPieces.Add(temp);
        temp = new Piece(Config.PieceColor.BLACK, Config.PieceType.B, 3, 8);
        enemyPieces.Add(temp);
        temp = new Piece(Config.PieceColor.BLACK, Config.PieceType.Q, 4, 8);
        enemyPieces.Add(temp);
        temp = new Piece(Config.PieceColor.BLACK, Config.PieceType.K, 5, 8);
        enemyPieces.Add(temp);
        temp = new Piece(Config.PieceColor.BLACK, Config.PieceType.B, 6, 8);
        enemyPieces.Add(temp);
        temp = new Piece(Config.PieceColor.BLACK, Config.PieceType.N, 7, 8);
        enemyPieces.Add(temp);
        temp = new Piece(Config.PieceColor.BLACK, Config.PieceType.R, 8, 8);
        enemyPieces.Add(temp);
    }
}

