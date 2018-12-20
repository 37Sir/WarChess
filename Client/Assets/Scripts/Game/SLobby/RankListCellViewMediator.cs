using PureMVC.Interfaces;
using PureMVC.Patterns.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class RankListCellViewMediator : Mediator
{
    public new const string NAME = "RankListCellViewMediator";
    public PVPProxy pvpProxy;
    private RankListCellView m_viewComponent;

    public RankListCellViewMediator(RankListCellView lobbyPanel) : base(NAME)
    {
        m_viewComponent = lobbyPanel;
    }

    public override void OnRegister()
    {
        base.OnRegister();
        pvpProxy = Facade.RetrieveProxy("PVPProxy") as PVPProxy;
    }

    public override IList<string> ListNotificationInterests()
    {
        IList<string> list = new List<string>();
        return list;
    }

    public override void HandleNotification(INotification notification)
    {
        object body = notification.Body;
        switch (notification.Name)
        {
            default:
                break;
        }
    }
}

