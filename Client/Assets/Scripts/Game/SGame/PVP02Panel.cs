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

public class PVP02Panel
{
    //templet
    private PVP02PanelMediator m_mediator;
    private UserDataProxy m_proxy;//todo
    private PVP02Proxy m_pvpProxy;

    //UI object
    private Button m_ready;
    private Button m_EndTurn;
    private Button m_P;
    private Button m_N;
    private Button m_B;
    private Button m_R;
    private Button m_Q;
    private Button m_Item1;
    private Button m_Item2;
    private Button m_Item3;
    private Button m_Item4;
    private Button m_SelectCloce;
    private Toggle m_BottomToggle;

    private Animator m_BlackAnimal;
    private Animator m_WhiteAnimal;
    private GameObject m_piece;//棋子
    private GameObject m_qizi;//棋子的父物体
    private GameObject m_enemyReady;
    private GameObject m_Bottom;
    private Text m_selfTimer;
    private Text m_enemyTimer;
    private GameObject m_selfTimerObj;
    private GameObject m_enemyTimerObj;
    private Image m_userImage;
    private Image m_enemyImage;
    private Camera m_worldCamera;
    private TweenPlayer m_cameraTween;
    private GameObject m_RoundChange;
    private GameObject m_sunLight;
    private Text m_EnergyText;
    private GameObject m_EnergyContainer;
    private GameObject m_EnemyEnergy;
    private Button m_SelfSay;
    private GameObject m_EnemySay;
    private Text m_SummonInfo;

    public bool isTurn = true;
    public bool canNext = true;
    public bool isPause = false;
    private bool m_isTest = false;
    private bool m_needSendEndTurn = false;
    
    public int summonIndex = 0;
    private int m_selectType = -1;
    public Config.PieceColor selfColor = Config.PieceColor.WHITE;//自己的颜色
    private IEnumerator m_roundTimer;   //计时器
    private IEnumerator m_tipsCoroutine;
    private List<GameObject> m_tips = new List<GameObject>();//走棋提示
    private List<GameObject> m_summonTips = new List<GameObject>();//召唤提示
    private List<GameObject> m_tempEnergy = new List<GameObject>();//能量
    private List<GameObject> m_EnergyEffectBoxes = new List<GameObject>();//能量高亮特效
    private ModelDrag02 m_modelDrag;
    private ModelClick m_modelClick;

    #region Panel 生命周期
    public void InitView(GameObject gameObject)
    {
        //templet
        m_mediator = new PVP02PanelMediator(this);
        App.Facade.RegisterMediator(m_mediator);
        m_proxy = App.Facade.RetrieveProxy("UserDataProxy") as UserDataProxy;
        m_pvpProxy = App.Facade.RetrieveProxy("PVP02Proxy") as PVP02Proxy;
        InitUIBinder(gameObject);
        m_modelClick.pvp02Panel = this;

        m_piece.SetActive(false);
        m_ready.onClick.AddListener(OnReadyClick);
        m_P.onClick.AddListener(OnPClick);
        m_N.onClick.AddListener(OnNClick);
        m_R.onClick.AddListener(OnRClick);
        m_B.onClick.AddListener(OnBClick);
        m_Q.onClick.AddListener(OnQClick);
        m_SelectCloce.onClick.AddListener(OnSelectCloseClick);
        m_Item1.onClick.AddListener(OnItem1Click);
        m_Item2.onClick.AddListener(OnItem2Click);
        m_Item3.onClick.AddListener(OnItem3Click);
        m_Item4.onClick.AddListener(OnItem4Click);
        m_SelfSay.onClick.AddListener(OnSayClick);
        m_EndTurn.onClick.AddListener(OnEndTurnClick);
        m_BottomToggle.onValueChanged.AddListener(OnBottomToggleClick);

        App.Facade.RegisterCommand(NotificationConstant.NewEndTurn, () => new NewEndTurnCommand());
        App.Facade.RegisterCommand(NotificationConstant.PlayerReady, () => new PlayerReadyCommand());
        App.Facade.RegisterCommand(NotificationConstant.PlayerActive, () => new PlayerActiveCommand());
        App.Facade.RegisterCommand(NotificationConstant.EndTurn, () => new EndTurnCommand());
        App.Facade.RegisterCommand(NotificationConstant.PlayerMutually, () => new PlayerMutuallyCommand());
        App.Facade.RegisterCommand(NotificationConstant.PlayerMutuallyFeedback, () => new PlayerMutuallyFeedbackCommand());
        App.Facade.RegisterCommand(NotificationConstant.PlayerChat, () => new PlayerChatCommand());

        App.NetworkManager.RegisterPushCall(Config.PushMessage.NewServerBattleMesPush, ShowOtherActive);
        App.NetworkManager.RegisterPushCall(Config.PushMessage.OnePlayerReady, OnOnePlayerReady);
        App.NetworkManager.RegisterPushCall(Config.PushMessage.PlayerNotReady, OnPlayerNotReady);
        App.NetworkManager.RegisterPushCall(Config.PushMessage.PlayerReadyFinish, OnReadyFinish);
        App.NetworkManager.RegisterPushCall(Config.PushMessage.PlayNext, OnNextPlay);
        App.NetworkManager.RegisterPushCall(Config.PushMessage.PlayerEnd, OnGameOver);
        
        App.NetworkManager.RegisterPushCall(Config.PushMessage.PlayerCanNextPush, OnCanNextPush);
        App.NetworkManager.RegisterPushCall(Config.PushMessage.PlayerCanPaintingPush, OnOtherEndTurnPush);
        App.NetworkManager.RegisterPushCall(Config.PushMessage.PlayerPaintingOverPush, OnRoundStartPush);

        App.NetworkManager.RegisterPushCall(Config.PushMessage.PlayerChatPush, OnPlayerChatPush);
    }

    private void InitUIBinder(GameObject gameObject)
    {
        m_qizi = GameObject.Find("qizi");
        m_piece = m_qizi.gameObject.transform.Find("m_Chess").gameObject;
        m_worldCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        m_cameraTween = m_worldCamera.GetComponent<TweenPlayer>();
        m_selfTimerObj = gameObject.transform.Find("Container/m_SelfTimer").gameObject;
        m_selfTimer = m_selfTimerObj.transform.Find("Text").GetComponent<Text>();
        m_enemyTimerObj = gameObject.transform.Find("Container/m_EnemyTimer").gameObject;
        m_enemyTimer = m_enemyTimerObj.transform.Find("Text").GetComponent<Text>();
        m_userImage = gameObject.transform.Find("Container/m_SelfIcon").GetComponent<Image>();
        m_enemyImage = gameObject.transform.Find("Container/m_EnemyIcon").GetComponent<Image>();
        m_modelDrag = GameObject.Find("board").GetComponent<ModelDrag02>();
        m_modelClick = GameObject.Find("board").GetComponent<ModelClick>();
        m_ready = gameObject.transform.Find("Container/m_Ready").gameObject.GetComponent<Button>();
        m_Bottom = gameObject.transform.Find("Container/m_Bottom").gameObject;
        m_BottomToggle = m_Bottom.GetComponent<Toggle>();
        m_EnergyContainer = m_Bottom.transform.Find("m_PowerContent/m_Energy").gameObject;
        m_EnergyText = m_Bottom.transform.Find("m_PowerContent/m_EnergyWidget/m_EnergyText").GetComponent<Text>();

        m_EndTurn = gameObject.transform.Find("Container/m_EndTurn").GetComponent<Button>();
        m_P = gameObject.transform.Find("Container/m_Bottom/m_CardLineup/Scroll View/Viewport/Content/m_P").gameObject.GetComponent<Button>();
        m_N = gameObject.transform.Find("Container/m_Bottom/m_CardLineup/Scroll View/Viewport/Content/m_N").gameObject.GetComponent<Button>();
        m_B = gameObject.transform.Find("Container/m_Bottom/m_CardLineup/Scroll View/Viewport/Content/m_B").gameObject.GetComponent<Button>();
        m_R = gameObject.transform.Find("Container/m_Bottom/m_CardLineup/Scroll View/Viewport/Content/m_R").gameObject.GetComponent<Button>();
        m_Q = gameObject.transform.Find("Container/m_Bottom/m_CardLineup/Scroll View/Viewport/Content/m_Q").gameObject.GetComponent<Button>();
        m_enemyReady = gameObject.transform.Find("Container/m_EnemyReady").gameObject;
        m_RoundChange = gameObject.transform.Find("Container/m_RoundChange").gameObject;
        m_sunLight = GameObject.Find("Directional Light");
        m_BlackAnimal = GameObject.Find("m_Rabbit").GetComponent<Animator>();
        m_WhiteAnimal = GameObject.Find("m_Cat").GetComponent<Animator>();
        m_EnemyEnergy = gameObject.transform.Find("Container/m_EnergyWidget").gameObject;
        m_SelfSay = gameObject.transform.Find("Container/m_SelfSay").GetComponent<Button>();
        m_EnemySay = gameObject.transform.Find("Container/m_EnemySay").gameObject;
        m_SelectCloce = m_SelfSay.transform.Find("Select/m_SelectClose").GetComponent<Button>();
        m_Item1 = m_SelfSay.transform.Find("Select/Item1").GetComponent<Button>();
        m_Item2 = m_SelfSay.transform.Find("Select/Item2").GetComponent<Button>();
        m_Item3 = m_SelfSay.transform.Find("Select/Item3").GetComponent<Button>();
        m_Item4 = m_SelfSay.transform.Find("Select/Item4").GetComponent<Button>();
        m_SummonInfo = gameObject.transform.Find("Container/m_Summon").gameObject.GetComponent<Text>();
    }

    public void OpenView(object intent)
    {
        isPause = false;
        canNext = true;
        m_needSendEndTurn = false;
        var rect = m_Bottom.GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, rect.anchoredPosition.y - App.ScreenFixedHeight);
        m_EnergyText.text = "0/0";
        int firstId = m_pvpProxy.GetFirstId();
        int userId = m_proxy.GetPlayerId();
        //test
        m_isTest = m_pvpProxy.GetTestMode();
        if (m_isTest)
        {
            isTurn = true;
            selfColor = Config.PieceColor.WHITE;
        }
        else
        {
            if (firstId == userId)
            {
                isTurn = true;
                selfColor = Config.PieceColor.WHITE;
                App.ResourceManager.LoadResource<UnityEngine.Object>("ArtRes/Icon/CatBt", (UnityEngine.Object obj) =>
                {
                    if (obj != null)
                    {
                        var texture = obj as Texture2D;
                        var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                        m_userImage.sprite = sprite;
                    }
                });
                App.ResourceManager.LoadResource<UnityEngine.Object>("ArtRes/Icon/BunnyBt", (UnityEngine.Object obj) =>
                {
                    if (obj != null)
                    {
                        var texture = obj as Texture2D;
                        var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                        m_enemyImage.sprite = sprite;
                    }
                });
            }
            else
            {
                m_sunLight.transform.localRotation = Quaternion.Euler(145, -30, 0);
                isTurn = false;
                selfColor = Config.PieceColor.BLACK;
                App.ResourceManager.LoadResource<UnityEngine.Object>("ArtRes/Icon/CatBt", (UnityEngine.Object obj) =>
                {
                    if (obj != null)
                    {
                        var texture = obj as Texture2D;
                        var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                        m_enemyImage.sprite = sprite;
                    }
                });
                App.ResourceManager.LoadResource<UnityEngine.Object>("ArtRes/Icon/BunnyBt", (UnityEngine.Object obj) =>
                {
                    if (obj != null)
                    {
                        var texture = obj as Texture2D;
                        var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                        m_userImage.sprite = sprite;
                    }
                });
            }
        }
        m_pvpProxy.SetSelfColor(selfColor);
        //m_userImage.GetComponentInChildren<Text>().text = m_proxy.GetPlayerName();
        m_enemyImage.GetComponentInChildren<Text>().text = m_pvpProxy.GetEnemyName();
        InitTimer();
        App.SoundManager.PlaySoundClip(Config.Sound.InGameStart);
        var fixedK = App.EffectManager.ScreenFixedK;
        m_worldCamera.fieldOfView = m_worldCamera.fieldOfView * fixedK;
    }

    public void CloseView()
    {

    }

    public void DestroyView()
    {
        App.Facade.RemoveMediator(m_mediator.MediatorName);
        RemovePush();
    }
    #endregion

    #region 回合流程

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
        summonIndex = 0;
        m_mediator.Energy = 1;
        m_mediator.CurPieceNum = 0;
        if (isTurn)
        {
            ShowTransAnimation(false);
        }
        else
        {
            ShowTransAnimation(true);
        }
    }

    public void OnGameOver(string name, List<byte[]> packet)
    {
        var pushMes = PlayerEndPush.ParseFrom(packet[0]);
        var winId = pushMes.WinUserId;
        var result = pushMes.Result;
        var winScore = pushMes.WinRank;
        var loseScore = pushMes.LoseRank;
        if(winId == m_pvpProxy.GetFirstId())
        {
            m_BlackAnimal.Play("Die");
            m_WhiteAnimal.Play("Jump");
        }
        else
        {
            m_BlackAnimal.Play("Jump");
            m_WhiteAnimal.Play("Die");
        }
        Debug.Log("GameOver!!");
        App.SoundManager.StopBackSound();
        if (result >= 6)
        {
            App.UIManager.OpenPanel("ResultPanel", new object[] { Config.GameResult.DRAW, "0" });
        }
        else
        {
            if (winId == m_proxy.GetPlayerId())
            {
                App.UIManager.OpenPanel("ResultPanel", new object[] { Config.GameResult.WIN, "+" + winScore });
            }
            else
            {
                App.UIManager.OpenPanel("ResultPanel", new object[] { Config.GameResult.LOSE, "-" + loseScore });
            }
        }
        StopRoundTimer();
    }

    /// <summary>
    /// 回合开始
    /// </summary>
    private void OnRoundStart()
    {
        UpdateSummonInfo();
        m_SummonInfo.gameObject.SetActive(true);
        m_Bottom.GetComponent<TweenPlayer>().PlayOne("move_show");
        StopRoundTimer();
        m_mediator.NotifyRoundBegin(selfColor);       
        if(m_mediator.MaxEnergy < 10)
        {
            m_mediator.MaxEnergy++;
        }
        m_mediator.Energy = m_mediator.MaxEnergy;
        ShowEnergyChange(m_mediator.MaxEnergy);
        m_EnergyText.text = m_mediator.Energy + "/" + m_mediator.MaxEnergy;
        isTurn = true;
        canNext = true;
        m_modelDrag.isTurn = isTurn;
        m_EndTurn.gameObject.SetActive(true);
        StartRoundTimer();
    }

    /// <summary>
    /// 停掉回合中的一些表现
    /// </summary>
    private void StopRoundShow()
    {
        m_SummonInfo.gameObject.SetActive(false);
        m_mediator.NotifyRoundEnd();
        m_BottomToggle.isOn = false;
        var tweenPlayer = m_Bottom.GetComponent<TweenPlayer>();       
        var tween = tweenPlayer.GetClipTween("move_hide");
        tween.SetTo(new Vector3(0, -App.ScreenFixedHeight + 3, 0));
        tween.Play();
        m_selfTimerObj.SetActive(false);
        m_enemyTimerObj.SetActive(false);
        m_modelDrag.isTurn = false;
        m_EndTurn.gameObject.SetActive(false);
        CardSelectReset();
        OnEnergyEffectHide();
        OnSummonTipsHide();
        var selfRound = m_RoundChange.transform.Find("m_MyselfRound").GetComponent<CanvasGroup>();
        selfRound.alpha = 0;

        var enemyRound = m_RoundChange.transform.Find("m_EnemyRound").GetComponent<CanvasGroup>();
        enemyRound.alpha = 0;
    }

    private void ShowEnergyChange(int maxEnergy)
    {
        m_tempEnergy.Clear();
        for (int i = 0; i < maxEnergy; i++)
        {
            var name = (i + 1).ToString();
            var energyObj = m_EnergyContainer.transform.Find(name).gameObject;
            energyObj.SetActive(true);
            var maskObj = energyObj.transform.Find("Mask").gameObject;
            maskObj.SetActive(false);
            m_tempEnergy.Add(energyObj);
        }
    }

    /// <summary>
    /// 自己的回合结束 
    /// </summary>
    private void OnRoundEnd()
    {
        isTurn = false;
        m_modelDrag.isTurn = isTurn;
        m_EndTurn.gameObject.SetActive(false);
        StopRoundTimer();
        StartRoundTimer();
        m_mediator.E_MaxEnergy = m_mediator.MaxEnergy;
        if(selfColor == Config.PieceColor.BLACK)
        {
            m_mediator.E_MaxEnergy++;
        }
        m_mediator.E_Energy = m_mediator.E_MaxEnergy;
        m_EnemyEnergy.SetActive(true);
        var text = m_EnemyEnergy.transform.Find("m_EnergyText").GetComponent<Text>();
        text.text = m_mediator.E_Energy + "/" + m_mediator.E_MaxEnergy;
    }
    #endregion

    /// <summary>
    /// 初始化棋盘
    /// </summary>
    private void InitChessBoard()
    {
        for (int y = 0; y < Config.Board.MaxY; y++)
        {
            for (int x = 0; x < Config.Board.MaxX; x++)
            {
                int piece = App.ChessLogic02.GetPiece(x, y);
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
                    PieceItem02 pieceItem = temp.AddComponent<PieceItem02>();
                    summonIndex++;
                    pieceItem.InitView(temp, new Piece((Config.PieceColor)color, (Config.PieceType)type, x + 1, y + 1), summonIndex, false);

                    App.ObjectPoolManager.RegisteObject(pieceName, "FX/" + pieceName, 0, 30, -1);
                    App.ObjectPoolManager.Instantiate(pieceName, (GameObject obj) =>
                    {
                        if (selfColor == Config.PieceColor.WHITE)
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
    /// 播放转场动画
    /// </summary>
    private void ShowTransAnimation(bool isEnd)
    {
        m_RoundChange.SetActive(true);
        if (isEnd == true)
        {
            var tweenPlayer = m_RoundChange.transform.Find("m_EnemyRound").GetComponent<TweenPlayer>();
            var tween = tweenPlayer.GetClipTween("move_top");
            tween.SetOnComplete(OnTransAniComplete, new object[] { tweenPlayer });
            tweenPlayer.enabled = true;
        }
        else
        {
            var tweenPlayer = m_RoundChange.transform.Find("m_MyselfRound").GetComponent<TweenPlayer>();
            var tween = tweenPlayer.GetClipTween("move_bottom");
            tween.SetOnComplete(OnTransAniComplete, new object[] { tweenPlayer });
            tweenPlayer.enabled = true;
        }
    }

    private void OnTransAniComplete(object[] args)
    {
        var tweenPlayer = (TweenPlayer)args[0];
        tweenPlayer.enabled = false;
        m_mediator.NotifyEndTurn(1);
        if(m_isTest == true)
        {
            OnRoundStart();        
        }
    }

    /// <summary>
    /// 提示
    /// </summary>
    /// <param name="from"></param>
    public void OnTipsShow(Vector2 from, bool isBlue = false)
    {
        var moves = App.ChessLogic02.NewGenerateMoves(new Vector2(from.x - 1, from.y - 1));
        moves = moves.OrderBy(v => v.Distance).ToList();
        string colorName = "green";
        if(isBlue == true)
        {
            colorName = "blue";
        }
        if(m_tipsCoroutine == null)
        {
            m_tipsCoroutine = _OnTipsShow(moves, colorName);
            App.UIManager.StartCoroutine(m_tipsCoroutine);
        }        
    }

    /// <summary>
    /// 提示
    /// </summary>
    /// <param name="from"></param>
    public IEnumerator _OnTipsShow(List<Move> moves, string colorName)
    {
        int distance = moves[0].Distance;
        foreach (Move move in moves)
        {
            App.ObjectPoolManager.RegisteObject("biankuang_" + colorName, "FX/biankuang_" + colorName, 0, 30, -1);
            App.ObjectPoolManager.Instantiate("biankuang_"+ colorName, (GameObject obj) =>
            {
                obj.SetActive(true);
                obj.transform.parent = m_qizi.transform;
                obj.transform.localPosition = new Vector3(move.To.x * Config.PieceWidth, 3, move.To.y * Config.PieceWidth);
                obj.transform.localScale = new Vector3(15, 15, 1);
                m_tips.Add(obj);
                var tweenPlayer = obj.transform.Find("box_01/light1").GetComponent<TweenPlayer>();
                tweenPlayer.enabled = true;
            });
            if (move.Distance > distance)
            {
                distance = move.Distance;
                yield return new WaitForSeconds(0.05f);
            }
        }
    }

    /// <summary>
    /// 召唤提示显示
    /// </summary>
    public void OnSummonTipsShow()
    {
        var points = App.ChessLogic02.GetSummonPoints((int)selfColor);
        foreach (Vector2 to in points)
        {
            App.ObjectPoolManager.RegisteObject("biankuang_yellow", "FX/biankuang_yellow", 0, 30, -1);
            App.ObjectPoolManager.Instantiate("biankuang_yellow", (GameObject obj) =>
            {
                obj.SetActive(true);
                obj.transform.parent = m_qizi.transform;
                obj.transform.localPosition = new Vector3(to.x * Config.PieceWidth, 3, to.y * Config.PieceWidth);
                obj.transform.localScale = new Vector3(15, 15, 1);
                m_summonTips.Add(obj);
            });
        }
    }

    public void OnSummonTipsHide()
    {
        foreach (GameObject obj in m_summonTips)
        {
            App.ObjectPoolManager.Release("biankuang_yellow", obj);
        }
        m_summonTips.Clear();
    }

    public void OnEnergyEffectHide()
    {
        foreach (GameObject obj in m_EnergyEffectBoxes)
        {
            obj.SetActive(false);
        }
        m_EnergyEffectBoxes.Clear();        
    }

    private void CardSelectReset()
    {
        m_P.transform.Find("m_Border").gameObject.SetActive(false);
        m_N.transform.Find("m_Border").gameObject.SetActive(false);
        m_B.transform.Find("m_Border").gameObject.SetActive(false);
        m_R.transform.Find("m_Border").gameObject.SetActive(false);
        m_Q.transform.Find("m_Border").gameObject.SetActive(false);
    }

    public void OnPieceTypeSelect(int type)
    {
        m_selectType = -1;
        OnSummonTipsHide();
        OnEnergyEffectHide();
        if (Config.PieceCost[type] > m_mediator.Energy)
        {
            App.UIManager.OpenPanel("MessagePanel", "能量不足！");
        }
        else if(m_mediator.CurPieceNum >= Config.MaxPieceNum)
        {
            App.UIManager.OpenPanel("MessagePanel", "最多只能存在" + Config.MaxPieceNum + "个棋子！");
        }
        else
        {
            App.SoundManager.PlaySoundClip(Config.Sound.SummonCardClick);
            for(int i = 1; i <= Config.PieceCost[type]; i++)
            {
                var boxObj = m_tempEnergy[m_mediator.Energy - i].transform.Find("EffectBox").gameObject;
                boxObj.SetActive(true);
                m_EnergyEffectBoxes.Add(boxObj);
            }
            
            OnSummonTipsShow();
            m_selectType = type + (int)selfColor * 10;
        }
    }

    public void OnTipsHide()
    {
        if(m_tipsCoroutine!= null)
        {
            App.UIManager.StopCoroutine(m_tipsCoroutine);
            m_tipsCoroutine = null;
        }
        foreach (GameObject obj in m_tips)
        {
            App.ObjectPoolManager.Release(obj.name, obj);
        }
        m_tips.Clear();
    }

    public void OnPieceAnimatorStop()
    {
        m_mediator.NotifyPieceAnimatorStop();
    }

    private void InitTimer()
    {
        m_selfTimerObj.SetActive(false);
        m_enemyTimerObj.SetActive(false);
    }

 
    /// <summary>
    /// 将军
    /// </summary>
    public void OnCheck(int color)
    {
        if (color == (int)selfColor)
        {
            Debug.Log("=======被将军======== ");
        }
        else
        {
            Debug.Log("=======将军======== ");
        }
    }

    public void EndCurRound()
    {
        ShowTransAnimation(true);
    }

    public void SetCanNext(bool isCan)
    {
        if (isTurn == true)
        {
            canNext = isCan;
            m_modelDrag.isTurn = isCan;
        }
    }

    private void OnTweenCompleteCommon(object[] args)
    {
        var tweemPlayer = (TweenPlayer)args[0];
        tweemPlayer.enabled = false;
    }

    private void OnChatShow(int index)
    {
        var tweenPlayer = m_EnemySay.GetComponent<TweenPlayer>();
        tweenPlayer.enabled = true;
        tweenPlayer.SetClipOnComplete("chat_hide", OnTweenCompleteCommon, new object[] { tweenPlayer });     
        switch (index)
        {
            case 1:
                m_EnemySay.transform.Find("Text").GetComponent<Text>().text = "祝你好运！";
                break;
            case 2:
                m_EnemySay.transform.Find("Text").GetComponent<Text>().text = "打的不错！";
                break;
            case 3:
                m_EnemySay.transform.Find("Text").GetComponent<Text>().text = "抱歉";
                break;
            case 4:
                m_EnemySay.transform.Find("Text").GetComponent<Text>().text = "......";
                break;
        }
    }

    private void UpdateSummonInfo()
    {
        m_SummonInfo.text = "还能召唤" + (Config.MaxPieceNum - m_mediator.CurPieceNum) + "个...";
    }

    /// <summary>
    /// 召唤
    /// </summary>
    /// <param name="type"></param>
    /// <param name="point"></param>
    private void OnSummon(int type, Vector2 point)
    {
        int color = type / 10;
        int newType = type % 10;
        string pieceName = "";
        if (color == (int)Config.PieceColor.BLACK)
        {
            pieceName = "Black_";
        }
        else
        {
            pieceName = "White_";
        }
        switch ((Config.PieceType)newType)
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
        temp.name = point.x + "_" + point.y;
        temp.transform.parent = m_qizi.transform;
        temp.SetActive(true);
        PieceItem02 pieceItem = temp.AddComponent<PieceItem02>();
        summonIndex++;
        pieceItem.InitView(temp, new Piece((Config.PieceColor)color, (Config.PieceType)newType, (int)point.x, (int)point.y), summonIndex, false);
        pieceItem.canMove = false;
        if (isTurn == true)
        {
            canNext = false;
            m_modelDrag.isTurn = canNext;
        }
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
            var effectPlayer = App.EffectManager.LoadEffect(obj, "summon_normal");
            effectPlayer.enabled = true;
            effectPlayer.IsOnce = true;
            App.SoundManager.PlaySoundClip(Config.Sound.SummonSuccess);
            App.UIManager.StartCoroutine(_SendPaintEnd(newType));
        });
    }

    private IEnumerator _SendPaintEnd(int type)
    {
        yield return new WaitForSeconds(1);
        if(isTurn == true)
        {
            var cost = Config.PieceCost[type];
            for (int i = 1; i <= cost; i++)
            {
                var maskObj = m_tempEnergy[m_mediator.Energy + cost - i].transform.Find("Mask").gameObject;
                maskObj.SetActive(true);
                m_EnergyText.text = m_mediator.Energy + "/" + m_mediator.MaxEnergy;
            }
            m_mediator.CurPieceNum++;
            UpdateSummonInfo();
        }
        m_mediator.NotifyEndTurn(1);
    }

    #region Response Method

    public void OnReadyResponse()
    {
        m_ready.gameObject.SetActive(false);
    }

    public void OnMutuallyResponse()
    {
        m_modelDrag.isTurn = false;
        isPause = true;
        App.UIManager.OpenPanel("WaitingPanel", "等待对方选择...");
    }
    #endregion 

    #region OnClick Method

    private void OnReadyClick()
    {
        App.SoundManager.PlaySoundClip(Config.Sound.Click1);
        if(m_isTest == false)
        {
            m_mediator.NotifySelfReady(1);
        }
        else
        {
            OnReadyFinish("", null);
        }
        //test
        m_ready.gameObject.SetActive(false);
        m_Bottom.SetActive(true);     
    }

    private void OnEndTurnClick()
    {
        StopRoundShow();
        if (m_isTest == false)
        {
            if(canNext == true)
            {
                m_needSendEndTurn = false;
                m_mediator.NotifyNewEndTurn();
                App.SoundManager.PlaySoundClip(Config.Sound.RoundSwitch);
            }
            else
            {
                m_needSendEndTurn = true;
            }            
        }
        else
        {
            ShowTransAnimation(true);
        }
    }

    private void OnBottomToggleClick(bool isOn)
    {
        if(isOn == true)
        {
            m_Bottom.GetComponent<TweenPlayer>().PlayOne("move_in");
        }
        else
        {
            m_Bottom.GetComponent<TweenPlayer>().PlayOne("move_out");
        }
    }

    public void OnModelClick(Vector3 clickPoint)
    {
        if(m_selectType > -1)
        {
            var pointX = clickPoint.x;
            var pointZ = clickPoint.z;
            var offsetX = pointX % 2;
            var offsetZ = pointZ % 2;
            var x = (pointX - offsetX) / 2;
            var z = (pointZ - offsetZ) / 2;
            var point = new Vector2(x, z);

            if(canNext == true && App.ChessLogic02.DoSummon(point, m_selectType))
            {
                Debug.Log("召唤成功");
                var type = m_selectType % 10;
                m_mediator.Energy -= Config.PieceCost[type];
                if(m_isTest == false)
                {
                    ActiveInfo.Builder activeInfo = ActiveInfo.CreateBuilder();
                    activeInfo.SetIsCall(true);
                    CallInfo.Builder callInfo = CallInfo.CreateBuilder();
                    callInfo.SetIndex(CoorToIndex((int)(x + 1), (int)(z + 1)));
                    callInfo.SetType(type);
                    callInfo.SetUserId(m_proxy.GetPlayerId());
                    activeInfo.SetCallInfo(callInfo);
                    m_mediator.NotifyPlayerActive(activeInfo);
                }               
                OnSummon(m_selectType, new Vector2(point.x + 1, point.y + 1));
            }
            else
            {
                Debug.Log("召唤失败");
            }
            m_selectType = -1;
            OnSummonTipsHide();
            OnEnergyEffectHide();
        }
        else
        {
            var pointX = clickPoint.x;
            var pointZ = clickPoint.z;
            var offsetX = pointX % 2;
            var offsetZ = pointZ % 2;
            var x = (pointX - offsetX) / 2;
            var z = (pointZ - offsetZ) / 2;
            var point = new Vector2(x + 1, z + 1);

            m_mediator.NotifyPieceClick(point);
        }
    }

    public void OnClickCancel()
    {
        OnTipsHide();
    }

    private void OnPClick()
    {
        if(isTurn == true)
        {
            m_P.transform.Find("m_Border").gameObject.SetActive(true);
            m_N.transform.Find("m_Border").gameObject.SetActive(false);
            m_B.transform.Find("m_Border").gameObject.SetActive(false);
            m_R.transform.Find("m_Border").gameObject.SetActive(false);
            m_Q.transform.Find("m_Border").gameObject.SetActive(false);
            OnPieceTypeSelect(0);
        }   
    }

    private void OnNClick()
    {
        if (isTurn == true)
        {
            m_P.transform.Find("m_Border").gameObject.SetActive(false);
            m_N.transform.Find("m_Border").gameObject.SetActive(true);
            m_B.transform.Find("m_Border").gameObject.SetActive(false);
            m_R.transform.Find("m_Border").gameObject.SetActive(false);
            m_Q.transform.Find("m_Border").gameObject.SetActive(false);
            OnPieceTypeSelect(1);
        }
    }

    private void OnBClick()
    {
        if (isTurn == true)
        {
            m_P.transform.Find("m_Border").gameObject.SetActive(false);
            m_N.transform.Find("m_Border").gameObject.SetActive(false);
            m_B.transform.Find("m_Border").gameObject.SetActive(true);
            m_R.transform.Find("m_Border").gameObject.SetActive(false);
            m_Q.transform.Find("m_Border").gameObject.SetActive(false);
            OnPieceTypeSelect(2);
        }
    }

    private void OnRClick()
    {
        if (isTurn == true)
        {
            m_P.transform.Find("m_Border").gameObject.SetActive(false);
            m_N.transform.Find("m_Border").gameObject.SetActive(false);
            m_B.transform.Find("m_Border").gameObject.SetActive(false);
            m_R.transform.Find("m_Border").gameObject.SetActive(true);
            m_Q.transform.Find("m_Border").gameObject.SetActive(false);
            OnPieceTypeSelect(3);
        }
    }

    private void OnQClick()
    {
        if (isTurn == true)
        {
            m_P.transform.Find("m_Border").gameObject.SetActive(false);
            m_N.transform.Find("m_Border").gameObject.SetActive(false);
            m_B.transform.Find("m_Border").gameObject.SetActive(false);
            m_R.transform.Find("m_Border").gameObject.SetActive(false);
            m_Q.transform.Find("m_Border").gameObject.SetActive(true);
            OnPieceTypeSelect(4);
        }
    }

    private void OnSayClick()
    {
        m_SelfSay.transform.Find("Select").gameObject.SetActive(true);
    }

    private void OnSelectCloseClick()
    {
        m_SelfSay.transform.Find("Select").gameObject.SetActive(false);
    }

    private void OnItem1Click()
    {
        Debug.Log("1");
        m_SelfSay.transform.Find("Select").gameObject.SetActive(false);
        m_mediator.NotifyPlayerChat(1);
    }

    private void OnItem2Click()
    {
        Debug.Log("2");
        m_SelfSay.transform.Find("Select").gameObject.SetActive(false);
        m_mediator.NotifyPlayerChat(2);
    }

    private void OnItem3Click()
    {
        Debug.Log("3");
        m_SelfSay.transform.Find("Select").gameObject.SetActive(false);
        m_mediator.NotifyPlayerChat(3);
    }

    private void OnItem4Click()
    {
        Debug.Log("4");
        m_SelfSay.transform.Find("Select").gameObject.SetActive(false);
        m_mediator.NotifyPlayerChat(4);
    }

    #endregion

    #region Coroutine Method
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
            m_selfTimerObj.SetActive(true);
            m_enemyTimerObj.SetActive(false);
            timer = m_selfTimer;
        }
        else
        {
            m_selfTimerObj.SetActive(false);
            m_enemyTimerObj.SetActive(true);
            timer = m_enemyTimer;
        }
        for (int i = 0; i < Config.Game.NewWaitingRound; i++)
        {
            while (isPause == true)
            {
                yield return new WaitForSeconds(1);
            }
            timer.text = (Config.Game.NewWaitingRound - i) + "s";
            yield return new WaitForSeconds(1);
        }
        StopRoundShow();
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

    #region Push Listener
    public void OnPlayerNotReady(string name, List<byte[]> packet)
    {
        App.UIManager.OpenPanel("TipsPanel", new object[] { "对局取消！", "有玩家没准备，对局取消！" });
    }

    private void ShowOtherActive(string name, List<byte[]> packet)
    {
        var pushMes = NewServerBattleMesPush.ParseFrom(packet[0]);
        bool isCall = pushMes.ActiveInfo.IsCall;
        //召唤
        if (isCall == true)
        {       
            var callInfo = pushMes.ActiveInfo.CallInfo;
            var index = callInfo.Index;
            var point = IndexToCoor(index);
            var otherColor = 1 - (int)selfColor;
            var type = callInfo.Type  + otherColor * 10;
            var userId = callInfo.UserId;

            if (canNext == true && App.ChessLogic02.DoSummon(new Vector2(point.x - 1, point.y - 1), type))
            {
                m_mediator.E_Energy -= Config.PieceCost[callInfo.Type];
                var text = m_EnemyEnergy.transform.Find("m_EnergyText").GetComponent<Text>();
                text.text = m_mediator.E_Energy + "/" + m_mediator.E_MaxEnergy;
                OnSummon(type, point);
            }
            else
            {
                Debug.Log("非法召唤！！");
            }
        }
        //移动
        else
        {
            var moveInfo = pushMes.ActiveInfo.MoveInfo;
            var fromIndex = moveInfo.From;
            var toIndex = moveInfo.To;
            var color = m_pvpProxy.GetEnemyColor();
            var from = IndexToCoor(fromIndex);
            var to = IndexToCoor(toIndex);
            m_mediator.NotifyOtherMove(new Vector2[] { from, to, new Vector2(-1, 0) });//todo response
        }
        Debug.Log("Push====: OtherActive!!!!");
    }

    public void OnOnePlayerReady(string name, List<byte[]> packet)
    {
        m_enemyReady.SetActive(true);
    }

    public void OnReadyFinish(string name, List<byte[]> packet)
    {
        if (isTurn == true)
        {
            m_cameraTween.SetTweenPackSync("camera_start_white");
        }
        else
        {
            m_cameraTween.SetTweenPackSync("camera_start_black");
        }
        var clip = m_cameraTween.GetClipTween("move_start");
        clip.SetOnComplete(OnCameratweenComplete, null);
        Debug.Log("Push:On Ready Finish");
    }

    private void OnCameratweenComplete(object[] args)
    {
        OnGameStart();
    }

    /// <summary>
    /// 可以召唤下一个棋子
    /// </summary>
    /// <param name="name"></param>
    /// <param name="packet"></param>
    private void OnCanNextPush(string name, List<byte[]> packet)
    {
        Debug.Log("Push: CanNextPiecePush!!!");
        if (isTurn == true)
        {
            canNext = true;
            m_modelDrag.isTurn = canNext;
            if (m_needSendEndTurn == true)
            {
                m_mediator.NotifyNewEndTurn();
                App.SoundManager.PlaySoundClip(Config.Sound.RoundSwitch);
                m_needSendEndTurn = false;
            }
        }
    }

    private void OnOtherEndTurnPush(string name, List<byte[]> packet)
    {
        m_EnemyEnergy.SetActive(false);
        Debug.Log("Push: OtherEndTurnPush!!!");
        App.SoundManager.PlaySoundClip(Config.Sound.RoundSwitch);
        StopRoundShow();
        ShowTransAnimation(isTurn);
    }

    /// <summary>
    /// 开局动画播完 可以进行回合了
    /// </summary>
    /// <param name="name"></param>
    /// <param name="packet"></param>
    private void OnRoundStartPush(string name, List<byte[]> packet)
    {
        Debug.Log("Push: OnRoundStartPush!!!");
        if (isTurn == true)
        {
            OnRoundStart();
        }
        else
        {
            OnRoundEnd();
        }
    }

    /// <summary>
    /// 快捷发言
    /// </summary>
    /// <param name="name"></param>
    /// <param name="packet"></param>
    private void OnPlayerChatPush(string name, List<byte[]> packet)
    {
        var pushMes = PlayerChatPush.ParseFrom(packet[0]);
        var index = pushMes.Number;
        OnChatShow(index);
        Debug.Log("push index" + index);
    }

    /// <summary>
    /// 双方回合切换动画都播完
    /// </summary>
    /// <param name="name"></param>
    /// <param name="packet"></param>
    public void OnNextPlay(string name, List<byte[]> packet)
    {
        Debug.Log("Push:OnNextPlayPush");
        if (isTurn == true)
        {
            OnRoundEnd();
        }
        else
        {
            OnRoundStart();
        }
    }
    #endregion

    private void RemovePush()
    {
        App.NetworkManager.RemovePushCall(Config.PushMessage.OtherMove);
        App.NetworkManager.RemovePushCall(Config.PushMessage.OnePlayerReady);
        App.NetworkManager.RemovePushCall(Config.PushMessage.PlayerNotReady);
        App.NetworkManager.RemovePushCall(Config.PushMessage.PlayerReadyFinish);
        App.NetworkManager.RemovePushCall(Config.PushMessage.PlayNext);
        App.NetworkManager.RemovePushCall(Config.PushMessage.PlayerEnd);

        App.NetworkManager.RemovePushCall(Config.PushMessage.PlayerNotAgreePush);
        App.NetworkManager.RemovePushCall(Config.PushMessage.PlayerUndoInfoPush);
        App.NetworkManager.RemovePushCall(Config.PushMessage.PlayerUndoPush);
        App.NetworkManager.RemovePushCall(Config.PushMessage.PlayUndoNextPush);

        App.NetworkManager.RemovePushCall(Config.PushMessage.PlayerCanNextPush);
        App.NetworkManager.RemovePushCall(Config.PushMessage.NewServerBattleMesPush);
        App.NetworkManager.RemovePushCall(Config.PushMessage.PlayerPaintingOverPush);
        App.NetworkManager.RemovePushCall(Config.PushMessage.PlayerCanPaintingPush);

        App.NetworkManager.RemovePushCall(Config.PushMessage.PlayerChatPush);
    }

    ///index转坐标 且棋盘翻转
    private Vector2 IndexToCoor(int index)
    {
        int y = (64 - index) / Config.Board.MaxX + 1;
        int dx = (64 - index) % Config.Board.MaxX - 1;
        int x = Config.Board.MaxX - dx - 1;
        return new Vector2(x, y);
    }

    ///坐标转index 且棋盘翻转
    private int CoorToIndex(int x, int y)
    {
        int index = 65 - (y * Config.Board.MaxX - x + 1);
        return index;
    }
}