using Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class PieceItem:MonoBehaviour
{
    private GameObject m_gameObject;
    private GameObject m_target;
    private PieceItemMediator m_mediator;
    private string m_name;
    private PVPProxy m_pvpProxy;
    private Vector3 m_beginPos;//初始位置
    public Config.PieceColor selfColor;
    private bool m_isTipsShow = false;
    private int m_type;//棋子的类型
    public float m_X;
    public float m_Z;
    public bool isDead = false;
    public bool isPVE = false;
    public bool isReborn = false;
    public GameObject pieceModel;
    private TweenPlayer m_TweenPlayer;
    private object[] m_body;
    private Vector2[] m_otherBody;
    private Vector2 m_undoFrom;

    public void InitView(GameObject gameObject, Piece pieceData, bool pve = false)
    {
        isPVE = pve;
        m_gameObject = gameObject;
        m_gameObject.transform.localRotation = Quaternion.Euler(0, 180, 0);
        m_pvpProxy = App.Facade.RetrieveProxy("PVPProxy") as PVPProxy;
        selfColor = m_pvpProxy.GetSelfColor();
        InitPieceShow(pieceData);
        m_name = m_X + "_" + m_Z;
        m_mediator = new PieceItemMediator(this, m_name);
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

    /// <summary>
    /// 被打了
    /// </summary>
    public void BeAttached()
    {
        App.ObjectPoolManager.Release("m_target", m_target);
        if (m_mediator.pieceData.type == Config.PieceType.K)
        {
            object loseColor = m_mediator.pieceData.color;
            m_mediator.NotityGameOver(loseColor);
        }
        isDead = true;
        gameObject.SetActive(false);
        OnDestroy();
    }

    private void OnDestroy()
    {
        App.Facade.RemoveMediator(m_mediator.MediatorName);
        Destroy(gameObject);
        Destroy(this);
    }

    public void OnGameOver()
    {
        OnDestroy();
    }

    /// <summary>
    /// 拖动开始
    /// </summary>
    public void OnDragBegin()
    {
        m_beginPos = transform.position;
        m_target.SetActive(true);
    }

    /// <summary>
    /// 拖动中
    /// </summary>
    /// <param name="currentPosition"></param>
    public void OnDrag(Vector3 currentPosition)
    {
        float tempZ = 0;
        if(selfColor == Config.PieceColor.WHITE)
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
        if(d >= Config.TipsDistance && m_isTipsShow == false)
        {
            m_isTipsShow = true;
            m_mediator.NotifyDragTips(new Vector2(m_X, m_Z));
        }        
    }

    /// <summary>
    /// 拖动结束
    /// </summary>
    public void OnDragEnd()
    {
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
        if (d >= Config.MoveDistance)//todo 不能直接套用
        {
            var tempEat = App.ChessLogic.GetPiece(to.x - 1, to.y -1);
            m_body = new object[]{ m_X, m_Z, m_mediator.pieceData.color, to, tempEat};
            if (App.ChessLogic.DoMove(new Vector2(m_X - 1, m_Z - 1), new Vector2(to.x - 1, to.y - 1)))
            {
                var piece = App.ChessLogic.GetPiece((int)to.x - 1, (int)to.y - 1);//移动后的棋子
                if(piece % 10 == (int)Config.PieceType.P && ((int)to.y == 1 || (int)to.y == 8))
                {
                    m_mediator.NotityPPromote(m_body);
                    ShowMove(to);
                }
                else
                {
                    m_mediator.NotityDragEnd(m_body);
                    ShowMove(to);
                }              
            }
            else
            {
                SetPiecePos(m_X, m_Z);
                m_mediator.NotityDragEnd(null);//不能移动
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
    /// 自己走棋
    /// </summary>
    /// <param name="to"></param>
    private void ShowMove(Vector2 to, bool isPromote = false)
    {
        if(m_TweenPlayer == null)
        {
            m_TweenPlayer = gameObject.AddComponent<TweenPlayer>();
        }
        Tween move_pos = m_TweenPlayer.AddClip("move", 2);
        move_pos.SetTweenType(TweenType.LocalPosition);
        move_pos.SetTo(new Vector3((to.x - 1) * Config.PieceWidth, 0, (to.y - 1) * Config.PieceWidth));
        move_pos.SetDuration(1);
        move_pos.SetOnComplete(OnCompleteMove);
        m_TweenPlayer.StartPlay();
    }

    private void OnCompleteMove()
    {
        Vector2[] body = new Vector2[] { new Vector2((float)m_body[0], (float)m_body[1]), (Vector2)m_body[3], new Vector2(m_type, 0) };
        m_mediator.NotifyMoveEnd(body);
        if (isPVE == false)
        {
            m_mediator.NotifyEndTurn();
            Debug.Log("isPVE:OnCompleteMove!");
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
        for(int i = 1; i <= directAngles.Length; i++)
        {
            if(angle < directAngles[i])
            {
                dId = i - 1;
                break;
            }
        }
        return directs[dId];
    }

    /// <summary>
    /// 其他人走棋
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    public void DoMove(Vector2 from, Vector2 to, Vector2 type)
    {
        if(from.x == m_X && from.y == m_Z)
        {
            if (App.ChessLogic.DoMove(new Vector2(from.x - 1, from.y - 1), new Vector2(to.x - 1, to.y - 1)))
            {
                if (m_TweenPlayer == null)
                {
                    m_TweenPlayer = gameObject.AddComponent<TweenPlayer>();
                }
                Tween move_pos = m_TweenPlayer.AddClip("move", 2);
                move_pos.SetTweenType(TweenType.LocalPosition);
                move_pos.SetTo(new Vector3((to.x - 1) * Config.PieceWidth, 0, (to.y - 1) * Config.PieceWidth));
                move_pos.SetDuration(2);
                move_pos.SetOnComplete(OnOtherMoveComplete);
                m_TweenPlayer.StartPlay();
                m_otherBody = new Vector2[] { from, to, type };
                Debug.Log("正常移动！！ fromx: " + from.x + "fromy" + from.y);
            }
            else
            {
                Debug.Log("非法移动！！ On Other");
            }
        }
    }

    private void OnOtherMoveComplete()
    {
        m_mediator.NotifyMoveEnd(m_otherBody);
        if (isPVE == false)
        {
            m_mediator.NotifyEndTurn();
        }
    }

    /// <summary>
    /// 兵生变
    /// </summary>
    /// <param name="type"></param>
    public void OnPromoted(int type)
    {
        App.ChessLogic.PPromoted(new Vector2Int((int)m_X - 1, (int)m_Z - 1), type);
        string pieceName = "";
        if(selfColor == Config.PieceColor.BLACK)
        {
            pieceName = "Black_";
        }
        else
        {
            pieceName = "White_";
        }
        switch ((Config.PieceType)type)
        {
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
        }
        Destroy(pieceModel);
        App.ObjectPoolManager.Instantiate(pieceName, (GameObject obj) =>
        {
            obj.SetActive(true);
            obj.transform.parent = transform;
            pieceModel = obj;
            m_mediator.pieceData.type = (Config.PieceType)type;
        });       
    }

    public void OnUndo(Vector2 from)
    {
        m_undoFrom = from;
        if (m_TweenPlayer == null)
        {
            m_TweenPlayer = gameObject.AddComponent<TweenPlayer>();
        }
        Tween move_pos = m_TweenPlayer.AddClip("undo", 2);
        move_pos.SetTweenType(TweenType.LocalPosition);
        move_pos.SetTo(new Vector3((from.x - 1) * Config.PieceWidth, 0, (from.y - 1) * Config.PieceWidth));
        move_pos.SetDuration(1);
        move_pos.SetOnComplete(OnCompleteUndo);
        m_TweenPlayer.StartPlay();
    }

    public void OnCompleteUndo()
    {
        SetPiecePos(m_undoFrom.x, m_undoFrom.y);
        Debug.Log("UndoOnComplete!~!!!");
        if(isPVE == false)
        {
            Debug.Log("NotifyUndoTweenEnd!~!!!");
            m_mediator.NotifyUndoTweenEnd();
        }
    }
}
