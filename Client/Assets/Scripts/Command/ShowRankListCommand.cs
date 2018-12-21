using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.zyd.common.proto.client;
using Framework;
using PureMVC.Interfaces;
using PureMVC.Patterns.Command;
using UnityEngine;

public class ShowRankListCommand : SimpleCommand
{
    public const string NAME = "ShowRankListCommand";
    private UserDataProxy m_userProxy;
    private PVPProxy m_pvpProxy;
    public override void Execute(INotification notification)
    {
        Request();
        m_pvpProxy = Facade.RetrieveProxy("PVPProxy") as PVPProxy;
        m_userProxy = Facade.RetrieveProxy("UserDataProxy") as UserDataProxy;
    }

    private void Request()
    {
        PlayerRankListRequest.Builder request = PlayerRankListRequest.CreateBuilder();
        var bytes = request.Build().ToByteArray();
        App.NetworkManager.Request("PlayerRankList", bytes, Response);
    }

    private void Response(int error, List<byte[]> btData)
    {
        var data = PlayerRankListResponse.ParseFrom(btData[0]);
        m_userProxy.SetPlayerIndex(data.UserRank);
        m_userProxy.SetPlayerRank(data.Rank);
        m_pvpProxy.SetRankListData(data);
        SendNotification(NotificationConstant.RankListResponse);
    }
}
