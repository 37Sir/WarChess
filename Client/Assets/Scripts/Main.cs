﻿using Framework;
using PureMVC.Patterns.Facade;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour {

    private void Awake()
    {
        Application.targetFrameRate = 60;//60 FPS
        Init();
    }

    /// <summary>
    /// 初始化
    /// </summary>
    void Init()
    {
        DontDestroyOnLoad(gameObject);  //防止销毁自己
        AddManager();
        App.NetworkManager.Init(true);
        App.EffectManager.Init();
        App.NSceneManager.LoadScene("SLogin");
    }

    void AddManager()
    {
        App.Instance.AddManager<NetworkManager>(ManagerName.Network);
        App.Instance.AddManager<NSceneManager>(ManagerName.Scene);
        App.Instance.AddManager<ObjectPoolManager>(ManagerName.ObjectPool);
        App.Instance.AddManager<UIManager>(ManagerName.UI);
        App.Instance.AddManager<ResourceManager>(ManagerName.Resource);
        App.Instance.AddManager<EffectManager>(ManagerName.Effect);
        App.Instance.AddManager<SoundManager>(ManagerName.Sound);
    }

    void InitManager()
    {
    }
}
