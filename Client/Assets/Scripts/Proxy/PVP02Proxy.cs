using com.zyd.common.proto.client;
using PureMVC.Patterns.Proxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class PVP02Proxy : Proxy
{
    public new static string NAME = "PVP02Proxy";
    private PVP02Package m_pvpData;


    public PVP02Proxy() : base(NAME)
    {
        m_pvpData = new PVP02Package();
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

    public void SetTestMode(bool isTest)
    {
        m_pvpData.IsTest = isTest;
    }

    public bool GetTestMode()
    {
        return m_pvpData.IsTest;
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
        if (id == m_pvpData.FirstId)
        {
            return Config.PieceColor.WHITE;
        }
        else
        {
            return Config.PieceColor.BLACK;
        }
    }
}

