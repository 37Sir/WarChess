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
    public const string PlayerReady = "PlayerReady";
    public const string PlayerMutually = "PlayerMutually";
    public const string PlayerMutuallyFeedback = "PlayerMutuallyFeedback";

    /// notify respons
    public const string LoginResponse = "LoginResponse";
    public const string MatchResponse = "MatchResponse";
    public const string DoMoveResponse = "DoMoveResponse";
    public const string EndTurnResponse = "EndTurnResponse";
    public const string PlayerReadyResponse = "PlayerReadyResponse";

    /// notify
    public const string OnDragEnd = "OnDragEnd";
    public const string OnMoveEnd = "OnMoveEnd";
    public const string OnGameOver = "OnGameOver";
    public const string OnTipsShow = "OnTipsShow";
    public const string OnOtherMove = "OnOtherMove";
    public const string OnCheck = "OnCheck";
    public const string OnTypeSelect = "OnTypeSelect";
    public const string OnPPromote = "OnPPromote";
    public const string OnUndo = "OnUndo";
    public const string OnUndoTweenEnd = "OnUndoTweenEnd";
}
