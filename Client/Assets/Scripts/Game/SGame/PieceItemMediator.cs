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

    public void InitPieceData(Piece pieceData)
    {
        this.pieceData = pieceData;
    }
}

