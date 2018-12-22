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

public class PVEPanel
{
    //templet
    private PVEPanelMediator m_mediator;
    private UserDataProxy m_proxy;//todo
    private PVEProxy m_pveProxy;

    //UI object
    private Button m_ready;
    private GameObject m_qizi;//棋子的父物体
    private GameObject m_chess;//棋子
    private GameObject m_enemyReady;
    private Text m_selfTimer;
    private Text m_enemyTimer;
    private Image m_userImage;
    private Image m_enemyImage;
    private Button m_test;
    private Button m_Undo;
    private Camera m_worldCamera;
    private GameObject m_gameStartLogo;
    private TweenPlayer m_cameraTween;

    private Vector2 m_lastFrom;
    private Vector2 m_lastTo;
    private int m_lastEat = -1;

    public bool isTurn = true;
    public bool isPause = false;
    public int roundNum = 0;
    public Config.PieceColor selfColor = Config.PieceColor.WHITE;//自己的颜色
    public Config.PieceColor AIColor = Config.PieceColor.BLACK;//AI的颜色
    private IEnumerator m_roundTimer;   //计时器
    private List<GameObject> m_tips = new List<GameObject>();
    private ModelDrag m_modelDrag;

    public void InitView(GameObject gameObject)
    {
        //templet
        m_mediator = new PVEPanelMediator(this);
        App.Facade.RegisterMediator(m_mediator);
        m_proxy = App.Facade.RetrieveProxy("UserDataProxy") as UserDataProxy;
        m_pveProxy = App.Facade.RetrieveProxy("PVEProxy") as PVEProxy;
        InitUIBinder(gameObject);
        m_chess.SetActive(false);
        m_ready.onClick.AddListener(OnReadyClick);
        m_test.onClick.AddListener(OnUndoClick);
        m_Undo.onClick.AddListener(OnRoundStart);
    }

    private void InitUIBinder(GameObject gameObject)
    {
        m_qizi = GameObject.Find("qizi");
        m_chess = m_qizi.gameObject.transform.Find("m_Chess").gameObject;
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
        m_gameStartLogo = gameObject.transform.Find("Container/m_GameBegin").gameObject;
    }

    public void OpenView(object intent)
    {
        int userId = m_proxy.GetPlayerId();
        isTurn = true;
        m_modelDrag.isTurn = true;
        m_pveProxy.SetSelfColor(Config.PieceColor.WHITE);
        
        m_userImage.GetComponentInChildren<Text>().text = m_proxy.GetPlayerName();
        m_enemyImage.GetComponentInChildren<Text>().text = m_pveProxy.GetEnemyName();
        InitTimer();
        App.SoundManager.PlaySoundClip(Config.Sound.InGameStart);
    }

    /// <summary>
    /// 初始化棋盘
    /// </summary>
    private void InitChessBoard()
    {
        for (int y = 0; y < Config.Board.MaxY; y++)
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
                    GameObject temp = GameObject.Instantiate(m_chess);
                    temp.name = x + 1 + "_" + (y + 1);
                    temp.transform.parent = m_qizi.transform;
                    temp.SetActive(true);
                    PieceItem pieceItem = temp.AddComponent<PieceItem>();
                    pieceItem.InitView(temp, new Piece((Config.PieceColor)color, (Config.PieceType)type, x + 1, y + 1), true);

                    App.ObjectPoolManager.RegisteObject(pieceName, "FX/" + pieceName, 0, 30, -1);
                    App.ObjectPoolManager.Instantiate(pieceName, (GameObject obj) => {
                        if(color == (int)Config.PieceColor.BLACK)
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
        var moves = App.ChessLogic.GenerateMoves(new Vector2(from.x - 1, from.y - 1));
        foreach (Vector2 to in moves)
        {
            App.ObjectPoolManager.RegisteObject("biankuang_green", "FX/biankuang_green", 0, 30, -1);
            App.ObjectPoolManager.Instantiate("biankuang_green", (GameObject obj) =>
            {
                obj.SetActive(true);
                obj.transform.parent = m_qizi.transform;
                obj.transform.localPosition = new Vector3(to.x * Config.PieceWidth, 3, to.y * Config.PieceWidth);
                obj.transform.localScale = new Vector3(15, 15, 1);
                m_tips.Add(obj);
            });
        }
    }

    public void OnTipsHide()
    {
        foreach (GameObject obj in m_tips)
        {
            App.ObjectPoolManager.Release("biankuang_green", obj);
        }
        m_tips.Clear();
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
        m_enemyReady.gameObject.SetActive(false);
        m_mediator.InitBoardData();//初始化棋盘数据
        InitChessBoard();          //初始化棋盘表现
        roundNum = 1;
        if (isTurn)
        {
            OnRoundStart();
        }
    }

    public void OnGameOver(Config.PieceColor loseColor)
    {
        Debug.Log("GameOver!!");
        StopRoundTimer();
        if (loseColor == selfColor)
        {
            App.UIManager.OpenPanel("ResultPanel", new object[] {Config.GameResult.LOSE, "-100"});
        }
        else
        {
            App.UIManager.OpenPanel("ResultPanel", new object[] { Config.GameResult.WIN, "+100" });
        }      
    }

    /// <summary>
    /// 回合开始
    /// </summary>
    private void OnRoundStart()
    {
        //自己的回合
        if (isTurn == true)
        {
            StopRoundTimer();
            StartRoundTimer();
        }
        //AI的回合
        else
        {
            Move move = App.ChessAI.GetSimpleNextMove((int)AIColor);
            Debug.Log("move from" + move.From+" to"+ move.To);
            var item = GameObject.Find((move.From.x + 1) + "_" + (move.From.y + 1));
            item.GetComponent<PieceItem>().AIMove(move);
        }

    }

    /// <summary>
    /// 回合结束 换人
    /// </summary>
    private void OnRoundEnd()
    {
        if(isTurn == true)
        {
            m_selfTimer.gameObject.SetActive(false);
            m_enemyTimer.gameObject.SetActive(true);
            isTurn = false;
        }
        else
        {
            m_selfTimer.gameObject.SetActive(true);
            m_enemyTimer.gameObject.SetActive(false);
            isTurn = true;
        }      
        roundNum++;
        Debug.Log("=======RoundEnd======== num: " + roundNum);
    }


    /// <summary>
    /// 结束当前回合
    /// </summary>
    public void EndCurRound()
    {
        OnRoundEnd();
        OnRoundStart();
    }

    /// <summary>
    /// 记录上一步走了什么
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <param name="type"></param>
    /// <param name="eatType"></param>
    public void ShowMove(Vector2 from, Vector2 to, int type, int eatType)
    {
        m_lastFrom = from;
        m_lastTo = to;
        m_lastEat = eatType;
    }


    ///兵晋升
    public void OnPPromote(object body)
    {
        isPause = true;
        App.UIManager.OpenPanel("TypeSelectPanel", body);
    }

    private void OnReadyClick()
    {
        App.SoundManager.PlaySoundClip(Config.Sound.Click1);
        m_ready.gameObject.SetActive(false);
        m_cameraTween.SetTweenPackSync("camera_start_white");      
        var clip = m_cameraTween.GetClipTween("move_start");
        clip.SetOnComplete(OnCameratweenComplete, null);      
    }

    private void OnCameratweenComplete(object[] args)
    {
        m_gameStartLogo.SetActive(true);
        OnGameStart();
    }

    private void OnUndoClick()
    {
        Debug.Log("OnUndoClick");
        App.ChessLogic.Undo(m_lastFrom, m_lastTo, m_lastEat);
        if (m_lastEat > -1)
        {
            int color = m_lastEat / 10;
            int type = m_lastEat % 10;
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
            GameObject temp = GameObject.Instantiate(m_chess);
            temp.name = m_lastTo.x + "_" + m_lastTo.y;
            temp.transform.parent = m_qizi.transform;
            temp.SetActive(true);
            PieceItem pieceItem = temp.AddComponent<PieceItem>();
            pieceItem.InitView(temp, new Piece((Config.PieceColor)color, (Config.PieceType)type, (int)m_lastTo.x, (int)m_lastTo.y), true);
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
        Vector2[] body = new Vector2[] { m_lastFrom, m_lastTo, new Vector2(m_lastEat, 0) };
        m_mediator.NotifyUndo(body);
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
        if (isTurn == true)
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
            while (isPause)
            {
                yield return new WaitForSeconds(1);
            }
            if (timer != null)
            {
                timer.text = (Config.Game.WaitingRound - i) + "s";
            }          
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

    private GameObject GetPieceItem(int x, int y)
    {
        var item = m_qizi.transform.Find(x + "_" + y).gameObject;
        return item;
    }
}

