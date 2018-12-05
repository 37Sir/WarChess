using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public sealed class UserDataPackage
{
    private int m_userId;
    private int m_gold;
    private string m_userName;

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
}

