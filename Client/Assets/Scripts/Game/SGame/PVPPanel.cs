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

    //UI object
    private Button m_ready;
    private GameObject m_piece;//棋子
    private GameObject m_qizi;//棋子的父物体
    private GameObject m_enemyReady;
    private Text m_selfTimer;
    private Text m_enemyTimer;
    private Button m_test;

    public bool isTurn = true;
    public Config.PieceColor selfColor = Config.PieceColor.WHITE;//自己的颜色
    private IEnumerator m_roundTimer;   //计时器
    private List<GameObject> m_tips = new List<GameObject>();
    private ModelDrag m_modelDrag;

    public void InitView(GameObject gameObject)
    {
        //templet
        m_mediator = new PVPPanelMediator(this);
        m_proxy = new UserDataProxy();
        App.Facade.RegisterMediator(m_mediator);
        App.Facade.RegisterProxy(m_proxy);
        InitUIBinder(gameObject);

        m_piece.SetActive(false);
        m_ready.onClick.AddListener(OnReadyClick);
        m_test.onClick.AddListener(OnRoundStart);
        App.NetworkManager.RegisterPushCall(Config.PushMessage.OtherMove, ShowOtherMove);      
    }

    private void InitUIBinder(GameObject gameObject)
    {
        m_piece = GameObject.Find("m_Chess");
        m_qizi = GameObject.Find("qizi");
        m_selfTimer = gameObject.transform.Find("Container/m_SelfTimer").GetComponent<Text>();
        m_enemyTimer = gameObject.transform.Find("Container/m_EnemyTimer").GetComponent<Text>();
        m_modelDrag = GameObject.Find("board").GetComponent<ModelDrag>();
        m_ready = gameObject.transform.Find("Container/m_Ready").gameObject.GetComponent<Button>();
        m_enemyReady = gameObject.transform.Find("Container/m_EnemyReady").gameObject;
        m_test = gameObject.transform.Find("Container/test").GetComponent<Button>();
    }

    public void OpenView()
    {

    }

    /// <summary>
    /// 初始化棋盘
    /// </summary>
    private void InitChessBoard()
    {
        for(int y = 0; y < Config.Board.MaxY; y++)
        {
            for(int x = 0; x < Config.Board.MaxX; x++)
            {
                int piece = App.ChessLogic.GetPiece(x, y);
                if(piece >= 0)
                {
                    int color = piece / 10;
                    int type = piece % 10;
                    GameObject temp = GameObject.Instantiate(m_piece);//todo
                    temp.transform.parent = m_qizi.transform;
                    temp.SetActive(true);
                    PieceItem pieceItem = temp.AddComponent<PieceItem>();
                    pieceItem.InitView(temp, new Piece((Config.PieceColor)color, (Config.PieceType)type, x+1, y+1));
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
                obj.transform.localPosition = new Vector3(to.x * Config.PieceWidth, 2f, to.y * Config.PieceWidth);
                obj.transform.localScale = new Vector3(40, 40, 1);
                m_tips.Add(obj);
            });
        }
    }

    private void ShowOtherMove(string name, List<byte[]> packet)
    {

    }

    public void OnTipsHide()
    {
        foreach(GameObject obj in m_tips)
        {
            App.ObjectPoolManager.Release("m_TipGreen", obj);
        }
        m_tips.Clear();
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

