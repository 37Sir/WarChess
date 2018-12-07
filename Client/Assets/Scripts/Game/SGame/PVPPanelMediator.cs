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
    public Dictionary<string, Piece> pieces;
    
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

        return list;
    }

    public override void HandleNotification(INotification notification)
    {
        switch (notification.Name)
        {
            default:
                break;
        }
    }

    public void InitBoardData()
    {
        Piece temp;
        temp = new Piece(Config.PieceColor.BLACK, Config.PieceType.B, 1, 2);
        pieces.Add("P", temp);
        temp = new Piece(Config.PieceColor.BLACK, Config.PieceType.B, 2, 2);
        pieces.Add("P", temp);
        temp = new Piece(Config.PieceColor.BLACK, Config.PieceType.B, 3, 2);
        pieces.Add("P", temp);
        temp = new Piece(Config.PieceColor.BLACK, Config.PieceType.B, 4, 2);
        pieces.Add("P", temp);
        temp = new Piece(Config.PieceColor.BLACK, Config.PieceType.B, 5, 2);
        pieces.Add("P", temp);
        temp = new Piece(Config.PieceColor.BLACK, Config.PieceType.B, 6, 2);
        pieces.Add("P", temp);
        temp = new Piece(Config.PieceColor.BLACK, Config.PieceType.B, 7, 2);
        pieces.Add("P", temp);
        temp = new Piece(Config.PieceColor.BLACK, Config.PieceType.B, 8, 2);
        pieces.Add("P", temp);
    }
}

