﻿using com.zyd.common.proto.client;
using Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class PieceItem02 : MonoBehaviour
{
    private GameObject m_gameObject;
    private GameObject m_target;
    private PieceItem02Mediator m_mediator;
    private string m_name;
    private PVP02Proxy m_pvpProxy;
    private UserDataProxy m_UserProxy;
    private Vector3 m_beginPos;//初始位置
    public Config.PieceColor selfColor;
    public Config.PieceColor pieceColor;
    private bool m_isTipsShow = false;
    private Config.PieceType m_type;//棋子的类型
    public float m_X;
    public float m_Z;
    public int m_roundNum;//注册mediator唯一标识
    public bool isDead = false;
    public bool isPVE = false;
    public bool isReborn = false;
    public bool canMove = false;//棋子一回合只能移动一次
    public GameObject pieceModel;
    private GameObject m_Attack;
    private TweenPlayer m_TweenPlayer;
    private TweenPlayer m_ModelTween;
    private object[] m_body;
    private Vector2[] m_otherBody;
    private Vector2 m_undoFrom;
    private Animator m_pieceAnimator;

    public void InitView(GameObject gameObject, Piece pieceData, int roundNum, bool pve = false)
    {
        isPVE = pve;
        m_gameObject = gameObject;
        m_roundNum = roundNum;
        //m_gameObject.transform.localRotation = Quaternion.Euler(0, 180, 0);
        m_Attack = gameObject.transform.Find("m_Attack").gameObject;
        m_UserProxy = App.Facade.RetrieveProxy("UserDataProxy") as UserDataProxy;
        m_pvpProxy = App.Facade.RetrieveProxy("PVP02Proxy") as PVP02Proxy;
        selfColor = m_pvpProxy.GetSelfColor();
        m_type = pieceData.type;
        pieceColor = pieceData.color;
        InitPieceShow(pieceData);
        m_name = m_X + "_" + m_Z + "_" + m_roundNum;
        m_mediator = new PieceItem02Mediator(this, m_name);
        App.Facade.RegisterMediator(m_mediator);
        m_mediator.InitPieceData(pieceData);

        OpenView();
    }

    private void InitPieceShow(Piece pieceData)
    {
        //不是自己的棋子 不能拖
        if (isPVE == false)
        {
            if (m_pvpProxy.GetSelfColor() != pieceData.color)
            {
                var collider = GetComponent<CapsuleCollider>();
                if (collider != null)
                {
                    collider.enabled = false;
                }
            }
        }
        SetPiecePos(pieceData.x, pieceData.y);
        if (m_target == null)
        {
            App.ObjectPoolManager.RegisteObject("m_target", "FX/m_target", 0, 30, -1);
            App.ObjectPoolManager.Instantiate("m_target", (GameObject obj) =>
            {
                //obj.SetActive(true);
                obj.transform.parent = transform;
                obj.transform.localPosition = Vector3.zero;
                obj.transform.localScale = new Vector3(2, 1.5f, 1);
                m_target = obj;
            });
        }
    }

    public void SetPiecePos(float x, float z)
    {
        m_gameObject.transform.localPosition = new Vector3((x - 1) * Config.PieceWidth, 0, (z - 1) * Config.PieceWidth);
        m_X = x;
        m_Z = z;
        name = m_X + "_" + m_Z;
        if (m_target != null)
        {
            m_target.transform.localPosition = Vector3.zero;
        }
    }

    private void InitAttackPoint()
    {
        switch (m_type)
        {
            case Config.PieceType.B:
                m_Attack.transform.localPosition = Config.AttackPos.B_AttackPoint;
                break;
            case Config.PieceType.R:
                m_Attack.transform.localPosition = Config.AttackPos.R_AttackPoint;
                break;
            case Config.PieceType.Q:
                m_Attack.transform.localPosition = Config.AttackPos.Q_AttackPoint;
                break;
            case Config.PieceType.P:
                m_Attack.transform.localPosition = Config.AttackPos.P_AttackPoint;
                break;
            case Config.PieceType.K:
                m_Attack.transform.localPosition = Config.AttackPos.K_AttackPoint;
                break;
            case Config.PieceType.N:
                m_Attack.transform.localPosition = Config.AttackPos.N_AttackPoint;
                break;
        }
    }

    /// <summary>
    /// 被打了
    /// </summary>
    public void BeAttached()
    {
        Debug.Log("被打了~ m_x=" + m_X + "m_z=" + m_Z);
        App.SoundManager.PlaySoundClip(Config.Sound.BMagicAttack);
        var effectPlayer = App.EffectManager.LoadEffect(m_gameObject, "normal_dead");
        effectPlayer.IsOnce = true;
        effectPlayer.enabled = true;
        App.ObjectPoolManager.Release("m_target", m_target);
        if (m_mediator.pieceData.type == Config.PieceType.K)
        {
            object loseColor = m_mediator.pieceData.color;
            m_mediator.NotityGameOver(loseColor);
        }
        isDead = true;
        m_mediator.NotifySomeoneDead();
        OnDestroy();
    }

    private void OnDestroy()
    {
        App.Facade.RemoveMediator(m_mediator.MediatorName);
        Destroy(gameObject, 1);
        Destroy(this, 1);
    }

    public void OnGameOver()
    {
        OnDestroy();
    }

    public void OnRoundBegin()
    {
        canMove = true;
        if(m_type != Config.PieceType.K && selfColor == pieceColor)
        {
            pieceModel.GetComponent<TweenPlayer>().enabled = true;
        }
    }

    /// <summary>
    /// 拖动开始
    /// </summary>
    public void OnDragBegin()
    {
        if(canMove == true && m_type!= Config.PieceType.K)
        {
            m_beginPos = transform.position;
            m_target.SetActive(true);
            App.SoundManager.PlaySoundClip(Config.Sound.DragBegin);
        }
        else
        {
            App.SoundManager.PlaySoundClip(Config.Sound.DragFail);
        }
    }

    /// <summary>
    /// 拖动中
    /// </summary>
    /// <param name="currentPosition"></param>
    public void OnDrag(Vector3 currentPosition)
    {
        if(canMove == true && m_type != Config.PieceType.K)
        {
            float tempZ = 0;
            if (selfColor == Config.PieceColor.WHITE)
            {
                tempZ = currentPosition.z + currentPosition.y - m_beginPos.y;
            }
            else
            {
                tempZ = currentPosition.z - currentPosition.y + m_beginPos.y;
            }
            m_target.transform.position = new Vector3(currentPosition.x, m_beginPos.y, tempZ);
            var px = m_target.transform.position.x - m_beginPos.x;
            var pz = m_target.transform.position.z - m_beginPos.z;
            var d = Math.Sqrt(px * px + pz * pz);
            if (d >= Config.TipsDistance && m_isTipsShow == false)
            {
                m_isTipsShow = true;
                m_mediator.NotifyDragTips(new Vector2(m_X, m_Z));
            }
        }
    }

    /// <summary>
    /// 拖动结束
    /// </summary>
    public void OnDragEnd()
    {
        if (canMove == false || m_type == Config.PieceType.K) return;
        m_isTipsShow = false;
        var px = m_target.transform.position.x - m_beginPos.x;
        var pz = m_target.transform.position.z - m_beginPos.z;
        var d = Math.Sqrt(px * px + pz * pz);
        var dx = Math.Abs(px);
        var dz = Math.Abs(pz);
        int xblock = (int)Math.Round(dx / Config.PieceWidth, 0);
        int zblock = (int)Math.Round(dz / Config.PieceWidth, 0);
        var dir = GetDirectByDeltaXZ(px, pz);
        Vector2 to = new Vector2(m_X + xblock * dir.x, m_Z + zblock * dir.y);
        if (d >= Config.MoveDistance && to.x <= 8 && to.x >= 1 && to.y >= 1 && to.y <= 8)//todo 不能直接套用
        {
            var tempEat = App.ChessLogic02.GetPiece(to.x - 1, to.y - 1);
            m_body = new object[] { m_X, m_Z, m_mediator.pieceData.color, to, tempEat };
            if (App.ChessLogic02.DoMove(new Vector2(m_X - 1, m_Z - 1), new Vector2(to.x - 1, to.y - 1)))
            {
                var piece = App.ChessLogic02.GetPiece((int)to.x - 1, (int)to.y - 1);//移动后的棋子
                var indexFrom = CoorToIndex((int)m_X, (int)m_Z);
                var indexTo = CoorToIndex((int)to.x, (int)to.y);

                ActiveInfo.Builder activeInfo = ActiveInfo.CreateBuilder();
                MoveInfo.Builder moveInfo = MoveInfo.CreateBuilder();
                moveInfo.SetFrom(indexFrom);
                moveInfo.SetTo(indexTo);
                moveInfo.SetUserId(m_UserProxy.GetPlayerId());
                activeInfo.SetIsCall(false);
                activeInfo.SetMoveInfo(moveInfo);
                m_mediator.NotityDragEnd(activeInfo);
                object[] args = new object[] { new Vector2(m_X, m_Z), to, new Vector2(-1, tempEat) };//0:from, 1:to, 2.x:兵生变类型 -1为没有， 2.y:吃棋信息
                ShowMove(args);
                StopFlash();
                App.SoundManager.PlaySoundClip(Config.Sound.DragSuccess);
            }
            else
            {
                SetPiecePos(m_X, m_Z);
                m_mediator.NotityDragEnd(null);//不能移动
                App.SoundManager.PlaySoundClip(Config.Sound.DragFail);
            }
        }
        else
        {
            SetPiecePos(m_X, m_Z);
            m_mediator.NotityDragEnd(null);//距离不够 驳回
        }
        m_target.SetActive(false);
    }

    public void OpenView()
    {
        var modelDrag = GameObject.Find("board").GetComponent<ModelDrag>();
    }

    /// <summary>
    /// AI走棋
    /// </summary>
    /// <param name="move"></param>
    public void AIMove(Move move)
    {
        var posMove = new Move(new Vector2(move.From.x + 1, move.From.y + 1), new Vector2(move.To.x + 1, move.To.y + 1));
        var tempEat = App.ChessLogic02.GetPiece(move.To.x, move.To.y);
        if (App.ChessLogic02.DoMove(move.From, move.To))
        {
            Debug.Log("Move success!");
            object[] args = new object[] { posMove.From, posMove.To, new Vector2(-1, tempEat) };//0:from, 1:to, 2.x:兵生变类型 -1为没有， 2.y:吃棋信息
            ShowMove(args);
        }
    }

    /// <summary>
    /// 自己走棋
    /// </summary>
    /// <param name="to"></param>
    private void ShowMove(object[] args)
    {
        var eatPiece = ((Vector2)args[2]).y;
        var to = (Vector2)args[1];
        var from = (Vector2)args[0];

        var dx = to.x - from.x;
        var dy = to.y - from.y;
        var angle = GetAngleByDeltaXY(dx, dy);
        var localAngle = (angle) % 360;

        if (m_TweenPlayer == null)
        {
            m_TweenPlayer = gameObject.AddComponent<TweenPlayer>();
        }

        if (m_ModelTween == null)
        {
            m_ModelTween = pieceModel.AddComponent<TweenPlayer>();
        }

        //有棋子播放攻击表现
        if (eatPiece >= 0)
        {
            Tween rotate_start = m_ModelTween.AddClip("rotate_start", Config.RotateTime);
            rotate_start.SetTweenType(TweenType.LocalRotation);
            rotate_start.SetTo(new Vector3(0, localAngle, 0));
            rotate_start.SetOnComplete(ShowAttack, args);
            m_ModelTween.StartPlay();
        }

        //没有棋子直接走
        else
        {
            Debug.Log("localAngel==============" + localAngle);
            Tween rotate_start = m_ModelTween.AddClip("rotate_start", Config.RotateTime);
            rotate_start.SetTweenType(TweenType.LocalRotation);
            rotate_start.SetTo(new Vector3(0, localAngle, 0));
            rotate_start.SetOnComplete(OnRotate1Complete, args);
            m_ModelTween.StartPlay();
        }
    }

    /// <summary>
    /// 开始走 转过去的回调
    /// </summary>
    /// <param name="args"></param>
    private void OnRotate1Complete(object args)
    {
        var temp = (object[])args;
        var to = (Vector2)temp[1];
        var from = (Vector2)temp[0];
        Tween move_pos = m_TweenPlayer.AddClip("move", Config.RotateTime);
        move_pos.SetTweenType(TweenType.LocalPosition);
        move_pos.SetTo(new Vector3((to.x - 1) * Config.PieceWidth, 0, (to.y - 1) * Config.PieceWidth));
        var steps = CalMoveSteps(from, to);
        move_pos.SetDuration(steps * Config.MoveStepTime);
        move_pos.SetOnComplete(OnRotate2, temp);
        m_TweenPlayer.StartPlay();
    }

    /// <summary>
    /// 走完了 转回来
    /// </summary>
    /// <param name="args"></param>
    private void OnRotate2(object args)
    {
        var temp = (object[])args;
        var to = (Vector2)temp[1];
        var from = (Vector2)temp[0];
        Tween rotate_start = m_ModelTween.AddClip("rotate_start", Config.RotateTime);
        rotate_start.SetTweenType(TweenType.LocalRotation);
        if (selfColor == Config.PieceColor.BLACK)
        {
            rotate_start.SetTo(new Vector3(0, 0, 0));
        }
        else
        {
            rotate_start.SetTo(new Vector3(0, 180, 0));
        }

        rotate_start.SetOnComplete(OnCompleteMove, temp);
        m_ModelTween.StartPlay();
    }

    /// <summary>
    /// 攻击表现
    /// </summary>
    /// <param name="to"></param>
    private void ShowAttack(object[] args)
    {
        var from = (Vector2)args[0];
        var to = (Vector2)args[1];

        switch (m_type)
        {
            case Config.PieceType.B:
                OnRemoteAttack("b_attack", args);
                break;
            case Config.PieceType.K:
                var KPoint = new Vector2((from.x + to.x) / 2, (from.y + to.y) / 2);
                OnMeleeAttack("b_attack", KPoint, args);
                break;
            case Config.PieceType.Q:
                OnRemoteAttack("b_attack", args);
                break;
            case Config.PieceType.N:
                var NPoint = new Vector2((from.x + to.x) / 2, (from.y + to.y) / 2);
                OnMeleeAttack("b_attack", NPoint, args);
                break;
            case Config.PieceType.P:
                var PPoint = new Vector2((from.x + to.x) / 2, (from.y + to.y) / 2);
                OnMeleeAttack("b_attack", PPoint, args);
                break;
            case Config.PieceType.R:
                OnRemoteAttack("b_attack", args);
                break;

        }
    }

    /// <summary>
    /// 远程攻击
    /// </summary>
    /// <param name="effectName"></param>
    /// <param name="to"></param>
    private void OnRemoteAttack(string effectName, object[] args)
    {
        var to = (Vector2)args[1];
        var from = (Vector2)args[0];
        if (m_pieceAnimator == null)
        {
            m_pieceAnimator = pieceModel.GetComponent<Animator>();
        }
        m_pieceAnimator.Play("Attack");

        EffectPlayer effectPlayer = App.EffectManager.LoadEffect(m_Attack, effectName);
        effectPlayer.IsOnce = true;

        TweenPlayer m_AttackPlayer = m_Attack.AddComponent<TweenPlayer>();
        var steps = CalMoveSteps(from, to);
        Tween attackTween = m_AttackPlayer.AddClip("attack", steps * Config.AttackStepTime);
        attackTween.SetTweenType(TweenType.LocalPosition);
        attackTween.SetDelayTime(0.8f);
        attackTween.SetOnComplete(OnRemoteAttackComplete, args);

        App.SoundManager.PlaySoundClip(Config.Sound.MagicAttack, 0.6f);

        var dx = Config.PieceWidth * (to.x - m_X);
        var dy = Config.PieceWidth * (to.y - m_Z);
        attackTween.SetTo(new Vector3(dx, m_Attack.transform.localPosition.y, dy));
        var angle = GetAngleByDeltaXY(dx, dy);
        var localAngle = (angle) % 360;
        m_AttackPlayer.gameObject.transform.rotation = Quaternion.Euler(0, localAngle, 0);
        //if (pieceColor == Config.PieceColor.WHITE)
        //{
        //    effectPlayer.LocalRotation = Quaternion.Euler(0, 180, 0);
        //}
        effectPlayer.enabled = true;
        m_AttackPlayer.StartPlay();
    }

    /// <summary>
    /// 近战攻击
    /// </summary>
    private void OnMeleeAttack(string effectName, Vector2 attackPoint, object[] args)
    {
        var from = (Vector2)args[0];
        var to = (Vector2)args[1];
        var steps = CalMoveSteps(from, to);
        Tween move_pos = m_TweenPlayer.AddClip("move", steps * Config.MoveStepTime);
        move_pos.SetTweenType(TweenType.LocalPosition);
        move_pos.SetTo(new Vector3((attackPoint.x - 1) * Config.PieceWidth, 0, (attackPoint.y - 1) * Config.PieceWidth));
        move_pos.SetOnComplete(OnMeleeMoveComplete, args);
        m_TweenPlayer.StartPlay();
    }

    private void OnRemoteAttackComplete(object[] args)
    {
        m_Attack.GetComponent<EffectPlayer>().enabled = false;
        InitAttackPoint();
        var to = (Vector2)args[1];
        var from = (Vector2)args[0];
        var item = GameObject.Find(to.x + "_" + to.y);
        item.GetComponent<PieceItem02>().BeAttached();

        Tween move_pos = m_TweenPlayer.AddClip("move", 1);
        move_pos.SetDelayTime(0.5f);
        var steps = CalMoveSteps(from, to);
        move_pos.SetDuration(steps * Config.MoveStepTime);
        move_pos.SetTweenType(TweenType.LocalPosition);
        move_pos.SetTo(new Vector3((to.x - 1) * Config.PieceWidth, 0, (to.y - 1) * Config.PieceWidth));
        move_pos.SetOnComplete(OnRotate2, args);
        m_TweenPlayer.StartPlay();
    }

    /// <summary>
    /// 近战走到攻击点
    /// </summary>
    /// <param name="args"></param>
    private void OnMeleeMoveComplete(object[] args)
    {
        if (m_pieceAnimator == null)
        {
            m_pieceAnimator = pieceModel.GetComponent<Animator>();
        }
        StartCoroutine(_OnMeleeStartAttack(args));
        var to = (Vector2)args[1];
        App.SoundManager.PlaySoundClip(Config.Sound.PhysAttack, 0.3f);
        Tween move_pos = m_TweenPlayer.AddClip("move", 0.5f);
        move_pos.SetDelayTime(1);
        move_pos.SetTweenType(TweenType.LocalPosition);
        move_pos.SetTo(new Vector3((to.x - 1) * Config.PieceWidth, 0, (to.y - 1) * Config.PieceWidth));
        move_pos.SetOnComplete(OnRotate2, args);
        m_TweenPlayer.StartPlay();
    }

    private IEnumerator _OnMeleeStartAttack(object[] args)
    {
        yield return new WaitForSeconds(0.4f);
        var to = (Vector2)args[1];
        var from = (Vector2)args[0];
        var item = GameObject.Find(to.x + "_" + to.y);
        item.GetComponent<PieceItem02>().BeAttached();
        m_pieceAnimator.Play("Attack");
    }

    //private void Example()
    //{
    //    int x = 0;
    //    int y = 0;
    //    int index = 0;

    //    if (index >= 1 && index <= 64)
    //    {

    //    }
    //    if(!(index & 0x88))
    //}

    private void OnCompleteMove(object args)
    {
        var temp = (object[])args;
        canMove = false;
        Vector2[] body = new Vector2[] { (Vector2)temp[0], (Vector2)temp[1], (Vector2)temp[2] };
        m_mediator.NotifyMoveEnd(body);
        if (isPVE == false)
        {
            m_mediator.NotifyEndTurn(1);
            Debug.Log("isPVE:OnCompleteMove!");
        }
        else
        {
            m_mediator.NotifyPVEEndTurn();
        }
    }

    private Vector2 GetDirectByDeltaXZ(float px, float pz)
    {
        var angle = (Math.Atan2(pz, px) * 180 / Math.PI + 360 + 22.5) % 360;
        int[] directAngles = { 0, 45, 90, 135, 180, 225, 270, 315, 360 };
        Vector2[] directs =
        {
            new Vector2(1, 0),new Vector2(1, 1),new Vector2(0, 1),new Vector2(-1, 1),
            new Vector2(-1, 0),new Vector2(-1, -1),new Vector2(0, -1),new Vector2(1, -1)
        };
        var dId = 0;
        for (int i = 1; i <= directAngles.Length; i++)
        {
            if (angle < directAngles[i])
            {
                dId = i - 1;
                break;
            }
        }
        return directs[dId];
    }

    private float GetAngleByDeltaXY(float x, float y)
    {
        return (float)(Math.Atan2(x, y) * 180 / Math.PI + 360) % 360;
    }

    /// <summary>
    /// 其他人走棋
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    public void DoMove(Vector2 from, Vector2 to, Vector2 type)
    {
        Debug.Log("from== " + from + "  to== " + to + "////// m_X== " + m_X + " m_Y== " + m_Z);
        if (from.x == m_X && from.y == m_Z)
        {
            var dx = to.x - from.x;
            var dy = to.y - from.y;
            var angle = GetAngleByDeltaXY(dx, dy);
            var localAngle = (angle) % 360;
            var targetPiece = App.ChessLogic02.GetPiece(to.x - 1, to.y - 1);//目标位置棋子
            if (App.ChessLogic02.DoMove(new Vector2(from.x - 1, from.y - 1), new Vector2(to.x - 1, to.y - 1)))
            {
                if (m_TweenPlayer == null)
                {
                    m_TweenPlayer = gameObject.AddComponent<TweenPlayer>();
                }

                if (m_ModelTween == null)
                {
                    m_ModelTween = pieceModel.AddComponent<TweenPlayer>();
                }

                var args = new object[] { from, to, new Vector2(type.x, targetPiece) };
                //攻击表现
                if (targetPiece > -1)
                {
                    ShowAttack(args);
                }

                //正常走棋
                else
                {
                    Tween rotate_start = m_ModelTween.AddClip("rotate_start", 1);
                    rotate_start.SetTweenType(TweenType.LocalRotation);
                    rotate_start.SetTo(new Vector3(0, localAngle, 0));
                    rotate_start.SetOnComplete(OnOtherRotate1Complete, args);
                    m_ModelTween.StartPlay();
                }
            }
            else
            {
                App.UIManager.OpenPanel("MessagePanel", "非法移动！from == " + from + "to == "+to + "type "+type);
                Debug.Log("非法移动！！ On Other");
            }
        }
    }

    private void OnOtherRotate1Complete(object args)
    {
        var temp = (object[])args;
        var from = (Vector2)temp[0];
        var to = (Vector2)temp[1];
        var type = ((Vector2)temp[2]);
        Tween move_pos = m_TweenPlayer.AddClip("move", 2);
        move_pos.SetTweenType(TweenType.LocalPosition);
        move_pos.SetTo(new Vector3((to.x - 1) * Config.PieceWidth, 0, (to.y - 1) * Config.PieceWidth));
        var steps = CalMoveSteps(from, to);
        move_pos.SetDuration(steps * Config.MoveStepTime);
        move_pos.SetOnComplete(OnOtherRotate2, temp);
        m_TweenPlayer.StartPlay();
        m_otherBody = new Vector2[] { from, to, type };
        Debug.Log("正常移动！！ fromx: " + from.x + "fromy" + from.y);
    }

    /// <summary>
    /// 走完了 转回来
    /// </summary>
    /// <param name="args"></param>
    private void OnOtherRotate2(object args)
    {
        var temp = (object[])args;
        var to = (Vector2)temp[1];
        var from = (Vector2)temp[0];
        Tween rotate_start = m_ModelTween.AddClip("rotate_start", Config.RotateTime);
        rotate_start.SetTweenType(TweenType.LocalRotation);
        if (selfColor == Config.PieceColor.BLACK)
        {
            rotate_start.SetTo(new Vector3(0, 0, 0));
        }
        else
        {
            rotate_start.SetTo(new Vector3(0, 180, 0));
        }
        rotate_start.SetOnComplete(OnOtherMoveComplete, temp);
        m_ModelTween.StartPlay();
    }

    private void OnOtherMoveComplete(object args)
    {
        m_mediator.NotifyMoveEnd(m_otherBody);
        if (isPVE == false)
        {
            m_mediator.NotifyEndTurn(1);
        }
    }

    private float CalMoveSteps(Vector2 from, Vector2 to)
    {
        var stepX = Math.Abs(from.x - to.x);
        var stepY = Math.Abs(from.y - to.y);
        if (stepX > stepY) return stepX;
        else
        {
            return stepY;
        }
    }

    ///坐标转index 且棋盘翻转
    private int CoorToIndex(int x, int y)
    {
        int index = 65 - (y * Config.Board.MaxX - x + 1);
        return index;
    }

    public void StopIdle()
    {
        if (m_pieceAnimator == null)
        {
            m_pieceAnimator = pieceModel.GetComponent<Animator>();
        }
        m_pieceAnimator.Play("New State");
    }

    public void StopFlash()
    {
        if(m_type != Config.PieceType.K)
        {
            pieceModel.GetComponent<TweenPlayer>().enabled = false;
        }
        foreach (var renderer in pieceModel.GetComponentsInChildren<Renderer>())
        {
            foreach (var material in renderer.materials)
            {
                material.color = Color.white;
            }
        }
    }

    public void OnPieceClick(Vector2 point)
    {
        if(selfColor == pieceColor)
        {
            if (canMove == true)
            {
                m_mediator.NotifyShowTips(point);
                if (m_pieceAnimator == null)
                {
                    m_pieceAnimator = pieceModel.GetComponent<Animator>();
                }
                m_pieceAnimator.Play("Idle_1");
            }
            else
            {
                m_mediator.NotifyShowRedTips(point);
            }
        }
        else
        {
            m_mediator.NotifyShowRedTips(point);
        }
    }
}
