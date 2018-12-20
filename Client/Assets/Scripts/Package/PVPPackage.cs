using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class RankList
{
    public string userName;
    public int rank;
    public int index;
}

public sealed class PVPPackage
{
    private int m_userId;
    private int m_enemyId;
    private int m_gold;
    private string m_userName;
    private string m_enemyName;

    private int m_firstId;
    private Config.PieceColor m_selfColor;

    private List<RankList> m_rankListData;

    public List<RankList> RankListData
    {
        get
        {
            return m_rankListData;
        }
        set
        {
            m_rankListData = value;
        }
    }

    public int FirstId
    {
        get
        {
            return m_firstId;
        }
        set
        {
            m_firstId = value;
        }
    }

    public Config.PieceColor SelfColor
    {
        get
        {
            return m_selfColor;
        }
        set
        {
            m_selfColor = value;
        }
    }

    public int UserId
    {
        get
        {
            return m_userId;
        }
        set
        {
            m_userId = value;
        }
    }

    public int Gold
    {
        get
        {
            return m_gold;
        }
        set
        {
            m_gold = value;
        }
    }

    public string UserName
    {
        get
        {
            return m_userName;
        }
        set
        {
            m_userName = value;
        }
    }

    public int EnemyId
    {
        get
        {
            return m_enemyId;
        }
        set
        {
            m_enemyId = value;
        }
    }

    public string EnemyName
    {
        get
        {
            return m_enemyName;
        }
        set
        {
            m_enemyName = value;
        }
    }


}

