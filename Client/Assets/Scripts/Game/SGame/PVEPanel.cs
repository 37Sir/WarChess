﻿using com.zyd.common.proto.client;
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
    private GameObject m_piece;//棋子
    private GameObject m_qizi;//棋子的父物体
    private GameObject m_enemyReady;
    private Text m_selfTimer;
    private Text m_enemyTimer;
    private Image m_userImage;
    private Image m_enemyImage;
    private Button m_test;
    private Camera m_worldCamera;

    public bool isTurn = true;
    public int roundNum = 0;
    public Config.PieceColor selfColor = Config.PieceColor.WHITE;//自己的颜色
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

        m_piece.SetActive(false);
        m_ready.onClick.AddListener(OnReadyClick);
        m_test.onClick.AddListener(OnRoundStart);   }

    private void InitUIBinder(GameObject gameObject)
    {
        m_piece = GameObject.Find("m_Chess");
        m_qizi = GameObject.Find("qizi");
        m_worldCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        m_selfTimer = gameObject.transform.Find("Container/m_SelfTimer").GetComponent<Text>();
        m_enemyTimer = gameObject.transform.Find("Container/m_EnemyTimer").GetComponent<Text>();
        m_userImage = gameObject.transform.Find("Container/m_SelfIcon").GetComponent<Image>();
        m_enemyImage = gameObject.transform.Find("Container/m_EnemyIcon").GetComponent<Image>();
        m_modelDrag = GameObject.Find("board").GetComponent<ModelDrag>();
        m_ready = gameObject.transform.Find("Container/m_Ready").gameObject.GetComponent<Button>();
        m_enemyReady = gameObject.transform.Find("Container/m_EnemyReady").gameObject;
        m_test = gameObject.transform.Find("Container/test").GetComponent<Button>();
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
                    GameObject temp = GameObject.Instantiate(m_piece);//todo
                    temp.transform.parent = m_qizi.transform;
                    temp.SetActive(true);
                    PieceItem pieceItem = temp.AddComponent<PieceItem>();
                    pieceItem.InitView(temp, new Piece((Config.PieceColor)color, (Config.PieceType)type, x + 1, y + 1), true);
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
            App.ObjectPoolManager.RegisteObject("m_TipGreen", "FX/m_TipGreen", 0, 30, -1);
            App.ObjectPoolManager.Instantiate("m_TipGreen", (GameObject obj) =>
            {
                obj.SetActive(true);
                obj.transform.parent = m_qizi.transform;
                obj.transform.localPosition = new Vector3(to.x * Config.PieceWidth, 2f, to.y * Config.PieceWidth);
                obj.transform.localScale = new Vector3(40, 40, 1);
                m_tips.Add(obj);
            });
        }
    }

    public void OnTipsHide()
    {
        foreach (GameObject obj in m_tips)
        {
            App.ObjectPoolManager.Release("m_TipGreen", obj);
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

    public void OnGameOver(Config.PieceColor loseColor)
    {
        Debug.Log("GameOver!!");
        StopRoundTimer();
        if (loseColor == selfColor)
        {
            App.UIManager.OpenPanel("ResultPanel", Config.GameResult.LOSE);
        }
        else
        {
            App.UIManager.OpenPanel("ResultPanel", Config.GameResult.WIN);
        }      
    }

    /// <summary>
    /// 回合开始
    /// </summary>
    private void OnRoundStart()
    {
        StopRoundTimer();
        isTurn = true;
        StartRoundTimer();
    }

    /// <summary>
    /// 回合结束 换人
    /// </summary>
    private void OnRoundEnd()
    {
        isTurn = false;
        roundNum++;
        Debug.Log("=======RoundEnd======== num: " + roundNum);
    }

    public void EndCurRound()
    {
        OnRoundEnd();
        OnNextPlay();
    }

    public void ShowMove(Vector2 from, Vector2 to, int type)
    {
        m_mediator.NotifyMoveEnd(new Vector2[] { from, to, new Vector2(type, 0)});
        bool isCheck = App.ChessLogic.IsCheck(0);
        if(isCheck == true)
        {
            Debug.Log("被将军了！");
        }
        EndCurRound();
    }


    ///兵晋升
    public void OnPPromote()
    {
        App.UIManager.OpenPanel("TypeSelectPanel");
    }

    private void OnReadyClick()
    {
        OnGameStart();
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

    public void OnNextPlay()
    {
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

