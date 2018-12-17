using PureMVC.Interfaces;
using PureMVC.Patterns.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class ResultPanelMediator : Mediator
{
    public new const string NAME = "ResultPanelMediator";
    private ResultPanel m_viewComponent;

    public ResultPanelMediator(ResultPanel resultPanel) : base(NAME)
    {
        m_viewComponent = resultPanel;
    }

    public override void OnRegister()
    {
        base.OnRegister();
    }

    public override IList<string> ListNotificationInterests()
    {
        IList<string> list = new List<string>();
        return list;
    }

    public override void HandleNotification(INotification notification)
    {
        switch (notification.Name)
        {
            default:
                break;
        }
    }
}

