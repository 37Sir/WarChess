using com.zyd.common.proto.client;
using Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class PVPPanel
{
    //templet
    private PVPPanelMediator m_mediator;
    private UserDataProxy m_proxy;//todo
    private PVPProxy m_pvpProxy;

    //UI object
    private Button m_ready;
    private GameObject m_piece;//棋子
    private GameObject m_qizi;//棋子的父物体
    private GameObject m_enemyReady;
    private Text m_selfTimer;
    private Text m_enemyTimer;
    private Image m_userImage;
    private Image m_enemyImage;
    private Button m_test;
    private Camera m_worldCamera;
    private TweenPlayer m_cameraTween;
    private Button m_Undo;

    public bool isTurn = true;
    public int roundNum = 0;
    private int m_undoCompleteNum = 0;
    private int m_undoRoundNum = 0;
    public Config.PieceColor selfColor = Config.PieceColor.WHITE;//自己的颜色
    private IEnumerator m_roundTimer;   //计时器
    private List<GameObject> m_tips = new List<GameObject>();
    private ModelDrag m_modelDrag;


    public void InitView(GameObject gameObject)
    {
        //templet
        m_mediator = new PVPPanelMediator(this);
        App.Facade.RegisterMediator(m_mediator);
        m_proxy = App.Facade.RetrieveProxy("UserDataProxy") as UserDataProxy;
        m_pvpProxy = App.Facade.RetrieveProxy("PVPProxy") as PVPProxy;
        InitUIBinder(gameObject);

        m_piece.SetActive(false);
        m_ready.onClick.AddListener(OnReadyClick);
        m_test.onClick.AddListener(OnRoundStart);
        m_Undo.onClick.AddListener(OnUndoClick);
        App.Facade.RegisterCommand(NotificationConstant.PlayerReady, () => new PlayerReadyCommand());
        App.Facade.RegisterCommand(NotificationConstant.DoMove, () => new DoMoveCommand());
        App.Facade.RegisterCommand(NotificationConstant.EndTurn, () => new EndTurnCommand());
        App.Facade.RegisterCommand(NotificationConstant.PlayerMutually, () => new PlayerMutuallyCommand());
        App.Facade.RegisterCommand(NotificationConstant.PlayerMutuallyFeedback, () => new PlayerMutuallyFeedbackCommand());

        App.NetworkManager.RegisterPushCall(Config.PushMessage.OtherMove, ShowOtherMove);
        App.NetworkManager.RegisterPushCall(Config.PushMessage.OnePlayerReady, OnOnePlayerReady);
        App.NetworkManager.RegisterPushCall(Config.PushMessage.PlayerNotReady, OnPlayerNotReady);
        App.NetworkManager.RegisterPushCall(Config.PushMessage.PlayerReadyFinish, OnReadyFinish);
        App.NetworkManager.RegisterPushCall(Config.PushMessage.PlayNext, OnNextPlay);
        App.NetworkManager.RegisterPushCall(Config.PushMessage.PlayerEnd, OnGameOver);

        App.NetworkManager.RegisterPushCall(Config.PushMessage.PlayerUndoPush, OnPlayerUndoPush);
        App.NetworkManager.RegisterPushCall(Config.PushMessage.PlayerUndoInfoPush, OnPlayerUndoInfoPush);
        App.NetworkManager.RegisterPushCall(Config.PushMessage.PlayerNotAgreePush, OnPlayerNotAgreePush);
        App.NetworkManager.RegisterPushCall(Config.PushMessage.PlayUndoNextPush, OnPlayUndoNextPush);
    }

    private void InitUIBinder(GameObject gameObject)
    {
        m_qizi = GameObject.Find("qizi");
        m_piece = m_qizi.gameObject.transform.Find("m_Chess").gameObject;
        m_worldCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        m_cameraTween = m_worldCamera.GetComponent<TweenPlayer>();
        m_selfTimer = gameObject.transform.Find("Container/m_SelfTimer").GetComponent<Text>();
        m_enemyTimer = gameObject.transform.Find("Container/m_EnemyTimer").GetComponent<Text>();
        m_userImage = gameObject.transform.Find("Container/m_SelfIcon").GetComponent<Image>();
        m_enemyImage = gameObject.transform.Find("Container/m_EnemyIcon").GetComponent<Image>();
        m_modelDrag = GameObject.Find("board").GetComponent<ModelDrag>();
        m_ready = gameObject.transform.Find("Container/m_Ready").gameObject.GetComponent<Button>();
        m_enemyReady = gameObject.transform.Find("Container/m_EnemyReady").gameObject;
        m_test = gameObject.transform.Find("Container/test").GetComponent<Button>();
        m_Undo = gameObject.transform.Find("Container/m_Undo").GetComponent<Button>();
    }

    public void OpenView(object intent)
    {
        int firstId = m_pvpProxy.GetFirstId();
        int userId = m_proxy.GetPlayerId();
        if(firstId == userId)
        {
            isTurn = true;
            
            m_pvpProxy.SetSelfColor(Config.PieceColor.WHITE);
        }
        else
        {
            isTurn = false;
            //m_worldCamera.transform.localRotation = Quaternion.Euler(90, 180, 0);
            m_pvpProxy.SetSelfColor(Config.PieceColor.BLACK);
        }
        m_userImage.GetComponentInChildren<Text>().text = m_proxy.GetPlayerName();
        m_enemyImage.GetComponentInChildren<Text>().text = m_pvpProxy.GetEnemyName();
        InitTimer();
        App.SoundManager.PlaySoundClip(Config.Sound.InGameStart);
    }

    /// <summary>
    /// 初始化棋盘
    /// </summary>
    private void InitChessBoard()
    {
        for(int y = 0; y < Config.Board.MaxY; y++)
        {
            for (int x = 0; x < Config.Board.MaxX; x++)
            {
                int piece = App.ChessLogic.GetPiece(x, y);
                if (piece >= 0)
                {
                    int color = piece / 10;
                    int type = piece % 10;
                    string pieceName = "";
                    if (color == (int)Config.PieceColor.BLACK)
                    {
                        pieceName = "Black_";
                    }
                    else
                    {
                        pieceName = "White_";
                    }
                    switch ((Config.PieceType)type)
                    {
                        case Config.PieceType.P:
                            pieceName = pieceName + "P";
                            break;
                        case Config.PieceType.N:
                            pieceName = pieceName + "N";
                            break;
                        case Config.PieceType.B:
                            pieceName = pieceName + "B";
                            break;
                        case Config.PieceType.R:
                            pieceName = pieceName + "R";
                            break;
                        case Config.PieceType.Q:
                            pieceName = pieceName + "Q";
                            break;
                        case Config.PieceType.K:
                            pieceName = pieceName + "K";
                            break;
                    }
                    GameObject temp = GameObject.Instantiate(m_piece);
                    temp.transform.parent = m_qizi.transform;
                    temp.SetActive(true);
                    PieceItem pieceItem = temp.AddComponent<PieceItem>();
                    pieceItem.InitView(temp, new Piece((Config.PieceColor)color, (Config.PieceType)type, x + 1, y + 1), false);

                    App.ObjectPoolManager.RegisteObject(pieceName, "FX/" + pieceName, 0, 30, -1);
                    App.ObjectPoolManager.Instantiate(pieceName, (GameObject obj) =>
                    {
                        if (color == (int)Config.PieceColor.BLACK)
                        {
                            obj.transform.localRotation = Quaternion.Euler(0, 180, 0);
                        }
                        obj.SetActive(true);
                        obj.transform.parent = temp.transform;
                        obj.transform.localPosition = Vector3.zero;
                        obj.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
                        pieceItem.pieceModel = obj;
                    });
                }
            }
        }
    }

    /// <summary>
    /// 提示
    /// </summary>
    /// <param name="from"></param>
    public void OnTipsShow(Vector2 from)
    {
        var moves =App.ChessLogic.GenerateMoves(new Vector2(from.x - 1, from.y - 1));
        foreach(Vector2 to in moves)
        {
            App.ObjectPoolManager.RegisteObject("m_TipGreen", "FX/m_TipGreen", 0, 30, -1);
            App.ObjectPoolManager.Instantiate("m_TipGreen", (GameObject obj) =>
            {
                obj.SetActive(true);
                obj.transform.parent = m_qizi.transform;
                obj.transform.localPosition = new Vector3(to.x * Config.PieceWidth, 0.5f, to.y * Config.PieceWidth);
                obj.transform.localScale = new Vector3(15, 15, 1);
                m_tips.Add(obj);
            });
        }
    }

    private void ShowOtherMove(string name, List<byte[]> packet)
    {
        var pushMes = ServerBattleMesPush.ParseFrom(packet[0]);
        int fromIndex = pushMes.BattleMes.From;
        int toIndex = pushMes.BattleMes.To;
        int type = pushMes.BattleMes.Promption;
        var from = IndexToCoor(fromIndex);
        var to = IndexToCoor(toIndex);
        Debug.Log("Push====: ShowOtherMove; from:" + from + " to:" + to + " type:" + type);
        m_mediator.NotifyOtherMove(new Vector2[] {from, to, new Vector2(type, 0)});//todo response
        roundNum++;
    }

    public void OnTipsHide()
    {
        foreach(GameObject obj in m_tips)
        {
            App.ObjectPoolManager.Release("m_TipGreen", obj);
        }
        m_tips.Clear();
    }

    public void OnUndoTweenEnd()
    {
        m_undoCompleteNum++;
        Debug.Log("OnUndoTweenEnd=:" + m_undoCompleteNum + "  OnUndoCount=:" + m_undoRoundNum);
        if(m_undoCompleteNum >= m_undoRoundNum)
        {
            m_mediator.NotifyEndTurn();
        }
    }

    private void InitTimer()
    {
        m_selfTimer.gameObject.SetActive(false);
        m_enemyTimer.gameObject.SetActive(false);
    }

    /// <summary>
    /// 对局开始
    /// </summary>
    private void OnGameStart()
    {
        App.SoundManager.PlayBacksound(Config.Sound.InGameMain);
        m_ready.gameObject.SetActive(false);
        m_enemyReady.gameObject.SetActive(false);
        m_mediator.InitBoardData();//初始化棋盘数据
        InitChessBoard();          //初始化棋盘表现
        roundNum = 1;
        if (isTurn)
        {
            OnRoundStart();
        }               
    }

    public void OnGameOver(string name, List<byte[]> packet)
    {
        var pushMes = PlayerEndPush.ParseFrom(packet[0]);
        var winId = pushMes.WinUserId;
        var result = pushMes.Result;
        Debug.Log("GameOver!!");
        if(result >= 6)
        {
            App.UIManager.OpenPanel("ResultPanel", Config.GameResult.DRAW);
        }
        else
        {
            if (winId == m_proxy.GetPlayerId())
            {
                App.UIManager.OpenPanel("ResultPanel", Config.GameResult.WIN);
            }
            else
            {
                App.UIManager.OpenPanel("ResultPanel", Config.GameResult.LOSE);
            }
        }
        StopRoundTimer();
    }

    /// <summary>
    /// 回合开始
    /// </summary>
    private void OnRoundStart()
    {
        StopRoundTimer();
        isTurn = true;
        m_modelDrag.isTurn = isTurn;
        m_Undo.gameObject.SetActive(true);
        StartRoundTimer();
    }

    /// <summary>
    /// 自己的回合结束 
    /// </summary>
    private void OnRoundEnd()
    {
        isTurn = false;
        m_modelDrag.isTurn = isTurn;
        m_Undo.gameObject.SetActive(false);
        StopRoundTimer();
        StartRoundTimer();
        roundNum++;
        Debug.Log("=======RoundEnd======== num: " + roundNum);
    }

    /// <summary>
    /// 将军
    /// </summary>
    public void OnCheck(int color)
    {
        if(color == (int)selfColor)
        {
            Debug.Log("=======被将军======== ");
        }
        else
        {
            Debug.Log("=======将军======== ");
        }
    }

    ///兵晋升
    public void OnPPromote(object body)
    {
        App.UIManager.OpenPanel("TypeSelectPanel", body);
    }

    public void EndCurRound()
    {       
        OnRoundEnd();
    }

    public void OnReadyResponse()
    {
        m_ready.gameObject.SetActive(false);
    }

    private void OnReadyClick()
    {
        App.SoundManager.PlaySoundClip(Config.Sound.Click1);
        if(isTurn == true)
        {
            m_cameraTween.SetTweenPack("camera_start_white");
        }
        else
        {
            m_cameraTween.SetTweenPack("camera_start_black");
        }      
        m_mediator.NotifySelfReady();
    }

    private void OnUndoClick()
    {
        Debug.Log("OnUndoClick");
        App.SoundManager.PlaySoundClip(Config.Sound.Click1);
        m_mediator.NotifyRequestUndo();
    }

    #region Private Method
    private void StartRoundTimer()
    {
        if (m_roundTimer == null)
        {
            m_roundTimer = _OnTimer();
            App.UIManager.StartCoroutine(m_roundTimer);
        }
    }

    private IEnumerator _OnTimer()
    {
        Text timer;
        if(isTurn == true)
        {
            m_selfTimer.gameObject.SetActive(true);
            m_enemyTimer.gameObject.SetActive(false);
            timer = m_selfTimer;
        }
        else
        {
            m_selfTimer.gameObject.SetActive(false);
            m_enemyTimer.gameObject.SetActive(true);
            timer = m_enemyTimer;
        }
        for (int i = 0; i < Config.Game.WaitingRound; i++)
        {
            timer.text = (Config.Game.WaitingRound - i) + "s";
            yield return new WaitForSeconds(1);
        }
        if (isTurn)
        {
            EndCurRound();
        }
        else
        {
            OnRoundStart();            
        }
    }

    private void StopRoundTimer()
    {
        if (m_roundTimer != null)
        {
            App.UIManager.StopCoroutine(m_roundTimer);
            m_roundTimer = null;
        }
    }
    #endregion

    public void OnPlayerNotReady(string name, List<byte[]> packet)
    {
        App.Facade.RemoveMediator("PVPPanelMediator");
        App.NSceneManager.LoadScene("SLobby");
    }

    public void OnPlayerUndoPush(string name, List<byte[]> packet)
    {
        App.UIManager.OpenPanel("MutuallySelectPanel");
    }

    public void OnPlayerUndoInfoPush(string name, List<byte[]> packet)
    {     
        var pushMes = PlayerUndoInfoPush.ParseFrom(packet[0]);
        Debug.Log("同意悔棋"+ pushMes);
        m_undoRoundNum = pushMes.UndoInfoCount;
        for(int i = 0; i < m_undoRoundNum; i++)
        {
            var undoInfo = pushMes.GetUndoInfo(i);
            var battleMes = undoInfo.BattleMes;
            var isEat = undoInfo.IsEat;
            var eatInfo = undoInfo.Type;
            var userId = undoInfo.UserId;
            var from = battleMes.From;
            var to = battleMes.To;

            var lastEat = -1;
            var fromPos = IndexToCoor(from);
            var toPos = IndexToCoor(to);
            var userColor = m_pvpProxy.GetColorById(userId);
            if (isEat == true)
            {
                var color = 1 - userColor;//1-操作人的颜色 = 被吃棋子的颜色
                lastEat = (int)color * 10 + eatInfo;
                PieceReborn(lastEat, toPos);
            }
            App.ChessLogic.Undo(fromPos, toPos, lastEat);
           
            Vector2[] body = new Vector2[] { fromPos, toPos, new Vector2(lastEat, (int)userColor) };
            m_mediator.NotifyUndo(body);
            roundNum--;
        }
    }

    /// <summary>
    /// 棋子复活
    /// </summary>
    /// <param name="lastEat"></param>
    /// <param name="to"></param>
    private void PieceReborn(int lastEat, Vector2 to)
    {
        int color = lastEat / 10;
        int type = lastEat % 10;
        string pieceName = "";
        if (color == (int)Config.PieceColor.BLACK)
        {
            pieceName = "Black_";
        }
        else
        {
            pieceName = "White_";
        }
        switch ((Config.PieceType)type)
        {
            case Config.PieceType.P:
                pieceName = pieceName + "P";
                break;
            case Config.PieceType.N:
                pieceName = pieceName + "N";
                break;
            case Config.PieceType.B:
                pieceName = pieceName + "B";
                break;
            case Config.PieceType.R:
                pieceName = pieceName + "R";
                break;
            case Config.PieceType.Q:
                pieceName = pieceName + "Q";
                break;
            case Config.PieceType.K:
                pieceName = pieceName + "K";
                break;
        }
        GameObject temp = GameObject.Instantiate(m_piece);
        temp.name = to.x + "_" + to.y;
        temp.transform.parent = m_qizi.transform;
        temp.SetActive(true);
        PieceItem pieceItem = temp.AddComponent<PieceItem>();
        pieceItem.InitView(temp, new Piece((Config.PieceColor)color, (Config.PieceType)type, (int)to.x, (int)to.y), false);
        pieceItem.isReborn = true;
        App.ObjectPoolManager.RegisteObject(pieceName, "FX/" + pieceName, 0, 30, -1);
        App.ObjectPoolManager.Instantiate(pieceName, (GameObject obj) =>
        {
            if (color == (int)Config.PieceColor.BLACK)
            {
                obj.transform.localRotation = Quaternion.Euler(0, 180, 0);
            }
            obj.SetActive(true);
            obj.transform.parent = temp.transform;
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
            pieceItem.pieceModel = obj;
        });
    }

    public void OnPlayerNotAgreePush(string name, List<byte[]> packet)
    {
        Debug.Log("对方不同意悔棋!!");
    }  

    public void OnPlayerAgreePush(string name, List<byte[]> packet)
    {
        Debug.Log("Push:On Player Agree");
    }

    public void OnPlayUndoNextPush(string name, List<byte[]> packet)
    {
        Debug.Log("Push:On PLayerUndoNext");
    }

    public void OnOnePlayerReady(string name, List<byte[]> packet)
    {
        m_enemyReady.SetActive(true);
    }

    public void OnReadyFinish(string name, List<byte[]> packet)
    {
        OnGameStart();
        Debug.Log("Push:On Ready Finish");
    }

    public void OnNextPlay(string name, List<byte[]> packet)
    {
        Debug.Log("Push:OnNextPlayPush");
        if (isTurn == true)
        {
            EndCurRound();
        }
        else
        {
            OnRoundStart();
        }
    }

    public void CloseView()
    {

    }

    public void DestroyView()
    {
        App.Facade.RemoveMediator(m_mediator.MediatorName);
    }

    ///index转坐标 且棋盘翻转
    private Vector2 IndexToCoor(int index)
    {
        int y = (64 - index) / Config.Board.MaxX + 1;
        int dx = (64 - index) % Config.Board.MaxX - 1;
        int x = Config.Board.MaxX - dx - 1;
        return new Vector2(x, y);
    }
}

