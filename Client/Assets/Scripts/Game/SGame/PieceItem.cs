﻿using Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class PieceItem:MonoBehaviour
{
    private GameObject m_gameObject;
    private PieceItemMediator m_mediator;
    private string m_name;
    private PVPProxy m_pvpProxy;
    private TextMesh m_Type;//棋子类型
    private GameObject m_Model;//棋子模型
    private Vector3 m_beginPos;//初始位置
    public Config.PieceColor selfColor;
    private bool m_isTipsShow = false;
    public float m_X;
    public float m_Z;
    public bool isDead = false;
    public bool isPVE = false;

    public void InitView(GameObject gameObject, Piece pieceData, bool pve = false)
    {
        isPVE = pve;
        m_gameObject = gameObject;
        m_Type = gameObject.transform.Find("m_Type").GetComponent<TextMesh>();
        m_Model = gameObject.transform.Find("m_Model").gameObject;
        m_pvpProxy = App.Facade.RetrieveProxy("PVPProxy") as PVPProxy;
        InitPieceShow(pieceData);
        m_name = m_X + "_" + m_Z;
        m_mediator = new PieceItemMediator(this, m_name);
        App.Facade.RegisterMediator(m_mediator);       
        m_mediator.InitPieceData(pieceData);
       
        OpenView();
    }

    private void InitPieceShow(Piece pieceData)
    {
        switch (pieceData.type)
        {
            case Config.PieceType.P:
                m_Type.text = "兵";
                break;
            case Config.PieceType.N:
                m_Type.text = "马";
                break;
            case Config.PieceType.B:
                m_Type.text = "象";
                break;
            case Config.PieceType.R:
                m_Type.text = "车";
                break;
            case Config.PieceType.Q:
                m_Type.text = "后";
                break;
            case Config.PieceType.K:
                m_Type.text = "王";
                break;
        }
        if(pieceData.color == Config.PieceColor.BLACK)
        {
            var mesh = m_Model.GetComponent<MeshRenderer>();
            mesh.material.color = Color.black;
            m_Type.color = Color.white;
        }
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
    }

    public void SetPiecePos(float x, float z)
    {
        m_gameObject.transform.localPosition = new Vector3((x - 1) * Config.PieceWidth, 0, (z - 1) * Config.PieceWidth);
        m_X = x;
        m_Z = z;
    }

    /// <summary>
    /// 被打了
    /// </summary>
    public void BeAttached()
    {
        if(m_mediator.pieceData.type == Config.PieceType.K)
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
    }

    /// <summary>
    /// 拖动中
    /// </summary>
    /// <param name="currentPosition"></param>
    public void OnDrag(Vector3 currentPosition)
    {
        transform.position = currentPosition;
        var px = transform.position.x - m_beginPos.x;
        var pz = transform.position.z - m_beginPos.z;
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
        var px = transform.position.x - m_beginPos.x;
        var pz = transform.position.z - m_beginPos.z;
        var d = Math.Sqrt(px * px + pz * pz);
        var dx = Math.Abs(px);
        var dz = Math.Abs(pz);
        int xblock = (int)Math.Round(dx / Config.PieceWidth, 0);
        int zblock = (int)Math.Round(dz / Config.PieceWidth, 0);
        var dir = GetDirectByDeltaXZ(px, pz);
        Vector2 to = new Vector2(m_X + xblock * dir.x, m_Z + zblock * dir.y);     
        if (d >= Config.MoveDistance)//todo 不能直接套用
        {
            object[] body = { m_X, m_Z, m_mediator.pieceData.color, to };
            if (App.ChessLogic.DoMove(new Vector2(m_X - 1, m_Z - 1), new Vector2(to.x - 1, to.y - 1)))
            {
                var piece = App.ChessLogic.GetPiece((int)to.x - 1, (int)to.y - 1);//移动后的棋子
                if(piece % 10 == (int)Config.PieceType.P && ((int)to.y == 1 || (int)to.y == 8))
                {
                    m_mediator.NotityPPromote(body);
                }
                else
                {
                    m_mediator.NotityDragEnd(body);
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
    }

    public void OpenView()
    {
        var modelDrag = GameObject.Find("board").GetComponent<ModelDrag>();
        //modelDrag.BindDrag(m_gameObject, OnDrag);
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
                m_mediator.NotifyMoveEnd(new Vector2[] {from, to, type});
                if (isPVE == false)
                {
                    m_mediator.NotifyEndTurn();
                }              
                Debug.Log("正常移动！！ fromx: " + from.x + "fromy" + from.y);
            }
            else
            {
                Debug.Log("非法移动！！ On Other");
            }
        }
    }

    public void OnPromoted(int type)
    {
        App.ChessLogic.PPromoted(new Vector2Int((int)m_X - 1, (int)m_Z - 1), type);
        switch ((Config.PieceType)type)
        {
            case Config.PieceType.N:
                m_Type.text = "马";
                break;
            case Config.PieceType.B:
                m_Type.text = "象";
                break;
            case Config.PieceType.R:
                m_Type.text = "车";
                break;
            case Config.PieceType.Q:
                m_Type.text = "后";
                break;
        }
    }
}
