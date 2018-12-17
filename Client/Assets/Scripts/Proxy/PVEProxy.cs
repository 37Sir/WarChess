using PureMVC.Patterns.Proxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class PVEProxy : Proxy
{
    public new static string NAME = "PVEProxy";
    private PVPPackage m_pvpData;

    public PVEProxy() : base(NAME)
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
}

