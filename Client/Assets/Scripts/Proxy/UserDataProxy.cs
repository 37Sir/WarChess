using PureMVC.Patterns.Proxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class UserDataProxy:Proxy
{
    public new static string NAME = "UserDataProxy";
    private UserDataPackage m_userData;

    public UserDataProxy() : base(NAME)
    {
        m_userData = new UserDataPackage();
    }

    public override void OnRegister()
    {
        Debug.Log("User Data Proxy Registed");
    }

    public void SetPlayerInfo(string userName, int userId, int rank, int winning, int losing, int winCount, int loseCount, int draw)
    {
        m_userData.UserName = userName;
        m_userData.UserId = userId;
        m_userData.Rank = rank;
        m_userData.Winning = winning;
        m_userData.Losing = losing;
        m_userData.WinCount = winCount;
        m_userData.LoseCount = loseCount;
        m_userData.Draw = draw;

        m_userData.Gold = 9999;
    }

    public string GetPlayerName()
    {
        return m_userData.UserName;
    }

    public int GetPlayerId()
    {
        return m_userData.UserId;
    }

    public int GetPlayerRank()
    {
        return m_userData.Rank;
    }

    public int GetPlayerWinning()
    {
        return m_userData.Winning;
    }

    public int GetPlayerLosing()
    {
        return m_userData.Losing;
    }

    public int GetPlayerWinCount()
    {
        return m_userData.WinCount;
    }

    public int GetPlayerLoseCount()
    {
        return m_userData.LoseCount;
    }

    public int GetPlayerDraw()
    {
        return m_userData.Draw;
    }
}

