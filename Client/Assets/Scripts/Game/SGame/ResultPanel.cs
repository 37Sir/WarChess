using com.zyd.common.proto.client;
using Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class ResultPanel
{
    private Button m_Back;
    private GameObject m_WinState;
    private GameObject m_LoseState;
    private GameObject m_DrawState;

    private GameObject m_object;
    private ResultPanelMediator m_mediator;

    public void InitView(GameObject gameObject)
    {
        InitUIBinder(gameObject);
        m_Back.onClick.AddListener(OnBackClick);

        m_object = gameObject;
        m_mediator = new ResultPanelMediator(this);
        App.Facade.RegisterMediator(m_mediator);
    }

    private void InitUIBinder(GameObject gameObject)
    {
        m_Back = gameObject.transform.Find("Container/m_Back").GetComponent<Button>();
        m_WinState = gameObject.transform.Find("Container/m_ResultState/m_WinState").gameObject;
        m_LoseState = gameObject.transform.Find("Container/m_ResultState/m_LoseState").gameObject;
        m_DrawState = gameObject.transform.Find("Container/m_ResultState/m_DrawState").gameObject;
    }

    public void OpenView(object intent)
    {
        if (intent != null)
        {
            var state = (Config.GameResult)intent;
            if(state == Config.GameResult.LOSE)
            {
                m_WinState.SetActive(false);
                m_DrawState.SetActive(false);
                m_LoseState.SetActive(true);
            }
            else if(state == Config.GameResult.DRAW)
            {
                m_WinState.SetActive(false);
                m_DrawState.SetActive(true);
                m_LoseState.SetActive(false);
            }
            else
            {
                m_WinState.SetActive(true);
                m_DrawState.SetActive(false);
                m_LoseState.SetActive(false);
            }
        }
        App.SoundManager.PlaySoundClip(Config.Sound.GameWin);
    }

    public void CloseView()
    {

    }

    public void DestroyView()
    {
        App.Facade.RemoveMediator(m_mediator.MediatorName);
    }

    #region Private Method

    #endregion

    #region OnClick
    private void OnBackClick()
    {
        //App.UIManager.BackPanel();
        App.Facade.RemoveMediator("PVPPanelMediator");
        App.NSceneManager.LoadScene("SLobby");
    }
    #endregion 
}

