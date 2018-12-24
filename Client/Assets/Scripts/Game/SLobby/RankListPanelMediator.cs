using PureMVC.Interfaces;
using PureMVC.Patterns.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class RankListPanelMediator : Mediator
{
    public new const string NAME = "RankListPanelMediator";
    public PVPProxy pvpProxy;
    private RankListPanel m_viewComponent;

    public RankListPanelMediator(RankListPanel lobbyPanel) : base(NAME)
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
        list.Add(NotificationConstant.RankListResponse);
        return list;
    }

    public override void HandleNotification(INotification notification)
    {
        object body = notification.Body;
        switch (notification.Name)
        {
            case NotificationConstant.RankListResponse:
                m_viewComponent.UpdateRankList();
                m_viewComponent.UpdateRank();
                break;
            default:
                break;
        }
    }

    public void NotifyShowRankList()
    {
        SendNotification(NotificationConstant.ShowRankList);
    }

    public void NotifyUpdateRankList()
    {
        SendNotification(NotificationConstant.UpdateRankList);
    }
}

