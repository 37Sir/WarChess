using Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class PVPPanel
{
    private Button m_ready;
    private GameObject m_piece;
    private GameObject m_qizi;
    private GameObject m_enemyReady;
    private Text m_selfTimer;
    private Text m_enemyTimer;
    private Button m_test;

    public bool isTurn = true;
    public Config.PieceColor selfColor = Config.PieceColor.WHITE;//自己的颜色
    private IEnumerator m_roundTimer;   //计时器

    private PVPPanelMediator m_mediator;
    private UserDataProxy m_proxy;//todo
    private ModelDrag m_modelDrag;

    public void InitView(GameObject gameObject)
    {
        m_mediator = new PVPPanelMediator(this);
        m_proxy = new UserDataProxy();
        App.Facade.RegisterMediator(m_mediator);
        App.Facade.RegisterProxy(m_proxy);
        m_piece = GameObject.Find("m_Chess");
        m_piece.SetActive(false);
        m_qizi = GameObject.Find("qizi");
        m_selfTimer = gameObject.transform.Find("Container/m_SelfTimer").GetComponent<Text>();
        m_enemyTimer = gameObject.transform.Find("Container/m_EnemyTimer").GetComponent<Text>();
        m_modelDrag = GameObject.Find("board").GetComponent<ModelDrag>();
        m_ready = gameObject.transform.Find("Container/m_Ready").gameObject.GetComponent<Button>();
        m_ready.onClick.AddListener(OnReadyClick);
        m_enemyReady = gameObject.transform.Find("Container/m_EnemyReady").gameObject;

        m_test = gameObject.transform.Find("Container/test").GetComponent<Button>();
        m_test.onClick.AddListener(OnRoundStart);
    }

    public void OpenView()
    {
   
    }

    /// <summary>
    /// 初始化棋盘
    /// </summary>
    private void InitChessBoard()
    {
        foreach(Piece piece in m_mediator.selfPieces)
        {
            GameObject temp = GameObject.Instantiate(m_piece);//todo
            temp.transform.parent = m_qizi.transform;
            temp.SetActive(true);
            PieceItem pieceItem = temp.AddComponent<PieceItem>();
            pieceItem.InitView(temp, piece);
        }

        foreach (Piece piece in m_mediator.enemyPieces)
        {
            GameObject temp = GameObject.Instantiate(m_piece);//todo
            temp.transform.parent = m_qizi.transform;
            temp.SetActive(true);
            PieceItem pieceItem = temp.AddComponent<PieceItem>();
            pieceItem.isEnemy = true;
            pieceItem.InitView(temp, piece);
        }
    }

    private void InitTimer()
    {
        m_selfTimer.gameObject.SetActive(true);
        m_enemyTimer.gameObject.SetActive(true);
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
        InitTimer();
        OnRoundStart();         
    }

    public void OnGameOver(Config.PieceColor loseColor)
    {
        Debug.Log("GameOver!!");
        if(loseColor == selfColor)
        {
            Debug.Log("You Lose!!");
        }
        else
        {
            Debug.Log("You Win!!");
        }
        StopRoundTimer();
        App.NSceneManager.LoadScene("SLobby");
    }

    /// <summary>
    /// 回合开始
    /// </summary>
    private void OnRoundStart()
    {
        StopRoundTimer();
        isTurn = true;
        m_modelDrag.isTurn = isTurn;
        StartRoundTimer();
    }

    /// <summary>
    /// 回合结束
    /// </summary>
    private void OnRoundEnd()
    {
        isTurn = false;
        m_modelDrag.isTurn = isTurn;
        StopRoundTimer();
        StartRoundTimer();
    }

    public void EndCurRound()
    {       
        OnRoundEnd();
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

    public void CloseView()
    {

    }

    public void DestroyView()
    {

    }
}

