using PureMVC.Interfaces;
using PureMVC.Patterns.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class RankListViewMediator : Mediator
{
    public new const string NAME = "RankListViewMediator";
    public PVPProxy pvpProxy;
    private RankListView m_viewComponent;

    public RankListViewMediator(RankListView lobbyPanel) : base(NAME)
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
            case NotificationConstant.UpdateRankList:
                //m_viewComponent.SetDataSize(GetDataSize());
                break;
            default:
                break;
        }
    }

    private int GetDataSize()
    {
        var listData = pvpProxy.GetRankListData();
        return listData.Count;
    }
}

