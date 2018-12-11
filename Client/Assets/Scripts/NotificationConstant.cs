using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class NotificationConstant
{
    public const string LevelUp = "LevelUp";

    /// notify request
    public const string LobbyConnect = "LobbyConnect";
    public const string Login = "Login";
    public const string Match = "Match";
    public const string DoMove = "DoMove";
    public const string EndTurn = "EndTurn";

    /// notify respons
    public const string LoginResponse = "LoginResponse";
    public const string MatchResponse = "MatchResponse";
    public const string DoMoveResponse = "DoMoveResponse";
    public const string EndTurnResponse = "EndTurnResponse";

    /// notify
    public const string OnDragEnd = "OnDragEnd";
    public const string OnGameOver = "OnGameOver";
    public const string OnTipsShow = "OnTipsShow";
}
