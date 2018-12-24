using com.zyd.common.proto.client;
using PureMVC.Patterns.Proxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class PVPProxy : Proxy
{
    public new static string NAME = "PVPProxy";
    private PVPPackage m_pvpData;


    public PVPProxy() : base(NAME)
    {
        m_pvpData = new PVPPackage();
    }

    public override void OnRegister()
    {

    }

    public void SetFirstId(int id)
    {
        m_pvpData.FirstId = id;
    }

    public int GetFirstId()
    {
        return m_pvpData.FirstId;
    }

    public void SetEnemyId(int id)
    {
        m_pvpData.EnemyId = id;
    }

    public int GetEnemyId()
    {
        return m_pvpData.EnemyId;
    }

    public void SetEnemyName(string name)
    {
        m_pvpData.EnemyName = name;
    }

    public string GetEnemyName()
    {
        return m_pvpData.EnemyName;
    }

    public void SetSelfColor(Config.PieceColor color)
    {
        m_pvpData.SelfColor = color;
    }

    public Config.PieceColor GetSelfColor()
    {
        return m_pvpData.SelfColor;
    }

    public Config.PieceColor GetEnemyColor()
    {
        return 1 - m_pvpData.SelfColor;
    }

    public Config.PieceColor GetColorById(int id)
    {
        if(id == m_pvpData.FirstId)
        {
            return Config.PieceColor.WHITE;
        }
        else
        {
            return Config.PieceColor.BLACK;
        }
    }

    public List<RankList> GetRankListData()
    {
        return m_pvpData.RankListData;
    }

    public void SetRankListData(PlayerRankListResponse data)
    {
        
        List<RankList> temp = new List<RankList>();
        var count = data.RankInfoCount;
        for(int i = 0; i < count; i++)
        {
            var rankData = data.GetRankInfo(i);
            RankList rankInfo = new RankList();
            rankInfo.index = rankData.Ranking;
            rankInfo.userName = rankData.Name;
            rankInfo.rank = rankData.Rank;

            temp.Add(rankInfo);
        }
        m_pvpData.RankListData = temp;
    }

}

