using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Framework;
using PureMVC.Interfaces;
using PureMVC.Patterns.Command;
using UnityEngine;

public class LobbyConnectCommand: SimpleCommand 
{
    public const string NAME = "LobbyConnectCommand";

    public override void Execute(INotification notification)
    {
        Debug.Log("LobbyConnectCommand Execute!");
        App.NetworkManager.Init(true);
    }
}
