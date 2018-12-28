using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Config{
    //网络层配置
    public static string ServerHost = "192.168.90.12";
    public static int ServerHostPort = 10000;

    public static readonly int RpcTimeout = 10000;                  //rpc请求的超时时间ms
    public static readonly string RpcNamespace = "Com.Violet.Rpc";
    public static readonly string UIBaseDir = "UIRes";
    public static readonly string SoundsRoot = "ArtRes/Sounds";
    public static readonly string EffectsRoot = "ArtRes/Effects";
    public static readonly string EffectPacksRoot = "Resources/ArtRes/EffectPacks";
    public static readonly string TweenPacksRoot = "Resources/ArtRes/TweenPacks";
    public static readonly string TweenPacksLoadRoot = "ArtRes/TweenPacks";
    public static readonly string EffectPacksLoadRoot = "ArtRes/EffectPacks";
    public static readonly string EffectConfigPath = "Assets/Resources/ArtRes/Effects/EffectConfig.asset";

    public static float PieceWidth = 2f;

    public static float MoveDistanceCross = 1.3f * PieceWidth;//没用
    public static float MoveDistance = 0.7f * PieceWidth;//中层边界
    public static float TipsDistance = 0.3f * PieceWidth;//内层边界

    public static int[] PieceValue = { 10, 30, 30, 50, 90, 900 };//子力
    public static int[] PieceCost = { 1, 3, 5, 6, 10 };//耗费

    public class Game
    {
        public static int WaitingFindEnemy = 45;                    //匹配等待秒数
        public static int WaitingMutuallySelect = 30;               //选择等待秒数
        public static int WaitingReady = 60;                        //准备等待秒数
        public static int WaitingRound = 60;                        //回合等待秒数
        public static int NewWaitingRound = 120;                    //新模式回合等待秒数
    }

    public class GameMode
    {
        public static int PVP = 0;
        public static int PVP02 = 1;
    }

    public class Sound{
        public static string Click1 = "bt01";
        public static string MatchStart = "match_begin";
        public static string GameStart = "game_start";
        public static string GameWin = "game_win";
        public static string InGameStart = "ingame_start";
        public static string InGameMain = "ingame_long";
        public static string DragBegin = "piece_drag_begin";
        public static string DragSuccess = "piece_drag_success";
        public static string DragFail = "piece_drag_fail";
        public static string MagicAttack = "magic_attack";
        public static string BMagicAttack = "magic_battack";
        public static string PhysAttack = "phy_attack";
        public static string SummonCardClick = "summon_card_click";
        public static string SummonSuccess = "summon_success";
        public static string RoundSwitch = "round_switch";
        public static string GameFail = "game_fail";
    }

    public enum GameResult
    {
        WIN = 0,
        LOSE = 1,
        DRAW = 2,
    }

    public enum NetworkType
    {
        TCP = 0,
        UDP = 1,
    }

    public class Board
    {
        public static int MaxX = 8;
        public static int MaxY = 8;
    }

    public enum PieceColor
    {
        WHITE = 0,
        BLACK = 1,     
    }

    public enum PieceType
    {
        P = 0,
        N = 1,
        B = 2,
        R = 3,
        Q = 4,
        K = 5,
    }

    public class AttackPos
    {
        public static Vector3 B_AttackPoint = new Vector3(-0.5f, 1.8f, -0.3f);
        public static Vector3 N_AttackPoint = new Vector3(-0.5f, 1.8f, -0.3f);
        public static Vector3 P_AttackPoint = new Vector3(-0.5f, 1.8f, -0.3f);
        public static Vector3 Q_AttackPoint = new Vector3(-0.5f, 1.8f, -0.3f);
        public static Vector3 R_AttackPoint = new Vector3(-0.5f, 1.8f, -0.3f);
        public static Vector3 K_AttackPoint = new Vector3(-0.5f, 1.8f, -0.3f);
    }

    public class PushMessage
    {
        public const string PlayerNotReady = "PlayerNotReady";
        public const string OnePlayerReady = "OnePlayerReady";

        public const string MatchSuccess = "PlayerStartPush";
        public const string PlayerReadyFinish = "PlayerReadyFinishedPush";
        public const string OtherMove = "ServerBattleMesPush";
        public const string PlayNext = "PlayNextPush";
        public const string PlayerEnd = "PlayerEndPush";

        //悔棋相关
        public const string PlayerUndoPush = "PlayerUndoPush";
        public const string PlayerUndoInfoPush = "PlayerUndoInfoPush";
        public const string PlayerNotAgreePush = "PlayerNotAgreePush";
        public const string PlayUndoNextPush = "PlayUndoNextPush";


        //新模式
        public const string NewServerBattleMesPush = "NewServerBattleMesPush";
        public const string PlayerCanNextPush = "PlayerCanNextPush";
        public const string PlayerCanPaintingPush = "PlayerCanPaintingPush";
        public const string PlayerPaintingOverPush = "PlayerPaintingOverPush";
    }
}

public class ManagerName
{
    public const string Scene = "SceneManager";
    public const string Timer = "TimeManager";
    public const string Sound = "SoundManager";
    public const string Effect = "EffectManager";
    public const string NEffect = "NEffectManager";
    public const string Atlas = "AtlasManager";
    public const string Network = "NetworkManager";
    public const string Resource = "ResourceManager";
    public const string Thread = "ThreadManager";
    public const string ObjectPool = "ObjectPoolManager";
    public const string Update = "UpdateManager";
    public const string UI = "UIManager";
}
