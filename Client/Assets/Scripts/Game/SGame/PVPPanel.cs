using Framework;
using System;
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
    private PVPPanelMediator m_mediator;
    private UserDataProxy m_proxy;//todo

    public void InitView(GameObject gameObject)
    {
        m_mediator = new PVPPanelMediator(this);
        m_proxy = new UserDataProxy();
        App.Facade.RegisterMediator(m_mediator);
        App.Facade.RegisterProxy(m_proxy);
        m_piece = GameObject.Find("m_Chess");
        m_qizi = GameObject.Find("qizi");
        m_ready = gameObject.transform.Find("Container/m_Ready").gameObject.GetComponent<Button>();
        m_ready.onClick.AddListener(OnReadyClick);
        m_mediator.InitBoardData();
    }

    public void OpenView()
    {
        InitChessBoard();
    }

    /// <summary>
    /// 初始化棋盘
    /// </summary>
    private void InitChessBoard()
    {
        foreach(Piece piece in m_mediator.pieces)
        {
            GameObject temp = GameObject.Instantiate(m_piece);//todo
            temp.transform.parent = m_qizi.transform;
            PieceItem pieceItem = new PieceItem();
            pieceItem.InitView(temp, piece);
        }
    }

    private void OnReadyClick()
    {

    }

    public void CloseView()
    {

    }

    public void DestroyView()
    {

    }
}

