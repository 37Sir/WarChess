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
    public static readonly string SoundsRoot = "";
    public static readonly string EffectsRoot = "";
    public static readonly string EffectPacksRoot = "";
    public static readonly string TweenPacksRoot = "";
    public static readonly string EffectConfigPath = "";

    public static float PieceWidth = 5.5f;

    public static float MoveDistanceCross = 1.3f * PieceWidth;
    public static float MoveDistance = 0.7f * PieceWidth;

    public class Game
    {
        public static int WaitingFindEnemy = 45;                    //匹配等待秒数
        public static int WaitingReady = 60;                        //准备等待秒数
        public static int WaitingRound = 30;                        //回合等待秒数
    }

    public enum NetworkType
    {
        TCP = 0,
        UDP = 1,
    }

    public enum PieceColor
    {
        BLACK = 0,
        WHITE = 1,
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

    public class PushMessage
    {
        public const string MatchSuccess = "PlayerStartPush";
        public const string PlayerNotReady = "PlayerNotReady";
        public const string OnePlayerReady = "OnePlayerReady";
        public const string PlayerReadyFinish = "PlayerReadyFinishedPush";
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
