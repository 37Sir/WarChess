using Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class PieceItem
{
    private GameObject m_gameObject;
    private PieceItemMediator m_mediator;
    private TextMesh m_Type;//棋子类型
    public void InitView(GameObject gameObject, Piece pieceData)
    {
        m_mediator = new PieceItemMediator(this);
        App.Facade.RegisterMediator(m_mediator);
        m_mediator.InitPieceData(pieceData);
        m_Type = gameObject.transform.Find("m_Type").GetComponent<TextMesh>();
        InitPieceShow(pieceData);
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
        m_gameObject.transform.position = new Vector3((pieceData.x - 1) * Config.PieceWidth, (pieceData.y - 1) * Config.PieceWidth);

    }

    public void OpenView()
    {

    }
}
