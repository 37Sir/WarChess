using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Framework;

/// <summary>
/// 动画播放
/// </summary>
public class TweenPlayer : MonoBehaviour {

    #region Private Field
    private Dictionary<string, Tween> m_tweens = new Dictionary<string, Tween>();       //正在播放的动画
    private Dictionary<string, Tween> m_loadingTweens = new Dictionary<string, Tween>();//等待播放的动画
    private string m_lastTween = null;

    #endregion Private Field

    #region Public Field

    public TweenPack m_TweenPack;   //动画包
    public bool playOnAwake = false;

    public TweenPack TweenPack
    {
        get
        {
            return m_TweenPack;
        }
        set
        {
            m_TweenPack = value;
        }
    }

    #endregion Public Field

    #region Public Method

    /// <summary>
    /// 播放一个动画片段
    /// </summary>
    /// <param name="clipName">动画片段的名字</param>
    /// <param name="isCover">是否覆盖当前动画</param>
    public void PlayOne(string clipName, bool isCover = true)
    {
        if(isCover == true)
        {
            ClearAll();
        }

        if (m_TweenPack != null)
        {
            for (int i = 0; i < m_TweenPack.Attributes.Count; i++)
            {
                if(m_TweenPack.Attributes[i].TweenName == clipName)
                {
                    Play(m_TweenPack.Attributes[i]);
                }            
            }
        }
    }

    /// <summary>
    /// 设置动画包
    /// </summary>
    /// <param name="packName">动画包的名字</param>
    /// <param name="isAuto">是否自动播放</param>
    public void SetTweenPack(string packName, bool isAuto = true)//todo
    {
        enabled = false;
        m_loadingTweens.Clear();
        string tweenPackPath = Config.TweenPacksLoadRoot + "/" + packName;
        App.ResourceManager.LoadResource<TweenPack>(tweenPackPath, (TweenPack tweenPack) =>
        {
            if (tweenPack != null) 
            {
                m_TweenPack = tweenPack;
                foreach(TweenPack.Attribute attribute in m_TweenPack.Attributes)
                {
                    PrePlay(attribute);
                }
                enabled = isAuto;
            }
        });
    }

    /// <summary>
    /// 获取动画片段
    /// </summary>
    /// <param name="clipName">片段的名字</param>
    /// <returns></returns>
    public Tween GetClipTween(string clipName)
    {
        if (m_loadingTweens.ContainsKey(clipName))
        {
            return m_loadingTweens[clipName];
        }
        return null;
    }

    /// <summary>
    /// 播放等待列表中的动画
    /// </summary>
    public void StartPlay()
    {
        foreach(Tween tween in m_loadingTweens.Values)
        {
            tween.Play();
        }
        m_loadingTweens.Clear();
    }

    /// <summary>
    /// 暂停
    /// </summary>
    public void Pause()
    {
        foreach (Tween tween in m_tweens.Values)
        {
            tween.Pause();
        }
    }

    /// <summary>
    /// 恢复播放
    /// </summary>
    public void Resume()
    {
        foreach (Tween tween in m_tweens.Values)
        {
            tween.Resume();
        }
    }

    /// <summary>
    /// TweenPack播完的回调
    /// </summary>
    /// <param name="super"></param>
    /// <param name="call"></param>
    /// <param name="name"></param>
    public void SetOnComplete()//todo
    {
        //if (onComplete != null)
        //{
        //    onComplete.Release();
        //}
        //onComplete = new TweenLuaCallback(super, call);

        string name = GetLastTweenName(m_TweenPack.Attributes);
        if (m_loadingTweens.ContainsKey(name))
        {
            m_loadingTweens[name].isLast = true;
            m_loadingTweens[name].SetTweenPackCompleteDelegate(OnComplete);
        }
    }

    /// <summary>
    /// 移除回调
    /// </summary>
    /// <param name="super"></param>
    /// <param name="call"></param>
    public void RemoveOnComplete()
    {
        //if (onComplete != null)
        //{
        //    onComplete.Release();
        //}
        //onComplete = null;
    }

    /// <summary>
    /// 设置动画片段播完回调
    /// </summary>
    /// <param name="super"></param>
    /// <param name="call">回调函数</param>
    /// <param name="name">动画片段的名字</param>
    public void SetClipOnComplete(string name, Tween.TweenClipCompleteDelegate call)
    {
        if (m_tweens.ContainsKey(name))
        {
            m_tweens[name].SetOnComplete(call);
        }
    }

    /// <summary>
    /// 移除动画回调
    /// </summary>
    /// <param name="name">动画片段的名字</param>
    public void RemoveClipOnComplete(string name)
    {
        if (m_tweens.ContainsKey(name))
        {
            m_tweens[name].RemoveOnComplete();
        }
    }

    /// <summary>
    /// 添加动画片段
    /// </summary>
    /// <param name="clipName">动画名</param>
    /// <param name="to">目标点</param>
    /// <param name="duration">持续时间</param>
    /// <returns></returns>
    public Tween AddClip(string clipName, float duration)
    {
        Tween tween = new Tween();
        tween.Owner = gameObject;

        tween.SetDuration(duration);
        m_loadingTweens.Add(clipName, tween);
        return tween;
    }

    /// <summary>
    /// 停止并清除动画
    /// </summary>
    public void ClearAll()
    {
        foreach (Tween tween in m_tweens.Values)
        {
            tween.Stop();
        }
        m_loadingTweens.Clear();
        m_tweens.Clear();
    }

    #endregion Public Method

    #region MonoBehaviour Method

    private void Awake()
    {
        if (m_TweenPack != null)
        {
            for (int i = 0; i < m_TweenPack.Attributes.Count; i++)
            {
                PrePlay(m_TweenPack.Attributes[i]);
            }
        }
        enabled = playOnAwake;
    }

    private void OnEnable()
    {
        if (m_TweenPack != null)
        {
            for (int i = 0; i < m_TweenPack.Attributes.Count; i++)
            {
                Play(m_TweenPack.Attributes[i]);
            }
        }
    }

    private void OnDisable()
    {
        Stop();
    }

    #endregion MonoBehaviour Method

    #region Private Method

    private Tween PrePlay(TweenPack.Attribute attribute)
    {
        Tween tween = new Tween();
        tween.Owner = gameObject;

        tween.isNeedFrom = attribute.isNeedFrom;

        tween.SetTweenType(attribute.TweenType);
        tween.SetEaseType(attribute.EaseType);
        tween.SetTo(attribute.To);
        tween.SetColor(attribute.Color);
        tween.SetFade(attribute.Fade);
        tween.SetToText(attribute.ToText);
        tween.SetDuration(attribute.Duration);
        tween.SetDelayTime(attribute.DelayTime);
        tween.SetLoop(attribute.Loop);
        tween.SetLoopType(attribute.LoopType);
        tween.SetPosFrom(attribute.FromPos);
        tween.SetColorFrom(attribute.FromColor);
        tween.SetFadeFrom(attribute.FromFade);

        m_loadingTweens.Add(attribute.TweenName, tween);
        return tween;
    }

    /// <summary>
    /// 播放一个动画片段
    /// </summary>
    /// <param name="attribute">动画的参数</param>
    private void Play(TweenPack.Attribute attribute)
    {
        if (m_loadingTweens.ContainsKey(attribute.TweenName))
        {
            m_loadingTweens[attribute.TweenName].Play();
            m_tweens.Add(attribute.TweenName, m_loadingTweens[attribute.TweenName]);
        }
        else
        {
            Tween tween = PrePlay(attribute);
            tween.Play();
            m_tweens.Add(attribute.TweenName, tween);
        }
    }

    /// <summary>
    /// 停止播放
    /// </summary>
    private void Stop()
    {
        foreach(Tween tween in m_tweens.Values)
        {
            tween.Stop();
        }
        m_tweens.Clear();
    }

    /// <summary>
    /// 最后播完的动画的名字
    /// </summary>
    /// <returns></returns>
    private string GetLastTweenName(List<TweenPack.Attribute> attributes)
    {
        float max = 0;
        string lastTween = null;
        foreach(TweenPack.Attribute attribute in attributes)
        {
            float time = attribute.DelayTime + attribute.Duration;
            if(max <= time)
            {
                max = time;
                lastTween = attribute.TweenName;
            }
        }
        return lastTween;
    }

    #endregion Private Method

    /// <summary>
    /// 动画播完回调
    /// </summary>
    protected void OnComplete()
    {
        //if (onComplete != null)
        //{
        //    onComplete.Call();
        //}
    }
}
