using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public sealed class UserDataPackage
{
    private int m_userId;
    private int m_gold;
    private string m_userName;
    private int m_rank;
    private int m_winning;
    private int m_losing;
    private int m_winCount;
    private int m_loseCount;
    private int m_draw;
    private int m_index;

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

    public int Rank
    {
        get
        {
            return m_rank;
        }
        set
        {
            m_rank = value;
        }
    }

    public int Winning
    {
        get
        {
            return m_winning;
        }
        set
        {
            m_winning = value;
        }
    }

    public int Losing
    {
        get
        {
            return m_losing;
        }
        set
        {
            m_losing = value;
        }
    }

    public int WinCount
    {
        get
        {
            return m_winCount;
        }
        set
        {
            m_winCount = value;
        }
    }

    public int LoseCount
    {
        get
        {
            return m_loseCount;
        }
        set
        {
            m_loseCount = value;
        }
    }

    public int Draw
    {
        get
        {
            return m_draw;
        }
        set
        {
            m_draw = value;
        }
    }

    public int Index
    {
        get
        {
            return m_index;
        }
        set
        {
            m_index = value;
        }
    }
}

